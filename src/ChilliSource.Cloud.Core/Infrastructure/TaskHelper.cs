using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
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

        /// <summary>
        /// Queues the specified work returned by <paramref name="function"/> using a particular TaskScheduler.
        /// </summary>
        /// <param name="function">The work to execute asynchronously</param>
        /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
        /// <param name="taskScheduler">A taskScheduler to queue the work. e.g. TaskScheduler.Current, TaskScheduler.Default</param>
        /// <returns>A Task that represents a proxy for the Task returned by <paramref name="function"/>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="function"/> parameter was null.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The CancellationTokenSource associated with <paramref name="cancellationToken"/> was disposed.
        /// </exception>
        public static Task Run(Func<Task> function, TaskScheduler taskScheduler, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (function == null)
                throw new ArgumentNullException("The function parameter was null.");

            if (taskScheduler == null)
                throw new ArgumentNullException("The taskScheduler parameter was null.");

            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            Task<Task> task = Task<Task>.Factory.StartNew(function, cancellationToken, TaskCreationOptions.DenyChildAttach, taskScheduler);
            return task.Unwrap();
        }

        /// <summary>
        /// Queues the specified work returned by <paramref name="function"/> using a particular TaskScheduler.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the proxy Task.</typeparam>
        /// <param name="function">The work to execute asynchronously</param>
        /// <param name="cancellationToken">A cancellation token that should be used to cancel the work</param>
        /// <param name="taskScheduler">A taskScheduler to queue the work. e.g. TaskScheduler.Default, TaskScheduler.Current</param>
        /// <returns>A Task that represents a proxy for the Task returned by <paramref name="function"/>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// The <paramref name="function"/> parameter was null.
        /// </exception>
        /// <exception cref="System.ObjectDisposedException">
        /// The CancellationTokenSource associated with <paramref name="cancellationToken"/> was disposed.
        /// </exception>
        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function, TaskScheduler taskScheduler, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (function == null)
                throw new ArgumentNullException("The function parameter was null.");

            if (taskScheduler == null)
                throw new ArgumentNullException("The taskScheduler parameter was null.");

            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<TResult>(cancellationToken);

            Task<Task<TResult>> task = Task<Task<TResult>>.Factory.StartNew(function, cancellationToken, TaskCreationOptions.DenyChildAttach, taskScheduler);
            return task.Unwrap();
        }
    }
}
