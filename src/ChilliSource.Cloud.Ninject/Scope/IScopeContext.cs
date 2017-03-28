using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Ninject
{
    /// <summary>
    /// Represents a new instantiation scope
    /// </summary>
    public interface IScopeContext : IDisposable
    {
        /// <summary>
        /// Creates an object within this scope
        /// </summary>
        /// <typeparam name="T">A object type</typeparam>
        /// <returns>Returns an instance of T within this scope</returns>
        T Get<T>();

        /// <summary>
        /// Sets a value for a singleton type within this scope. Singleton types MUST be registered when calling ScopeContextHelperFactory.Create(...): <br/>
        /// IKernelBinderHelper.RegisterSingletonType(type);
        /// </summary>
        /// <typeparam name="T">A singleton type registered when creating IScopeContextHelper via ScopeContextHelperFactory.Create(...)</typeparam>
        /// <param name="value">A singleton value</param>
        void SetSingletonValue<T>(T value);

        /// <summary>
        /// Returns a  value for a singleton type withing this scope.
        /// </summary>
        /// <typeparam name="T">A singleton type registered when creating IScopeContextHelper via ScopeContextHelperFactory.Create(...): <br/>
        /// IKernelBinderHelper.RegisterSingletonType(type);
        /// </typeparam>
        /// <returns>Returns a singleton value</returns>
        T GetSingletonValue<T>();
    }
}
