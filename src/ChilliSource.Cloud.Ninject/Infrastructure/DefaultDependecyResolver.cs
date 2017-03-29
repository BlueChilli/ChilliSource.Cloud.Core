using ChilliSource.Cloud.Core;
using Ninject;
using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Ninject
{
    /// <summary>
    /// Default depency resolver for Ninject. It should be registered using the interface IResolver. e.g.:
    /// kernel.Bind<IResolver>().ToMethod(ctx => new DefaultDependecyResolver(ctx.GetContextPreservingResolutionRoot()));
    /// </summary>
    public class DefaultDependecyResolver : IResolver
    {
        private readonly object _localLock = new object();
        IResolutionRoot _root;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="root">Resolution root object</param>
        public DefaultDependecyResolver(IResolutionRoot root)
        {
            _root = root;
        }

        /// <summary>
        /// Gets an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">Specified type.</typeparam>
        /// <returns>An instance which is compatible with the specified type.</returns>
        public T Get<T>()
        {
            return _root.Get<T>();
        }

        public bool Release(object instance)
        {
            return _root.Release(instance);
        }

        bool disposed = false;
        public void Dispose()
        {
            if (disposed)
                return;

            lock (_localLock)
            {
                if (disposed)
                    return;
                disposed = true;
            }

            if (_root is IDisposable)
            {
                (_root as IDisposable).Dispose();
            }
        }
    }
}
