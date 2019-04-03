using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
