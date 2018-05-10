using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ChilliSource.Cloud.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;

namespace ChilliSource.Cloud.Core.Tests.Data
{
    public class IQueryableExtensionsTests
    {
        private readonly StringBuilder Console = new StringBuilder();
        private readonly ITestOutputHelper _output;

        public IQueryableExtensionsTests(ITestOutputHelper output)
        {
            _output = output;

            using (var context = TestDbContext.Create())
            {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<TestDbContext, TestDbConfiguration>());
                context.Database.Initialize(true);
            }
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
        public void TestInMemoryPagedList()
        {
            var list = Enumerable.Range(0, 1000).ToList();

            var paged = list.AsQueryable().ToPagedList(page: 1, pageSize: 100);

            Assert.True(paged.TotalCount == 1000);
            Assert.True(paged.CurrentPage == 1);
            Assert.True(paged.PageSize == 100);
            Assert.True(paged.Count == 100);
        }

        [Fact]
        public async Task TestInMemoryPagedListAsync()
        {
            var list = Enumerable.Range(0, 1000).ToList();

            var paged = await list.AsQueryable().ToPagedListAsync(page: 1, pageSize: 100);

            Assert.True(paged.TotalCount == 1000);
            Assert.True(paged.CurrentPage == 1);
            Assert.True(paged.PageSize == 100);
            Assert.True(paged.Count == 100);
        }

        [Fact]
        public void TestDBPagedList()
        {
            using (var context = TestDbContext.Create())
            {
                var paged = context.DistributedLocks
                                .OrderBy(l => l.Id)
                                .ToPagedList(page: 1, pageSize: 100);

                Assert.True(paged.CurrentPage == 1);
                Assert.True(paged.PageSize <= 100);
            }
        }

        [Fact]
        public async Task TestDBPagedListAsync()
        {
            using (var context = TestDbContext.Create())
            {
                var paged = await context.DistributedLocks
                                    .OrderBy(l => l.Id)
                                    .ToPagedListAsync(page: 1, pageSize: 100);

                Assert.True(paged.CurrentPage == 1);
                Assert.True(paged.PageSize <= 100);
            }
        }
    }
}
