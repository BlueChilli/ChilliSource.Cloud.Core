using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Data.Distributed
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
        /// <summary>
        /// Returns a database instance. The database connection may or may not be open yet.
        /// </summary>
        Database Database { get; }

        /// <summary>
        /// Persists all pending changes to the database
        /// </summary>
        /// <returns>Number of records affected</returns>
        int SaveChanges();
    }
}
