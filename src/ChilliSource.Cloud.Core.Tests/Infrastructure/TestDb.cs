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
    public class TestDb : IDisposable
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public TestDb(ITestOutputHelper output)
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
            var connStr = Environment.GetEnvironmentVariable("UnitTestsConnectionString");
            Console.AppendLine($"Connection String: {connStr}");

            connStr = "Network=dbmssocn;Data Source=staging.bluechilli.com,1433;Initial Catalog=ChilliSource.Cloud.Core.TestDbContext;Persist Security Info=True;User ID=BuildServer;Password=PgQJMdqp2CwkJueqtqBr;MultipleActiveResultSets=true;";
            Console.AppendLine($"Connection String (fixed): {connStr}");

            using (var db = TestDbContext.Create(connStr))
            {
                db.Database.Connection.Open();
            }
        }
    }
}