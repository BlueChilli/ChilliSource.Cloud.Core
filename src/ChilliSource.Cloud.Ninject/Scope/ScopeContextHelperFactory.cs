using ChilliSource.Cloud.DependencyInjection;
using Ninject;
using Ninject.Extensions.ChildKernel;
using Ninject.Extensions.ContextPreservation;
using Ninject.Extensions.NamedScope;
using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Ninject
{
    /// <summary>
    /// Allows the creation of a scope context Factory
    /// </summary>
    public class ScopeContextHelperFactory
    {
        /// <summary>
        /// Delegate to bind services in the kernel for a specific scopeAction. <br/>
        ///  e.g. kernel.Bind&lt;MyServiceA&gt;().ToSelf().InScopeAction(scopeAction);
        /// </summary>
        /// <param name="kernel">A kernel</param>
        /// <param name="scopeAction">A scope action</param>               
        public delegate void RegisterServices(IKernel kernel, Action<IBindingSyntax> scopeAction);

        private ScopeContextHelperFactory() { }

        /// <summary>
        /// Creates a scope context factory
        /// </summary>
        /// <param name="defaultKernel">A default kernel</param>
        /// <param name="kernelBinder">A kernel binder delegate</param>
        /// <returns>Returns a scope context factory</returns>
        public static IScopeContextHelper Create(IKernel defaultKernel, Action<IKernelBinderHelper> kernelBinder)
        {
            return new ScopeContextHelper(defaultKernel, kernelBinder);
        }
    }

    internal class ScopeContextHelper : IScopeContextHelper
    {
        ChildKernel _contextKernel;
        string _scopeName;
        ScopeContextKernelBinderHelper _binderHelper;

        internal ScopeContextHelper(IKernel defaultKernel, Action<IKernelBinderHelper> serviceBinder)
        {
            _scopeName = typeof(ScopeContextHelper).FullName;
            _contextKernel = new ChildKernel(defaultKernel, new NinjectSettings() { AllowNullInjection = true });

            _contextKernel.GetBindings(typeof(IKernel)).ToList().ForEach(b =>
                _contextKernel.RemoveBinding(b));
            _contextKernel.Bind<IKernel>().ToMethod(ctx => _contextKernel);

            _contextKernel.Bind<KernelScopeContextSingleton>().ToSelf().InNamedScope(_scopeName);

            _binderHelper = new ScopeContextKernelBinderHelper(_contextKernel, (syntax) => NamedScopeExtensionMethods.InNamedScope((dynamic)syntax, _scopeName));
            if (serviceBinder != null)
            {
                serviceBinder(_binderHelper);
            }

            _contextKernel.GetBindings(typeof(IResolver)).ToList().ForEach(b =>
                _contextKernel.RemoveBinding(b));
            _contextKernel.Bind<IResolver>().ToMethod(ctx => new DefaultDependecyResolver(ctx.GetContextPreservingResolutionRoot())).InNamedScope(_scopeName);
        }

        public IScopeContext CreateScope()
        {
            return new ScopeContext(_contextKernel, _scopeName, _binderHelper);
        }

        bool disposed = false;
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            _contextKernel.Dispose();
        }
    }

    internal class ScopeContext : IScopeContext
    {
        Dictionary<Type, object> _Values = new Dictionary<Type, object>();
        ScopeContextKernelBinderHelper _binderHelper;
        NamedScope _scope;
        IResolver _resolver;

        public ScopeContext(IKernel kernel, string scopeName, ScopeContextKernelBinderHelper binderHelper)
        {
            _scope = kernel.CreateNamedScope(scopeName);
            _resolver = _scope.Get<IResolver>();
            _binderHelper = binderHelper;

            var singletonPerScope = _scope.Get<KernelScopeContextSingleton>();
            singletonPerScope.Scope = this;
        }

        public T Get<T>()
        {
            return _resolver.Get<T>();
        }

        public void SetSingletonValue<T>(T value)
        {
            var type = typeof(T);
            if (!_binderHelper.IsTypeRegisteredAsSingleton(type))
                throw new ArgumentException(String.Format("The type [{0}] is not registered as a singleton in the ScopeContextHelper.", type.FullName));

            _Values[type] = value;
        }

        public T GetSingletonValue<T>()
        {
            return (T)this.GetSingletonValue(typeof(T));
        }

        internal object GetSingletonValue(Type type)
        {
            object value;
            if (_Values.TryGetValue(type, out value))
                return value;
            else
                return null;
        }

        bool disposed = false;
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            _scope.Dispose();
        }
    }

    internal class KernelScopeContextSingleton
    {
        public ScopeContext Scope { get; set; }
    }
}
