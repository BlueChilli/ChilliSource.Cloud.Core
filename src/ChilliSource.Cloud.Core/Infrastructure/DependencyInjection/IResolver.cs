using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.DependencyInjection
{
    /// <summary>
    /// Generic interface for resolving instances.
    /// </summary>
    public interface IResolver : IDisposable
    {
        /// <summary>
        /// Gets an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">Specified type.</typeparam>
        /// <returns>An instance which is compatible with the specified type.</returns>
        T Get<T>();
    }
}
