using ChilliSource.Cloud.Core.Distributed;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Tests
{
    public class TestDbContext : DbContext, ITaskRepository
    {
        private TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public static TestDbContext Create()
        {
            var connStr = ""; // Environment.GetEnvironmentVariable("UnitTestsConnectionString");
            return Create(connStr);
        }

        public static TestDbContext Create(string connStr)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>();
            options.UseSqlServer(String.IsNullOrEmpty(connStr) ? "Server=DAVE_DESKTOP\\SQLEXPRESS;Database=ChilliSourceCloudCoreTests;Integrated Security=true;Encrypt=True;TrustServerCertificate=True" : connStr);
            return new TestDbContext(options.Options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            DistributedLockSetup.OnModelCreating(modelBuilder);
            TaskDefinitionSetup.OnModelCreating(modelBuilder);
        }

        public DbSet<DistributedLock> DistributedLocks { get; set; }

        public DbSet<SingleTaskDefinition> SingleTasks { get; set; }

        public DbSet<RecurrentTaskDefinition> RecurrentTasks { get; set; }

        DbContext IRepository.DbContext => this;
    }

}
