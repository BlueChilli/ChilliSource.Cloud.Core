using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    internal interface IManagedThreadPoolItem : IThreadTaskInfo
    {
        WaitCallback Callback { get; }
        object State { get; }
        void SetTaskStatus(TaskStatus taskStatus);
    }
    
    internal interface IThreadTaskInfo
    {
        TaskStatus TaskStatus { get; }
    }
    
    internal static class IThreadTaskInfoExtensions
    {
        public static bool IsWaitingToRun(this IThreadTaskInfo task) { return task.TaskStatus == System.Threading.Tasks.TaskStatus.WaitingToRun; } 
        public static bool IsRunning(this IThreadTaskInfo task) {  return task.TaskStatus == System.Threading.Tasks.TaskStatus.Running; } 
        public static bool IsCompleted(this IThreadTaskInfo task) { return task.TaskStatus == System.Threading.Tasks.TaskStatus.RanToCompletion; } 
        public static bool IsFaulted(this IThreadTaskInfo task) { return task.TaskStatus == System.Threading.Tasks.TaskStatus.Faulted; } 
    }

    internal class ManagedThreadPool
    {
        private readonly int _maxThreads;
        private readonly BlockingCollection<IManagedThreadPoolItem> _queue;
        private readonly CancellationTokenSource _cancelTS;
        private readonly ManagedTaskScheduler _taskScheduler;
        private readonly TaskFactory _taskFactory;
        private ThreadWrapper[] _workerThreads;
        private int _activeThreadCount;

        public ManagedThreadPool(int maxThreads)
        {
            _maxThreads = maxThreads;

            _queue = new BlockingCollection<IManagedThreadPoolItem>();
            _workerThreads = new ThreadWrapper[_maxThreads];
            _cancelTS = new CancellationTokenSource();
            _activeThreadCount = 0;

            for (int i = 0; i < _maxThreads; i++)
            {
                _workerThreads[i] = ThreadWrapper.Create(ProcessQueue, _cancelTS.Token);
            }

            _taskScheduler = new ManagedTaskScheduler(this);
            _taskFactory = new TaskFactory(_taskScheduler);
        }

        public IThreadTaskInfo CreateCompletedTask()
        {
            return CompletedThreadTaskInfo.Instance;
        }

        public IThreadTaskInfo QueueUserWorkItem(WaitCallback callback, object state)
        {
            if (_cancelTS.IsCancellationRequested)
            {
                throw new ApplicationException("This thread pool is not running and cannot be used.");
            }

            IManagedThreadPoolItem item = ThreadTaskInfoFactory.Create(callback, state);
            item.SetTaskStatus(TaskStatus.WaitingToRun);
            _queue.Add(item);

            return item;
        }

        public int MaxThreads { get { return _maxThreads; } }

        public int ActiveThreads { get { return _activeThreadCount; } }

        public int WaitingCount { get { return _queue.Count; } }

        public TaskFactory TaskFactory { get { return _taskFactory; } }
        private BlockingCollection<IManagedThreadPoolItem> Queue => _queue;

        private void ProcessQueue()
        {
            while (!_cancelTS.IsCancellationRequested)
            {
                IManagedThreadPoolItem taskInfo = null;

                try
                {
                    //blocks til a task is available
                    taskInfo = _queue.Take(_cancelTS.Token);
                }
                catch (OperationCanceledException)
                {
                    //Take was cancelled, stop processing tasks
                    break;
                }

                if (taskInfo != null)
                {
                    try
                    {
                        Interlocked.Increment(ref _activeThreadCount);
                        taskInfo.SetTaskStatus(TaskStatus.Running);
                        taskInfo.Callback(taskInfo.State);
                        taskInfo.SetTaskStatus(TaskStatus.RanToCompletion);
                    }
                    catch (Exception ex)
                    {
                        taskInfo.SetTaskStatus(TaskStatus.Faulted);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _activeThreadCount);
                    }
                }
            }
        }

        public void StopPool(bool waitTillStops = false)
        {
            _cancelTS.Cancel();

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

        private class ThreadWrapper
        {
            readonly CancellationToken _ctToken;
            Thread _thread;
            ThreadStart _threadStart;

            private ThreadWrapper(ThreadStart threadStart, CancellationToken ctToken)
            {
                _ctToken = ctToken;
                Initialize(threadStart);
            }

            private void Initialize(ThreadStart threadStart)
            {
                _threadStart = threadStart;
                _thread = new Thread(threadStartWrapper);
                _thread.Name = "ManagedThreadPool_Thread";
                _thread.IsBackground = true;
                _thread.Start();
            }

            public static ThreadWrapper Create(ThreadStart threadStart, CancellationToken ctToken)
            {
                return new ThreadWrapper(threadStart, ctToken);
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
                if (!_ctToken.IsCancellationRequested)
                    Initialize(_threadStart);
            }
        }

        private class CompletedThreadTaskInfo : IThreadTaskInfo
        {
            private CompletedThreadTaskInfo() { }
            public static CompletedThreadTaskInfo Instance { get; } = new CompletedThreadTaskInfo();            

            public TaskStatus TaskStatus => TaskStatus.RanToCompletion;
        }
    }

    internal class ThreadTaskInfoFactory
    {
        internal static IManagedThreadPoolItem Create(WaitCallback callback, object state)
        {
            return new ThreadTaskInfo(callback, state);
        }

        internal static IThreadTaskInfo Create(Task managedTask)
        {
            return new ManagedTaskWrapper(managedTask);
        }

        private class ManagedTaskWrapper: IThreadTaskInfo
        {
            private readonly Task _managedTask;

            public ManagedTaskWrapper(Task managedTask)
            {
                if (managedTask == null)
                    throw new ArgumentNullException("managedTask is null.");

                _managedTask = managedTask;
            }

            public TaskStatus TaskStatus => _managedTask.Status;
        }

        private class ThreadTaskInfo : IThreadTaskInfo, IManagedThreadPoolItem
        {
            private readonly WaitCallback _callback;
            private readonly object _state;

            public ThreadTaskInfo(WaitCallback callback, object state)
            {
                _callback = callback;
                _state = state;
                TaskStatus = System.Threading.Tasks.TaskStatus.Created;
            }

            public void SetTaskStatus(TaskStatus taskStatus)
            {
                this.TaskStatus = taskStatus;
            }

            public WaitCallback Callback { get { return _callback; } }
            public object State { get { return _state; } }
            public TaskStatus TaskStatus { get; private set; }        
        }
    }

    internal class ManagedTaskScheduler : TaskScheduler
    {
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;

        private readonly ManagedThreadPool _threadPool;
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();
        private readonly object _lock = new object();

        public ManagedTaskScheduler(ManagedThreadPool threadPool)
        {
            _threadPool = threadPool;
        }

        protected sealed override void QueueTask(Task task)
        {
            lock (_lock)
            {
                _tasks.AddLast(task);
            }

            _threadPool.QueueUserWorkItem(ProcessWorkItem, null);
        }

        private bool DequeueFirstTask(out Task task)
        {
            lock (_lock)
            {
                if (_tasks.Count == 0)
                {
                    task = null;
                    return false;
                }
                else
                {
                    task = _tasks.First.Value;
                    _tasks.RemoveFirst();

                    return true;
                }
            }
        }

        //This method will run in a _threadPool thread.
        private void ProcessWorkItem(object _)
        {
            _currentThreadIsProcessingItems = true;
            try
            {
                Task task;
                while (DequeueFirstTask(out task))
                {
                    base.TryExecuteTask(task);
                }
            }
            finally
            {
                _currentThreadIsProcessingItems = false;
            }
        }

        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!_currentThreadIsProcessingItems)
                return false;

            if (taskWasPreviouslyQueued)
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        protected sealed override bool TryDequeue(Task task)
        {
            lock (_lock)
            {
                return _tasks.Remove(task);
            }
        }

        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            lock (_lock)
            {
                var copy = new Task[_tasks.Count];
                _tasks.CopyTo(copy, 0);

                return copy;
            }
        }
    }
}