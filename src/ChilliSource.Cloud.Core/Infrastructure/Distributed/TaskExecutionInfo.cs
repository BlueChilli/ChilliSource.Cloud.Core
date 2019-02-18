#if NET_4X
using ChilliSource.Cloud.Core.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Distributed
{
    /// <summary>
    /// Tracks information about the task health.
    /// </summary>
    public interface ITaskExecutionInfo
    {
        /// <summary>
        /// ITaskExecutionInfo.IsCancellationRequested MUST be read periodically.<br/>
        /// If cancellation has been requested, the task MUST end as soon as possible.<br/>
        /// </summary>
        bool IsCancellationRequested { get; }
        /// <summary>
        /// ITaskExecutionInfo.SendAliveSignal() MUST be called periodically to ensure the task is kept alive.
        /// </summary>
        void SendAliveSignal();
    }

    internal class TaskExecutionInfo : ITaskExecutionInfo, IDisposable
    {
        IThreadTaskInfo _task;
        Thread _taskThread;
        CancellationTokenSource _cancelTokenSource;
        LockInfo _lockInfo;
        SingleTaskDefinition _taskDefinition;

        DateTime? _lastAliveSignalAt;
        readonly object _localLock = new object();
        List<CancellationTokenRegistration> _cancelRegisters = new List<CancellationTokenRegistration>();

        internal TaskExecutionInfo(IThreadTaskInfo task, CancellationTokenSource cancelTokenSource, LockInfo lockInfo, SingleTaskDefinition taskDefinition)
        {
            if (lockInfo == null)
                throw new ArgumentNullException("lockInfo is null.");
            if (taskDefinition == null)
                throw new ArgumentNullException("taskDefinition is null.");
            if (cancelTokenSource == null)
                throw new ArgumentNullException("cancelTokenSource is null.");

            //Task may be null when first initialized.
            _task = task;
            _lockInfo = lockInfo;
            _taskDefinition = taskDefinition;
            _cancelTokenSource = cancelTokenSource;
            _lastAliveSignalAt = null;
        }

        internal SingleTaskDefinition TaskDefinition { get { return _taskDefinition; } }

        internal int TaskDefinitionId { get { return _taskDefinition.Id; } }

        internal DateTime? LastRunAt { get; set; }

        internal void SetTask(IThreadTaskInfo task)
        {
            _task = task;
        }

        internal LockInfo LockInfo { get { return _lockInfo; } }
        internal CancellationTokenSource CancellationTokenSource { get { return _cancelTokenSource; } }

        public bool IsCancellationRequested { get { return _cancelTokenSource.IsCancellationRequested; } }

        public void SendAliveSignal()
        {
            //If cancellation has been requested. Alive signal is ignored.
            if (IsCancellationRequested)
                return;

            _lastAliveSignalAt = DateTime.UtcNow;
        }

        internal bool IsSignaledAlive(TimeSpan period)
        {
            return _lastAliveSignalAt != null && _lastAliveSignalAt.Value.Add(period) > DateTime.UtcNow;
        }

        public void RegisterCancelAction(Action action, bool useSynchronizationContext = false)
        {
            var register = _cancelTokenSource.Token.Register(action, useSynchronizationContext);
            lock (_localLock)
            {
                _cancelRegisters.Add(register);
            }
        }

        bool isCancelledCalled = false;
        internal void SignalCancelTask()
        {
            if (isCancelledCalled)
                return;

            lock (_localLock)
            {
                if (isCancelledCalled)
                    return;

                isCancelledCalled = true;

                try
                {
                    _cancelTokenSource.Cancel();
                }
                catch (Exception ex)
                {
                    ex.LogException();
                }
            }
        }

        bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                _cancelRegisters.ForEach(r => r.Dispose());
                _cancelRegisters.Clear();

                _cancelTokenSource.Dispose();
                _cancelTokenSource = null;

                _task = null;
                _taskThread = null;
            }
        }

        internal void SetTaskThread(Thread thread)
        {
            _taskThread = thread;
        }

        internal bool IsTaskRunningOrWaiting()
        {
            if (_task == null)
                return false;

            return !_task.IsCompleted && !_task.IsFaulted;
        }

        bool aborted = false;
        internal void ForceCancelTask()
        {
            if (!IsTaskRunningOrWaiting() || aborted)
                return;

            //Has the task thread started yet?
            if (_taskThread != null)
            {
                try
                {
                    aborted = true;
                    _taskThread.Abort();
                }
                catch (Exception ex)
                {
                    ex.LogException();
                }
            }
            else
            {
                this.SignalCancelTask();
            }
        }

        internal bool RealTaskInvokedFlag { get; set; }
        internal bool LockWillBeReleasedFlag { get; set; }
    }
}
#endif