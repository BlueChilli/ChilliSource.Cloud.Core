using ChilliSource.Cloud.Core.Distributed;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ChilliSource.Cloud.Core.Tests
{
    public class TestDb : IClassFixture<DistributedTestsInitializerFixture>, IDisposable
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public TestDb(DistributedTestsInitializerFixture initializer, ITestOutputHelper output)
        {
            //We don't need to do anything with the initializer

            _output = output;
        }

        public void Dispose()
        {
            _output.WriteLine(Console.ToString());
        }

        [Fact]
        public void Test()
        {
            Console.AppendLine($"Connection String: {Environment.GetEnvironmentVariable("UnitTestsConnectionString")}");
        }
    }
}