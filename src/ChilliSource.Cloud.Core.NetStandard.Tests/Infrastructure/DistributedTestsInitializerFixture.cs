using Serilog;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ChilliSource.Cloud.Core.Tests
{
    [CollectionDefinition(DistributedTestsCollection.Name)]
    public class DistributedTestsCollection : ICollectionFixture<DistributedTestsInitializerFixture>
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
            //_initalizer.Value.CleanUp();
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
                context.Database.EnsureCreated();
            }

            this.CleanUp();
        }

        public void CleanUp()
        {
            using (var context = TestDbContext.Create())
            {
                context.Database.ExecuteSqlRaw("DELETE FROM SingleTasks");
                context.Database.ExecuteSqlRaw("DELETE FROM RecurrentTasks");
                context.Database.ExecuteSqlRaw("DELETE FROM DistributedLocks");
                context.SaveChanges();
            }
        }
    }
}
