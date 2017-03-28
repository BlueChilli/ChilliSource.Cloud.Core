using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Distributed
{
    /// <summary>
    /// Defines the repository structure for distributed tasks
    /// </summary>
    public interface ITaskRepository : IDistributedLockRepository
    {
        /// <summary>
        /// Returns a repository for single tasks
        /// </summary>
        DbSet<SingleTaskDefinition> SingleTasks { get; }

        /// <summary>
        /// Returns a repository for recurrent tasks
        /// </summary>
        DbSet<RecurrentTaskDefinition> RecurrentTasks { get; }
    }
}
