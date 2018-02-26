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

        /// <summary>
        /// (Async) Provides execution of tasks in a serial manner (one at a time) for each resource. Tasks for different resources will run in parallel.
        /// </summary>
        /// <typeparam name="T">The type of result.</typeparam>
        /// <param name="resource">The resource id.</param>
        /// <param name="factory">A task factory.</param>
        /// <returns>An awaitable task with a result.</returns>
        public async Task<T> RunAsync<T>(Guid resource, Func<Task<T>> factory)
        {
            T result = default(T);
            await RunAsync(resource, async () =>
            {
                result = await factory();
            });

            return result;
        }

        /// <summary>
        /// (Async) Provides execution of tasks in a serial manner (one at a time) for each resource. Tasks for different resources will run in parallel.
        /// </summary>
        /// <typeparam name="T">The type of result.</typeparam>
        /// <param name="resource">The resource id.</param>
        /// <param name="factory">A task factory.</param>
        /// <returns>An awaitable task.</returns>
        public async Task RunAsync(Guid resource, Func<Task> factory)
        {
            SerialExecutionBucket bucket = null;
            bool run = false;
            while (!run)
            {
                using (await _lockAsync.LockAsync())
                {
                    bucket = _buckets.ContainsKey(resource) ? _buckets[resource]
                                        : (_buckets[resource] = SerialExecutionBucket.Create(resource));
                }

                run = await bucket.RunTask(factory, ReleaseBucketTask);
            }
        }

        /// <summary>
        /// Provides execution of actions in a serial manner (one at a time) for each resource. Actions for different resources will run in parallel.
        /// The caller gets blocked till the action executes.
        /// </summary>
        /// <typeparam name="T">The type of result.</typeparam>
        /// <param name="resource">The resource id.</param>
        /// <param name="func">An action factory.</param>
        /// <returns>The action result.</returns>
        public T Run<T>(Guid resource, Func<T> func)
        {
            T result = default(T);
            Run(resource, () =>
            {
                result = func();
            });

            return result;
        }

        /// <summary>
        /// Provides execution of actions in a serial manner (one at a time) for each resource. Actions for different resources will run in parallel.
        /// The caller gets blocked till the action executes.
        /// </summary>
        /// <param name="resource">The resource id.</param>
        /// <param name="action">An action factory.</param>
        public void Run(Guid resource, Action action)
        {
            SerialExecutionBucket bucket = null;
            bool run = false;
            while (!run)
            {
                using (_lockAsync.Lock())
                {
                    bucket = _buckets.ContainsKey(resource) ? _buckets[resource]
                                        : (_buckets[resource] = SerialExecutionBucket.Create(resource));
                }

                run = bucket.RunAction(action, ReleaseBucketAction);
            }
        }

        private async Task ReleaseBucketTask(SerialExecutionBucket bucket)
        {
            using (await _lockAsync.LockAsync())
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

        private void ReleaseBucketAction(SerialExecutionBucket bucket)
        {
            using (_lockAsync.Lock())
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

        public async Task<bool> RunTask(Func<Task> factory, Func<SerialExecutionBucket, Task> releaseBucketTask)
        {
            if (_released)
            {
                return false;
            }

            Interlocked.Increment(ref this._waitingCount);
            try
            {
                using (await _bucketLockAsync.LockAsync())
                {
                    if (_released)
                    {
                        return false;
                    }

                    await factory();
                    return true;
                }
            }
            finally
            {
                Interlocked.Decrement(ref this._waitingCount);

                if (!_released && _waitingCount == 0)
                {
                    using (await _bucketLockAsync.LockAsync())
                    {
                        if (!_released && _waitingCount == 0)
                        {
                            await releaseBucketTask(this);
                            _released = true;
                        }
                    }
                }
            }
        }

        public bool RunAction(Action action, Action<SerialExecutionBucket> releaseBucketAction)
        {
            if (_released)
            {
                return false;
            }

            Interlocked.Increment(ref this._waitingCount);
            try
            {
                using (_bucketLockAsync.Lock())
                {
                    if (_released)
                    {
                        return false;
                    }

                    action();
                    return true;
                }
            }
            finally
            {
                Interlocked.Decrement(ref this._waitingCount);

                if (!_released && _waitingCount == 0)
                {
                    using (_bucketLockAsync.Lock())
                    {
                        if (!_released && _waitingCount == 0)
                        {
                            releaseBucketAction(this);
                            _released = true;
                        }
                    }
                }
            }
        }
    }
}
