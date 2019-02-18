#if NET_46X
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Distributed
{
    /// <summary>
    /// Represents a single task schedule across multiple machines or processes
    /// </summary>
    [Table("SingleTasks")]
    public class SingleTaskDefinition
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public SingleTaskDefinition()
        {
            this.SetStatus(Distributed.SingleTaskStatus.None);
        }

        /// <summary>
        /// Task Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Defines the type of the task. There can be multiple scheduled tasks of the same type.
        /// </summary>
        [Index]
        public Guid Identifier { get; set; }

        /// <summary>
        /// Contains the parameters for the task (serialized as JSON)
        /// </summary>
        public string JsonParameters { get; set; }

        /// <summary>
        /// The task status
        /// </summary>
        [Index("IX_CreateSingleTask", Order = 0)]
        public SingleTaskStatus Status { get; private set; }

        /// <summary>
        /// Date/time of the last status change.
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime StatusChangedAt { get; private set; }

        /// <summary>
        /// Time scheduled to run the task.
        /// </summary>
        [Index]
        [Column(TypeName = "datetime2")]
        public DateTime ScheduledAt { get; set; }

        /// <summary>
        /// (When running) The date/time when the task started running.
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime? LastRunAt { get; set; }

        /// <summary>
        /// Contains the upper time limit of the latest task lock
        /// </summary>
        [Index("IX_CreateSingleTask", Order = 1)]
        [Column(TypeName = "datetime2")]
        public DateTime? LockedUntil { get; set; }

        /// <summary>
        /// A reference to a recurrent task if this task was created from a recurrent task definition.
        /// </summary>
        public int? RecurrentTaskId { get; set; }

        /// <summary>
        /// A reference to a recurrent task if this task was created from a recurrent task definition.
        /// </summary>
        public RecurrentTaskDefinition RecurrentTask { get; set; }

        /// <summary>
        /// Changes the task status and logs the instant of change. No change is made, if it is the same status .
        /// </summary>
        /// <param name="value">New task status</param>
        /// <returns>Returns whether the status was set.</returns>
        public bool SetStatus(SingleTaskStatus value)
        {
            if (this.Status != value)
            {
                this.Status = value;
                this.StatusChangedAt = DateTime.UtcNow;

                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Represents a recurrent task schedule across multiple machines or processes
    /// </summary>
    [Table("RecurrentTasks")]
    public class RecurrentTaskDefinition : IValidatableObject
    {
        public RecurrentTaskDefinition()
        {
            this.SingleTasks = new List<SingleTaskDefinition>();
        }

        /// <summary>
        /// Task Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Defines the type of the task. There can be only one recurrent task per task type.<br/>
        /// Multiple single tasks instances will be generated for this recurrent task.
        /// </summary>
        [Index]
        public Guid Identifier { get; set; }

        /// <summary>
        /// Whether this recurrent task is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The interval between single task executions.
        /// </summary>
        public long Interval { get; set; }

        /// <summary>
        /// Reference to all single tasks instances created for this recurrent task.
        /// </summary>
        public virtual ICollection<SingleTaskDefinition> SingleTasks { get; set; }

        /// <summary>
        /// Object Validation
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.Interval < 1000)
            {
                yield return new ValidationResult("Recurrent tasks must have an interval greater than 1000 milliseconds.");
            }
        }
    }

    /// <summary>
    /// Task status
    /// </summary>
    public enum SingleTaskStatus : int
    {
        None = 0,
        Scheduled = 1,
        Running,
        Completed,
        CompletedCancelled,
        CompletedAborted,
        CompletedAbandoned,
    }
}
#endif