using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Ninject.Scope
{
    /// <summary>
    /// Represents a factory of scope's instantiation.
    /// </summary>
    public interface IScopeContextHelper : IDisposable
    {
        /// <summary>
        ///  Creates a new scope context. The caller MUST call IScopeContext.Dispose() to dispose it.
        /// </summary>
        /// <returns>Returns a new scope context.</returns>
        IScopeContext CreateScope();
    }
}
