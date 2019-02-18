#if !NET_46X
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// This class is only needed to due to the lack of a DbProviderFactories class in .Net Standard 2.0.
    /// It should be deprecated when .Net Standard 2.1 gets released.
    /// </summary>
    public abstract class CoreDbProviderFactories
    {
        internal static readonly Dictionary<string, Func<DbProviderFactory>> _providerFactories = new Dictionary<string, Func<DbProviderFactory>>();

        public static DbProviderFactory GetFactory(string providerInvariantName)
        {
            if (string.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentNullException(nameof(providerInvariantName));
            }

            if (_providerFactories.ContainsKey(providerInvariantName))
            {
                return _providerFactories[providerInvariantName]();
            }

            throw new ApplicationException($"Provider '{providerInvariantName}' is not registered in ChilliSource.Cloud.Core.CoreDbProviderFactories.");
        }

        public static void RegisterFactory(string providerInvariantName, Func<DbProviderFactory> factoryProvider)
        {
            if (string.IsNullOrEmpty(providerInvariantName))
            {
                throw new ArgumentNullException(nameof(providerInvariantName));
            }

            if (factoryProvider == null)
            {
                throw new ArgumentNullException(nameof(factoryProvider));
            }

            _providerFactories[providerInvariantName] = factoryProvider;
        }

        public static IEnumerable<string> GetFactoryProviderNames()
        {
            return _providerFactories.Keys.ToArray();
        }
    }
}
#endif