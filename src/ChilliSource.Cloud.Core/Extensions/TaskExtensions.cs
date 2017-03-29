using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Configures a task with Task.ConfigureAwait(false), ignoring the current synchronization context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static ConfiguredTaskAwaitable<T> IgnoreContext<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable IgnoreContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }
    }    
}
