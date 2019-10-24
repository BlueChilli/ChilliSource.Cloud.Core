using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Provides execution of tasks in a serial manner (one at a time).    
    /// </summary>
    public class SerialExecutionManager
    {
        public static readonly SerialExecutionManager Instance = new SerialExecutionManager();
        private readonly Dictionary<Guid, SerialExecutionBucket> _buckets = new Dictionary<Guid, SerialExecutionBucket>();
        private readonly AsyncLock _lockAsync = new AsyncLock();

        private SerialExecutionManager() { }

        private async Task RunImpl(Guid resource, Func<Task> factory, CancellationToken cancellationToken, bool isAsync)
        {
            SerialExecutionBucket bucket = null;
            bool run = false;
            while (!run)
            {
                using (isAsync ? await _lockAsync.LockAsync(cancellationToken)
                               : _lockAsync.Lock(cancellationToken))
                {
                    bucket = _buckets.ContainsKey(resource) ? _buckets[resource]
                                        : (_buckets[resource] = SerialExecutionBucket.Create(resource));
                }

                run = isAsync ? await bucket.RunImpl(factory, ReleaseBucketTask, cancellationToken, isAsync: isAsync)
                               : SyncTaskHelper.ValidateSyncTask(bucket.RunImpl(factory, ReleaseBucketSync, cancellationToken, isAsync: isAsync));
            }
        }

        /// <summary>
        /// (Async) Provides execution of tasks in a serial manner (one at a time) for each resource. Tasks for different resources will run in parallel.
        /// </summary>
        /// <typeparam name="T">The type of result.</typeparam>
        /// <param name="resource">The resource id.</param>
        /// <param name="factory">A task factory.</param>
        /// <param name="cancellationToken">(optional) Allows cancellation when waiting to run. Not after the action started.</param>
        /// <returns>An awaitable task with a result.</returns>
        public async Task<T> RunAsync<T>(Guid resource, Func<Task<T>> factory, CancellationToken cancellationToken = default(CancellationToken))
        {
            T result = default(T);
            await RunAsync(resource, async () =>
            {
                result = await factory();
            }, cancellationToken);

            return result;
        }

        /// <summary>
        /// (Async) Provides execution of tasks in a serial manner (one at a time) for each resource. Tasks for different resources will run in parallel.
        /// </summary>
        /// <typeparam name="T">The type of result.</typeparam>
        /// <param name="resource">The resource id.</param>
        /// <param name="factory">A task factory.</param>
        /// <param name="cancellationToken">(optional) Allows cancellation when waiting to run. Not after the action started.</param>
        /// <returns>An awaitable task.</returns>
        public async Task RunAsync(Guid resource, Func<Task> factory, CancellationToken cancellationToken = default(CancellationToken))
        {
            await RunImpl(resource, factory, cancellationToken, isAsync: true);
        }

        /// <summary>
        /// Provides execution of actions in a serial manner (one at a time) for each resource. Actions for different resources will run in parallel.
        /// The caller gets blocked till the action executes.
        /// </summary>
        /// <typeparam name="T">The type of result.</typeparam>
        /// <param name="resource">The resource id.</param>
        /// <param name="func">An action factory.</param>
        /// <param name="cancellationToken">(optional) Allows cancellation when waiting to run. Not after the action started.</param>
        /// <returns>The action result.</returns>
        public T Run<T>(Guid resource, Func<T> func, CancellationToken cancellationToken = default(CancellationToken))
        {
            T result = default(T);
            Run(resource, () =>
            {
                result = func();
            }, cancellationToken);

            return result;
        }

        /// <summary>
        /// Provides execution of actions in a serial manner (one at a time) for each resource. Actions for different resources will run in parallel.
        /// The caller gets blocked till the action executes.
        /// </summary>
        /// <param name="resource">The resource id.</param>
        /// <param name="action">An action factory.</param>
        /// <param name="cancellationToken">(optional) Allows cancellation when waiting to run. Not after the action started.</param>
        public void Run(Guid resource, Action action, CancellationToken cancellationToken = default(CancellationToken))
        {
            SyncTaskHelper.ValidateSyncTask(RunImpl(resource, WrapAction(action), cancellationToken, isAsync: false));
        }

        private Func<Task> WrapAction(Action action)
        {
            return () => { action(); return Task.CompletedTask; };
        }

        private async Task ReleaseBucketTask(SerialExecutionBucket bucket)
        {
            await ReleaseBucketImpl(bucket, isAsync: true);
        }

        private Task ReleaseBucketSync(SerialExecutionBucket bucket)
        {
            return ReleaseBucketImpl(bucket, isAsync: false);
        }

        private async Task ReleaseBucketImpl(SerialExecutionBucket bucket, bool isAsync)
        {
            using (isAsync ? await _lockAsync.LockAsync()
                           : _lockAsync.Lock())
            {
                var resource = bucket.Resource;

                //reads bucket from dictionary again
                var bucketInDict = _buckets.ContainsKey(resource) ? _buckets[resource] : null;

                if (object.ReferenceEquals(bucket, bucketInDict))
                {
                    _buckets.Remove(resource);
                }
            }
        }
    }

    internal class SerialExecutionBucket
    {
        private readonly AsyncLock _bucketLockAsync;
        private int _waitingCount;
        private bool _released;
#if DEBUG
        public static int _numberOfInstances = 0;
#endif

        Guid _resource;
        private SerialExecutionBucket(Guid resource)
        {
#if DEBUG
            Interlocked.Increment(ref _numberOfInstances);
#endif
            _resource = resource;
            _released = false;
            _waitingCount = 0;
            _bucketLockAsync = new AsyncLock();
        }

        public static SerialExecutionBucket Create(Guid resource)
        {
            return new SerialExecutionBucket(resource);
        }

        public Guid Resource { get { return _resource; } }

        public async Task<bool> RunImpl(Func<Task> factory, Func<SerialExecutionBucket, Task> releaseBucketTask, CancellationToken cancellationToken, bool isAsync)
        {
            if (_released)
            {
                return false;
            }

            Interlocked.Increment(ref this._waitingCount);
            try
            {
                using (isAsync ? await _bucketLockAsync.LockAsync(cancellationToken)
                               : _bucketLockAsync.Lock(cancellationToken))
                {
                    if (_released)
                    {
                        return false;
                    }

                    if (isAsync)
                    {
                        await factory();
                    }
                    else
                    {
                        SyncTaskHelper.ValidateSyncTask(factory());
                    }

                    return true;
                }
            }
            finally
            {
                Interlocked.Decrement(ref this._waitingCount);

                if (!_released && _waitingCount == 0)
                {
                    //no cancellation token used when releasing.
                    using (isAsync ? await _bucketLockAsync.LockAsync()
                                   : _bucketLockAsync.Lock())
                    {
                        if (!_released && _waitingCount == 0)
                        {
                            if (isAsync)
                            {
                                await releaseBucketTask(this);
                            }
                            else
                            {
                                SyncTaskHelper.ValidateSyncTask(releaseBucketTask(this));
                            }

                            _released = true;
                        }
                    }
                }
            }
        }
    }

    internal static class SyncTaskHelper
    {
        /// <summary>
        /// Validates that the task completed synchronously. If not, you have an *implementation error* and an exception will be thrown.
        /// </summary>
        /// <param name="task">A task.</param>
        public static void ValidateSyncTask(Task task)
        {
            if (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                throw new ApplicationException("The task was expected to be completed synchronously.");

            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Validates that the task completed synchronously. If not, you have an *implementation error* and an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The task result type.</typeparam>
        /// <param name="task">A task.</param>
        /// <returns></returns>
        public static T ValidateSyncTask<T>(Task<T> task)
        {
            if (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                throw new ApplicationException("The task was expected to be completed synchronously.");

            return task.GetAwaiter().GetResult();
        }
    }
}
