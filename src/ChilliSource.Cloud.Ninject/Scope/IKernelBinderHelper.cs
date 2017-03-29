using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Ninject
{
    /// <summary>
    /// Allows customization of IScopeContextHelper
    /// </summary>
    public interface IKernelBinderHelper
    {
        /// <summary>
        /// Binds services in the kernel for a specific scopeAction.<br/>
        /// e.g. kernel.Bind&lt;MyServiceA&gt;().ToSelf().InScopeAction(scopeAction);
        /// </summary>
        /// <param name="registerServices">Delegate to bind services</param>
        void RegisterServices(ScopeContextHelper.RegisterServices registerServices);

        /// <summary>
        /// Registers a type as a Singleton type. The object creation is not handled by the kernel. <br/>
        /// Instead, the caller can provide an object instance for the scope.
        /// </summary>
        /// <param name="type">A object type</param>
        void RegisterSingletonType(Type type);

        /// <summary>
        /// Returns whether an object type is already registered as a Singleton type.
        /// </summary>
        /// <param name="type">A object type</param>
        /// <returns>Returns whether an object type is already registered as a Singleton type.</returns>
        bool IsTypeRegisteredAsSingleton(Type type);
    }
}
