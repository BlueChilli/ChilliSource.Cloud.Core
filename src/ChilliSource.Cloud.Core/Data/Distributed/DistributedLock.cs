using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if !NET_4X
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
#endif

namespace ChilliSource.Cloud.Core.Distributed
{
#if !NET_4X
    public class DistributedLockSetup
    {
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DistributedLock>()
                .HasIndex(l => l.Resource).IsUnique();
        }

        public static bool CheckModel(IModel model)
        {
            var entityType = model.FindEntityType(typeof(DistributedLock));
            if (entityType == null)
                return false;

            var property = entityType.FindProperty(nameof(DistributedLock.Resource));
            if (property == null)
                return false;

            return property.GetContainingIndexes().Where(i => i.Properties.Count() == 1 && i.IsUnique).Any();
        }        
    }
#endif

    /// <summary>
    /// Represents a lock instance across multiple machines or processes
    /// </summary>
    public class DistributedLock
    {
        /// <summary>
        /// Lock Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Resource GUID that needs to be locked
        /// </summary>
#if NET_4X
        [Index(IsUnique = true)]
#else
        //Index is ensured via DistributedLockSetup class
#endif
        public Guid Resource { get; set; }

        /// <summary>
        /// Lock reference number, which is incremented on every lock acquisition.
        /// </summary>
        public int LockReference { get; set; }

        /// <summary>
        /// Lock timeout in milliseconds
        /// </summary>
        public long Timeout { get; set; }

        /// <summary>
        /// (When locked) Locked at Date/Time
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? LockedAt { get; set; }

        /// <summary>
        /// (When locked) Locked until Date/Time
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? LockedUntil { get; set; }

        /// <summary>
        /// (When locked) Locked by machine name
        /// </summary>
        [StringLength(100)]
        public string LockedByMachine { get; set; }

        /// <summary>
        /// (When locked) Locked by Process Id (PID)
        /// </summary>
        public int? LockedByPID { get; set; }
    }
}