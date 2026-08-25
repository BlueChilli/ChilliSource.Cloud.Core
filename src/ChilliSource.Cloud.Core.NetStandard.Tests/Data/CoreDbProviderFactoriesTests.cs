using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using ChilliSource.Cloud.Core;

namespace ChilliSource.Cloud.Core.NetStandard.Tests.Data
{
    public class CoreDbProviderFactoriesTests
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public CoreDbProviderFactoriesTests(ITestOutputHelper output)
        {
            _output = output;            
        }

        public void Dispose()
        {
            var outputStr = Console.ToString();
            if (outputStr.Length > 0)
            {
                _output.WriteLine(outputStr);
            }
        }

        [Fact]
        public void GetFactory_UsesDefaultSqlServerProvider()
        {
            var providerName = "Microsoft.Data.SqlClient";

            var factory = CoreDbProviderFactories.GetFactory(providerName);

            Assert.Same(Microsoft.Data.SqlClient.SqlClientFactory.Instance, factory);
        }
    }
}
