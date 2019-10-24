using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if NET_4X
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
#endif

namespace ChilliSource.Cloud.Core.Distributed
{
    /// <summary>
    /// Defines the repository structure for distributed locks
    /// </summary>
    public interface IDistributedLockRepository : IRepository
    {
        /// <summary>
        /// Returns a DistributedLock Entity Framework repository
        /// </summary>
        DbSet<DistributedLock> DistributedLocks { get; }
    }

    /// <summary>
    /// Generic interface for Entity Framework repositories
    /// </summary>
    public interface IRepository : IDisposable
    {
#if NET_4X
        /// <summary>
        /// Returns a database instance. The database connection may or may not be open yet.
        /// </summary>
        Database Database { get; }
#else        
        DbContext DbContext { get;  }
#endif

        /// <summary>
        /// Persists all pending changes to the database
        /// </summary>
        /// <returns>Number of records affected</returns>
        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
