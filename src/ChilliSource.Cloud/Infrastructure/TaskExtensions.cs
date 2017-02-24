﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Infrastructure
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

    public static class TaskHelper
    {
        public static void WaitSafeSync(this Func<Task> taskFactory)
        {
            //Good reat at https://msdn.microsoft.com/en-us/magazine/mt238404.aspx
            //This will make sure the async task doesn't run on the same calling thread (which can potentially be blocked by the async task)
            var task = Task.Run(taskFactory);

            //Blocks the current thread untill we have a result from the task. If the current thread is already blocked by the async task we will have a *deadlock* .
            //This is avoided by Task.Run(...)
            task.GetAwaiter().GetResult();
        }

        public static T GetResultSafeSync<T>(this Func<Task<T>> taskFactory)
        {
            var task = Task.Run(taskFactory);
            return task.GetAwaiter().GetResult();
        }
    }
}