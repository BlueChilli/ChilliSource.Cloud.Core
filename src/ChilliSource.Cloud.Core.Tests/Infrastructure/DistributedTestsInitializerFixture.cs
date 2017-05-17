using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    [Collection(DistributedTestsCollection.Name)]
    public class DistributedTestsCollection: ICollectionFixture<DistributedTestsInitializerFixture>
    {
        public const string Name = "DistributedTestsCollection";
    }

    public class DistributedTestsInitializerFixture : IDisposable
    {
        static readonly Lazy<DistributedInitializer> _initalizer = new Lazy<DistributedInitializer>(() => new DistributedInitializer(), LazyThreadSafetyMode.ExecutionAndPublication);
        public DistributedTestsInitializerFixture()
        {
            var value = _initalizer.Value;
        }

        public void Dispose()
        {
        }
    }

    public class DistributedInitializer
    {
        public DistributedInitializer()
        {
            var log = new LoggerConfiguration().CreateLogger();

            GlobalConfiguration.Instance.SetLogger(log);

            using (var context = TestDbContext.Create())
            {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<TestDbContext, TestDbConfiguration>());
                context.Database.Initialize(true);

                context.Database.ExecuteSqlCommand("DELETE FROM SingleTasks");
                context.Database.ExecuteSqlCommand("DELETE FROM RecurrentTasks");
                context.Database.ExecuteSqlCommand("DELETE FROM DistributedLocks");
                context.SaveChanges();
            }
        }
    }
}
