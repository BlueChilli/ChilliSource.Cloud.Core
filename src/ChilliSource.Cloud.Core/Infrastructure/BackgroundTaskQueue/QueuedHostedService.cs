#if !NET_4X
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private IBackgroundTaskQueueConsumer _taskQueue;
        private QueuedHostedServiceOptions _options;
        private SemaphoreSlim _semaphore;
        private HashSet<Task> _currentPromises;
        private readonly object _localLock = new object();

        public QueuedHostedService(IBackgroundTaskQueueConsumer taskQueue, ILoggerFactory loggerFactory, IOptions<QueuedHostedServiceOptions> options)
        {
            _taskQueue = taskQueue;
            _logger = loggerFactory?.CreateLogger<QueuedHostedService>();
            _options = options.Value;
            _semaphore = new SemaphoreSlim(_options.MaxConcurrency, _options.MaxConcurrency);
            _currentPromises = new HashSet<Task>();
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInformation("Queued Hosted Service is starting.");

            //Waits till cancellation is requested.
            await DequeueAndRunTasks(cancellationToken, cancellationToken);

            //Runs last minute tasks.
            using (var cts = new CancellationTokenSource(100))
            {
                await DequeueAndRunTasks(cts.Token, cancellationToken);
            }

            //Give tasks a chance to finish.
            await Task.WhenAll(GetTaskPromises());

            _logger?.LogInformation("Queued Hosted Service is stopping.");
        }

        private async Task DequeueAndRunTasks(CancellationToken dequeueCancellationToken, CancellationToken taskCancellationToken)
        {
            try
            {
                while (!dequeueCancellationToken.IsCancellationRequested)
                {
                    var workItem = await _taskQueue.DequeueAsync(dequeueCancellationToken);
                    RunWorkItem(workItem, taskCancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                /* noop */
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error occurred while dequeuing a task.");
            }
        }

        private void RunWorkItem(Func<CancellationToken, Task> workItem, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>();
            AddTaskPromise(tcs.Task);

            Task.Run(async () =>
            {
                //CancellationToken.None - The task has a chance to run even when cancellation is requested, but only after acquiring a semaphore lock.
                await _semaphore.WaitAsync(CancellationToken.None);

                try
                {
                    //It's up to the workItem to use the CancellationToken.
                    await workItem(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    /* noop - exception is *not* propagated to tcs */
                }
                catch (Exception ex)
                {
                    /* exception is *not* propagated to tcs */
                    _logger?.LogError(ex, $"Exception was thrown by a background task.");
                }
                finally
                {
                    RemoveTaskPromise(tcs.Task);
                    _semaphore.Release();

                    //signals tcs that the task is finished
                    tcs.TrySetResult(null);
                }
            });
        }

        private void AddTaskPromise(Task task)
        {
            lock (_localLock)
            {
                _currentPromises.Add(task);
            }
        }

        private void RemoveTaskPromise(Task task)
        {
            lock (_localLock)
            {
                _currentPromises.Remove(task);
            }
        }

        private Task[] GetTaskPromises()
        {
            lock (_localLock)
            {
                return _currentPromises.ToArray();
            }
        }

        bool disposed = false;
        public override void Dispose()
        {
            if (disposed)
                return;

            _semaphore.Dispose();
            base.Dispose();

            disposed = true;
        }
    }
}
#endif