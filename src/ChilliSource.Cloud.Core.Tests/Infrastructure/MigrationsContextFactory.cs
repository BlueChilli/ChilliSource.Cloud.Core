using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Tests
{
    public class MigrationsContextFactory : IDbContextFactory<TestDbContext>
    {
        public TestDbContext Create()
        {
            return TestDbContext.Create();
        }
    }
}
