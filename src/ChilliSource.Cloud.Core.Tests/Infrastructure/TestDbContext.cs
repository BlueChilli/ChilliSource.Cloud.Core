using ChilliSource.Cloud.Core.Distributed;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Tests.Infrastructure
{
    public class TestDbContext : DbContext, ITaskRepository
    {
        public TestDbContext() { }

        public DbSet<DistributedLock> DistributedLocks { get; set; }

        public DbSet<SingleTaskDefinition> SingleTasks { get; set; }

        public DbSet<RecurrentTaskDefinition> RecurrentTasks { get; set; }
    }

    public class TestDbConfiguration : DbMigrationsConfiguration<TestDbContext>
    {
        public TestDbConfiguration()
        {
            this.AutomaticMigrationDataLossAllowed = true;
            this.AutomaticMigrationsEnabled = true;
        }
    }
}
