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
    public class TaskDefinitionSetup
    {
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            var singleTask = modelBuilder.Entity<SingleTaskDefinition>();
            var recurrentTask = modelBuilder.Entity<RecurrentTaskDefinition>();

            singleTask.HasIndex(t => t.Identifier);
            singleTask.HasIndex(t => t.ScheduledAt).HasName("IX_CreateSingleTask");
            singleTask.HasIndex("Status", "LockedUntil");
            singleTask.HasIndex("RecurrentTaskId", "Status");

            recurrentTask.HasIndex(t => t.Identifier);
        }

        public static bool CheckModel(IModel model)
        {
            var entityType = model.FindEntityType(typeof(SingleTaskDefinition));
            if (entityType == null)
                return false;

            var property = entityType.FindProperty(nameof(SingleTaskDefinition.Identifier));
            if (property == null)
                return false;

            return property.GetContainingIndexes().Where(i => i.Properties.Count() == 1).Any();
        }
    }
#endif

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
#if NET_4X
        [Index]
#else
        //Index is ensured via TaskDefinitionSetup class
#endif
        public Guid Identifier { get; set; }

        /// <summary>
        /// Contains the parameters for the task (serialized as JSON)
        /// </summary>
        public string JsonParameters { get; set; }

        /// <summary>
        /// The task status
        /// </summary>
#if NET_4X
        [Index("IX_CreateSingleTask", Order = 0)]
#else
        //Index is ensured via TaskDefinitionSetup class
#endif
        public SingleTaskStatus Status { get; private set; }

        /// <summary>
        /// Date/time of the last status change.
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime StatusChangedAt { get; private set; }

        /// <summary>
        /// Time scheduled to run the task.
        /// </summary>        
#if NET_4X
        [Index]
#else
        //Index is ensured via TaskDefinitionSetup class
#endif
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
#if NET_4X
        [Index("IX_CreateSingleTask", Order = 1)]
#else
        //Index is ensured via TaskDefinitionSetup class
#endif
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
#if NET_4X
        [Index]
#else
        //Index is ensured via TaskDefinitionSetup class
#endif
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