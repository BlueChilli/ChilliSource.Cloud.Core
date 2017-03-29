using Ninject;
using Ninject.Extensions.ContextPreservation;
using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Ninject
{
    internal class ScopeContextKernelBinderHelper : IKernelBinderHelper
    {
        IKernel _kernel;
        Action<IBindingSyntax> _scopeAction;
        List<Type> _singletonTypes;

        internal ScopeContextKernelBinderHelper(IKernel kernel, Action<IBindingSyntax> scopeAction)
        {
            _kernel = kernel;
            _scopeAction = scopeAction;
            _singletonTypes = new List<Type>();
        }

        public void RegisterSingletonType(Type type)
        {
            _singletonTypes.Add(type);

            _kernel.Bind(type).ToMethod(ctx => ctx.ContextPreservingGet<KernelScopeContextSingleton>().Scope.GetSingletonValue(type));
        }

        public bool IsTypeRegisteredAsSingleton(Type type)
        {
            return _singletonTypes.Contains(type);
        }

        public void RegisterServices(ScopeContextHelper.RegisterServices registerServices)
        {
            registerServices(_kernel, _scopeAction);
        }
    }
}
