using System;
using System.Collections.Concurrent;
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

    internal class ManagedThreadPool
    {
        private readonly int _maxThreads;
        private readonly BlockingCollection<ThreadTaskInfo> _queue;
        private readonly CancellationTokenSource _cancelTS;

        private ThreadWrapper[] _workerThreads;
        private int _activeThreadCount;

        public ManagedThreadPool(int maxThreads)
        {
            _maxThreads = maxThreads;

            _queue = new BlockingCollection<ThreadTaskInfo>();
            _workerThreads = new ThreadWrapper[_maxThreads];
            _cancelTS = new CancellationTokenSource();
            _activeThreadCount = 0;

            for (int i = 0; i < _maxThreads; i++)
            {
                _workerThreads[i] = ThreadWrapper.Create(ProcessQueue, _cancelTS.Token);
            }
        }

        public IThreadTaskInfo CreateCompletedTask()
        {
            return new CompletedThreadTaskInfo();
        }

        public IThreadTaskInfo QueueUserWorkItem(WaitCallback callback, object state)
        {
            if (_cancelTS.IsCancellationRequested)
            {
                throw new ApplicationException("This thread pool is not running and cannot be used.");
            }

            ThreadTaskInfo item = new ThreadTaskInfo(callback, state)
            {
                TaskStatus = TaskStatus.WaitingToRun
            };
            _queue.Add(item);

            return item;
        }

        public int MaxThreads { get { return _maxThreads; } }

        public int ActiveThreads { get { return _activeThreadCount; } }

        public int WaitingCount { get { return _queue.Count; } }

        private BlockingCollection<ThreadTaskInfo> Queue => _queue;

        private void ProcessQueue()
        {
            while (!_cancelTS.IsCancellationRequested)
            {
                ThreadTaskInfo taskInfo = null;

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
                        taskInfo.TaskStatus = TaskStatus.Running;
                        taskInfo.Callback(taskInfo.State);
                        taskInfo.TaskStatus = TaskStatus.RanToCompletion;
                    }
                    catch (Exception ex)
                    {
                        taskInfo.TaskStatus = TaskStatus.Faulted;
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

        private class ThreadTaskInfo : IThreadTaskInfo
        {
            private WaitCallback _callback;
            private object _state;

            public ThreadTaskInfo(WaitCallback callback, object state)
            {
                _callback = callback;
                _state = state;
                TaskStatus = System.Threading.Tasks.TaskStatus.Created;
            }

            public WaitCallback Callback { get { return _callback; } }
            public object State { get { return _state; } }
            public TaskStatus TaskStatus { get; set; }
            public bool IsWaitingToRun { get { return this.TaskStatus == System.Threading.Tasks.TaskStatus.WaitingToRun; } }
            public bool IsRunning { get { return this.TaskStatus == System.Threading.Tasks.TaskStatus.Running; } }
            public bool IsCompleted { get { return this.TaskStatus == System.Threading.Tasks.TaskStatus.RanToCompletion; } }
            public bool IsFaulted { get { return this.TaskStatus == System.Threading.Tasks.TaskStatus.Faulted; } }
        }

        private class CompletedThreadTaskInfo : IThreadTaskInfo
        {
            public bool IsWaitingToRun { get { return false; } }
            public bool IsRunning { get { return false; } }
            public bool IsCompleted { get { return true; } }
            public bool IsFaulted { get { return false; } }
        }
    }
}
