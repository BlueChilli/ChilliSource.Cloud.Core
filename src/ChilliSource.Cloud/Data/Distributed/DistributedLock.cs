using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Data.Distributed
{
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
        [Index(IsUnique = true)]
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
