using ChilliSource.Cloud.Core.Distributed;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Tests
{
    public class TestDbContext : DbContext, ITaskRepository
    {
        private TestDbContext() : base() { }
        private TestDbContext(string connStr) : base(connStr) { }

        public static TestDbContext Create()
        {
            //var connStr = Environment.GetEnvironmentVariable("UnitTestsConnectionString");            
            var connStr = "Network=dbmssocn;Data Source=staging.bluechilli.com,1433;Initial Catalog=ChilliSource.Cloud.Core.TestDbContext.LOCAL;Persist Security Info=True;User ID=BuildServer;Password=PgQJMdqp2CwkJueqtqBr;MultipleActiveResultSets=true;";

            return Create(connStr);
        }

        public static TestDbContext Create(string connStr)
        {
            if (String.IsNullOrEmpty(connStr))
                return new TestDbContext();
            else
                return new TestDbContext(connStr);
        }

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
