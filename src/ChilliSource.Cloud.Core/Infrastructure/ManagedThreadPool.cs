using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    internal interface IThreadTaskInfo
    {
        bool IsWaitingToRun { get; }
        bool IsRunning { get; }
        bool IsCompleted { get; }
        bool IsFaulted { get; }
    }

    /// <summary>Managed thread pool.</summary>
    internal class ManagedThreadPool
    {
        #region Constants
        /// <summary>Maximum number of threads the thread pool has at its disposal.</summary>
        private int _maxWorkerThreads;
        #endregion

        #region Member Variables
        /// <summary>Queue of all the callbacks waiting to be executed.</summary>
        private Queue<ThreadTaskInfo> _waitingCallbacks;
        /// <summary>
        /// Used to signal that a worker thread is needed for processing.  Note that multiple
        /// threads may be needed simultaneously and as such we use a semaphore instead of
        /// an auto reset event.
        /// </summary>
        private Semaphore _workerThreadNeeded;
        /// <summary>List of all worker threads at the disposal of the thread pool.</summary>
        private ThreadWrapper[] _workerThreads;
        /// <summary>Number of threads currently active.</summary>
        private int _inUseThreads;
        /// <summary>Lockable object for the pool.</summary>
        private readonly object _poolLock = new object();
        private bool _stopPoolRequest = false;
        #endregion

        #region Construction and Finalization
        /// <summary>Initialize the thread pool.</summary>
        public ManagedThreadPool(int maxWorkerThreads) { Initialize(maxWorkerThreads); }

        /// <summary>Initializes the thread pool.</summary>
        private void Initialize(int maxWorkerThreads)
        {
            _maxWorkerThreads = maxWorkerThreads;
            // Create our thread stores; we handle synchronization ourself
            // as we may run into situtations where multiple operations need to be atomic.
            // We keep track of the threads we've created just for good measure; not actually
            // needed for any core functionality.
            _waitingCallbacks = new Queue<ThreadTaskInfo>();
            _workerThreads = new ThreadWrapper[_maxWorkerThreads];
            _inUseThreads = 0;

            // Create our "thread needed" event
            _workerThreadNeeded = new Semaphore(0);

            // Create all of the worker threads
            for (int i = 0; i < _maxWorkerThreads; i++)
            {
                // Create a new thread and add it to the list of threads.                
                _workerThreads[i] = ThreadWrapper.Create(ProcessQueuedItems);
            }
        }
        #endregion

        #region Public Methods
        public IThreadTaskInfo CreateCompletedTask()
        {
            return new CompletedThreadTaskInfo();
        }

        /// <summary>Queues a user work item to the thread pool.</summary>
        /// <param name="callback">
        /// A WaitCallback representing the delegate to invoke when the thread in the 
        /// thread pool picks up the work item.
        /// </param>
        public void QueueUserWorkItem(WaitCallback callback)
        {
            // Queue the delegate with no state
            QueueUserWorkItem(callback, null);
        }

        /// <summary>Queues a user work item to the thread pool.</summary>
        /// <param name="callback">
        /// A WaitCallback representing the delegate to invoke when the thread in the 
        /// thread pool picks up the work item.
        /// </param>
        /// <param name="state">
        /// The object that is passed to the delegate when serviced from the thread pool.
        /// </param>
        public IThreadTaskInfo QueueUserWorkItem(WaitCallback callback, object state)
        {
            // Create a waiting callback that contains the delegate and its state.
            // At it to the processing queue, and signal that data is waiting.
            ThreadTaskInfo waiting = new ThreadTaskInfo(callback, state);
            lock (_poolLock)
            {
                _waitingCallbacks.Enqueue(waiting);
                waiting.TaskStatus = TaskStatus.WaitingToRun;
            }
            _workerThreadNeeded.AddOne();

            return waiting;
        }

        #endregion

        #region Properties
        /// <summary>Gets the number of threads at the disposal of the thread pool.</summary>
        public int MaxThreads { get { return _maxWorkerThreads; } }
        /// <summary>Gets the number of currently active threads in the thread pool.</summary>
        public int ActiveThreads { get { return _inUseThreads; } }
        /// <summary>Gets the number of callback delegates currently waiting in the thread pool.</summary>
        public int WaitingCallbacks { get { lock (_poolLock) { return _waitingCallbacks.Count; } } }
        #endregion

        #region Thread Processing
        // /// <summary>Event raised when there is an exception on a threadpool thread.</summary>
        //public event UnhandledExceptionEventHandler UnhandledException;

        /// <summary>A thread worker function that processes items from the work queue.</summary>
        private void ProcessQueuedItems()
        {
            // Process indefinitely
            while (true)
            {
                _workerThreadNeeded.WaitOne();

                if (_stopPoolRequest)
                    return;

                // Get the next item in the queue.  If there is nothing there, go to sleep
                // for a while until we're woken up when a callback is waiting.
                ThreadTaskInfo callback = null;

                // Try to get the next callback available.  We need to lock on the 
                // queue in order to make our count check and retrieval atomic.
                lock (_poolLock)
                {
                    if (_waitingCallbacks.Count > 0)
                    {
                        try { callback = (ThreadTaskInfo)_waitingCallbacks.Dequeue(); }
                        catch { } // make sure not to fail here
                    }
                }

                if (callback != null)
                {
                    // We now have a callback.  Execute it.  Make sure to accurately
                    // record how many callbacks are currently executing.
                    try
                    {
                        Interlocked.Increment(ref _inUseThreads);
                        callback.TaskStatus = TaskStatus.Running;
                        callback.Callback(callback.State);
                        callback.TaskStatus = TaskStatus.RanToCompletion;
                    }
                    catch (Exception exc)
                    {
                        callback.TaskStatus = TaskStatus.Faulted;
                        //try
                        //{
                        //    UnhandledExceptionEventHandler handler = UnhandledException;
                        //    if (handler != null) handler(typeof(ManagedThreadPool), new UnhandledExceptionEventArgs(exc, false));
                        //}
                        //catch { }
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _inUseThreads);
                    }
                }
            }
        }

        public void StopPool(bool waitTillStops = false)
        {
            _stopPoolRequest = true;

            for (int i = 0; i < _maxWorkerThreads; i++)
            {
                _workerThreadNeeded.AddOne();
            }

            if (waitTillStops)
            {
                WaitTillStops();
            }
        }

        public void WaitTillStops()
        {
            foreach (var threadWrapper in _workerThreads)
            {
                threadWrapper.Join();
            }
        }

        #endregion

        private class ThreadWrapper
        {
            Thread _thread;
            ThreadStart _threadStart;

            private ThreadWrapper(ThreadStart threadStart)
            {
                Initialize(threadStart);
            }

            private void Initialize(ThreadStart threadStart)
            {
                _threadStart = threadStart;
                _thread = new Thread(threadStartWrapper);
                _thread.Name = "ThreadWrapper";
                _thread.IsBackground = true;
                _thread.Start();
            }

            public static ThreadWrapper Create(ThreadStart threadStart)
            {
                return new ThreadWrapper(threadStart);
            }

            public void Join()
            {
                _thread.Join();
            }

            private void threadStartWrapper()
            {
                try
                {
                    _threadStart();
                }
                catch (ThreadAbortException ex)
                {
                    this.ReplaceAbortingThread();
                }
            }

            private void ReplaceAbortingThread()
            {
                Initialize(_threadStart);
            }
        }

        /// <summary>Used to hold a callback delegate and the state for that delegate.</summary>
        private class ThreadTaskInfo : IThreadTaskInfo
        {
            #region Member Variables
            /// <summary>Callback delegate for the callback.</summary>
            private WaitCallback _callback;
            /// <summary>State with which to call the callback delegate.</summary>
            private object _state;
            #endregion

            #region Construction
            /// <summary>Initialize the callback holding object.</summary>
            /// <param name="callback">Callback delegate for the callback.</param>
            /// <param name="state">State with which to call the callback delegate.</param>
            public ThreadTaskInfo(WaitCallback callback, object state)
            {
                _callback = callback;
                _state = state;
                TaskStatus = System.Threading.Tasks.TaskStatus.Created;
            }
            #endregion

            #region Properties
            /// <summary>Gets the callback delegate for the callback.</summary>
            public WaitCallback Callback { get { return _callback; } }
            /// <summary>Gets the state with which to call the callback delegate.</summary>
            public object State { get { return _state; } }
            public TaskStatus TaskStatus { get; set; }
            public bool IsWaitingToRun { get { return this.TaskStatus == System.Threading.Tasks.TaskStatus.WaitingToRun; } }
            public bool IsRunning { get { return this.TaskStatus == System.Threading.Tasks.TaskStatus.Running; } }
            public bool IsCompleted { get { return this.TaskStatus == System.Threading.Tasks.TaskStatus.RanToCompletion; } }
            public bool IsFaulted { get { return this.TaskStatus == System.Threading.Tasks.TaskStatus.Faulted; } }
            #endregion
        }

        private class CompletedThreadTaskInfo : IThreadTaskInfo
        {
            public bool IsWaitingToRun { get { return false; } }
            public bool IsRunning { get { return false; } }
            public bool IsCompleted { get { return true; } }
            public bool IsFaulted { get { return false; } }
        }
    }

    /// <summary>Implementation of Dijkstra's PV Semaphore based on the Monitor class.</summary>
    internal class Semaphore
    {
        #region Member Variables
        /// <summary>The number of units alloted by this semaphore.</summary>
        private int _count;
        /// <summary>Lock for the semaphore.</summary>
        private readonly object _semLock = new object();
        #endregion

        #region Construction
        /// <summary> Initialize the semaphore as a binary semaphore.</summary>
        public Semaphore()
            : this(1)
        {
        }

        /// <summary> Initialize the semaphore as a counting semaphore.</summary>
        /// <param name="count">Initial number of threads that can take out units from this semaphore.</param>
        /// <exception cref="ArgumentException">Throws if the count argument is less than 0.</exception>
        public Semaphore(int count)
        {
            if (count < 0) throw new ArgumentException("Semaphore must have a count of at least 0.", "count");
            _count = count;
        }
        #endregion

        #region Synchronization Operations
        /// <summary>V the semaphore (add 1 unit to it).</summary>
        public void AddOne() { V(); }

        /// <summary>P the semaphore (take out 1 unit from it).</summary>
        public void WaitOne() { P(); }

        /// <summary>P the semaphore (take out 1 unit from it).</summary>
        public void P()
        {
            // Lock so we can work in peace.  This works because lock is actually
            // built around Monitor.
            lock (_semLock)
            {
                // Wait until a unit becomes available.  We need to wait
                // in a loop in case someone else wakes up before us.  This could
                // happen if the Monitor.Pulse statements were changed to Monitor.PulseAll
                // statements in order to introduce some randomness into the order
                // in which threads are woken.
                while (_count <= 0) Monitor.Wait(_semLock, Timeout.Infinite);
                _count--;
            }
        }

        /// <summary>V the semaphore (add 1 unit to it).</summary>
        public void V()
        {
            // Lock so we can work in peace.  This works because lock is actually
            // built around Monitor.
            lock (_semLock)
            {
                // Release our hold on the unit of control.  Then tell everyone
                // waiting on this object that there is a unit available.
                _count++;
                Monitor.Pulse(_semLock);
            }
        }

        /// <summary>Resets the semaphore to the specified count.  Should be used cautiously.</summary>
        public void Reset(int count)
        {
            lock (_semLock) { _count = count; }
        }
        #endregion
    }
}
