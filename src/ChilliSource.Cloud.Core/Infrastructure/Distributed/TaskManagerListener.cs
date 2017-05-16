using ChilliSource.Cloud.Core.Distributed;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace ChilliSource.Cloud.Core.Distributed
{
    internal class TaskManagerListener
    {
        TaskManager _taskManager;
        Thread _callbackThread;
        Action _listenerTickCallback = null;
        readonly object _localLock = new object();
        int _mainLoopWaitTime;
        int _maxWorkerThreads;
        ManagedThreadPool _managedThreadPool = null;

        ManualResetEvent _listenerEndedSignal = null;
        ManualResetEvent _listenerStartedSignal = null;
        CancellationTokenSource _listenerCtSource = null;

        public TaskManagerListener(TaskManager taskManager, int mainLoopWaitTime, int maxWorkerThreads)
        {
            _taskManager = taskManager;
            _mainLoopWaitTime = mainLoopWaitTime;
            _maxWorkerThreads = maxWorkerThreads;
        }

        public Exception LatestListenerException { get; private set; }
        int _tasksExecutedCount;
        public int TasksExecutedCount { get { return _tasksExecutedCount; } }

        public void StartListener(int delay)
        {
            if (_listenerStartedSignal != null)
                return;

            if (delay > 0)
            {
                Task.Run(() => StartListenerInternal(delay));
            }
            else
            {
                StartListenerInternal(delay);
            }
        }

        private void StartListenerInternal(int delay)
        {
            Thread.Sleep(delay);
            lock (_localLock)
            {
                if (_listenerStartedSignal != null)
                    return;

                var startedSignal = _listenerStartedSignal = new ManualResetEvent(false);

                var enqueued = false;
                try
                {
                    var hostingEnvironment = GlobalConfiguration.Instance.GetHostingEnvironment(throwIfNotSet: false);
                    //Tries to initialise task using HostingEnvironment
                    if (hostingEnvironment != null)
                    {
                        hostingEnvironment.QueueBackgroundWorkItem((Action<CancellationToken>)Listener_ThreadStart);
                        enqueued = true;
                    }
                }
                catch (ThreadAbortException ex)
                {
                    return;
                }
                catch (Exception ex)
                {
                    ex.LogException();
                }

                if (!enqueued)
                {
                    //Fall back initialisation
                    ThreadPool.QueueUserWorkItem((object state) => Listener_ThreadStart(CancellationToken.None));
                }

                startedSignal.WaitOne();
            }
        }

        public void StopListener()
        {
            var ctSoutce = _listenerCtSource;
            if (ctSoutce == null || ctSoutce.IsCancellationRequested)
                return;

            lock (_localLock)
            {
                ctSoutce = _listenerCtSource;
                if (ctSoutce == null || ctSoutce.IsCancellationRequested)
                    return;

                ctSoutce.Cancel();

                ResumeCallbackThread();
            }
        }

        public void JoinListener()
        {
            var endSignal = _listenerEndedSignal;
            if (endSignal != null)
                endSignal.WaitOne();
        }

        public void SubscribeToListener(Action action)
        {
            this._listenerTickCallback += action;
        }

        private static readonly TimeSpan OneMinute = new TimeSpan(TimeSpan.TicksPerMinute);
        private static readonly TimeSpan TwoMinutes = new TimeSpan(TimeSpan.TicksPerMinute * 2);
        private static readonly TimeSpan ThreeSeconds = new TimeSpan(TimeSpan.TicksPerSecond * 3);


        internal void Listener_ThreadStart(CancellationToken ctToken)
        {
            try
            {
                _managedThreadPool = new ManagedThreadPool(_maxWorkerThreads);
                _listenerEndedSignal = new ManualResetEvent(false);
                _listenerCtSource = CancellationTokenSource.CreateLinkedTokenSource(ctToken);

                //_listenerCtSource.Token.Register(() => { /* for debugging purposes. */ });

                _callbackThread = new Thread(Callback_ThreadStart);
                _callbackThread.IsBackground = false;
                _callbackThread.Start();

                _tasksExecutedCount = 0;

                PurgeRecurrentLogs(); //Extreme scenario. Also cleans one-off tasks that may have been added.

                _listenerStartedSignal.Set();

                while (!_listenerCtSource.IsCancellationRequested)
                {
                    try
                    {
                        try
                        {
                            Thread.Sleep(_mainLoopWaitTime);
                        }
                        catch (ThreadAbortException ex)
                        {
                            //Ignores thread abort exceptions while sleeping and quits
                            break;
                        }

                        CleanupAndRescheduleTasks();

                        var taskInfos = ProcessPendingTasks(_managedThreadPool.MaxThreads);

                        while (ManageTasksLifeTime(taskInfos) > 0)
                        {
                            //Allows other threads to execute
                            Thread.Sleep(1);
                        }

                        //Only counts tasks that acquired lock
                        _tasksExecutedCount += taskInfos.Where(t => t.RealTaskInvokedFlag).Count();

                        //Release all task locks
                        foreach (var taskInfo in taskInfos)
                        {
                            _taskManager.LockManager.Release(taskInfo.LockInfo);
                            taskInfo.Dispose();
                        }

                        ResumeCallbackThread();
                    }
                    catch (Exception ex)
                    {
                        LatestListenerException = ex;
                        ex.LogException();
                    }
                }
            }
            finally
            {
                _listenerStartedSignal = null;

                var pool = _managedThreadPool;
                if (pool != null)
                {
                    _managedThreadPool = null;
                    pool.StopPool(waitTillStops: true);
                }

                var endSignal = _listenerEndedSignal;

                if (endSignal != null)
                {
                    _listenerEndedSignal = null;
                    endSignal.Set();
                }
            }
        }

        ManualResetEvent _callbackSignal = new ManualResetEvent(false);
        private void ResumeCallbackThread()
        {
            _callbackSignal.Set();
        }

        private void Callback_ThreadStart()
        {
            while (true)
            {
                try
                {
                    //Waits for a signal and set it to false for the next loop cycle
                    _callbackSignal.WaitOne();
                    _callbackSignal.Reset();

                    if (_listenerCtSource == null || _listenerCtSource.IsCancellationRequested)
                        break;


                    if (this._listenerTickCallback != null)
                    {
                        this._listenerTickCallback();
                    }
                }
                catch (ThreadAbortException ex)
                {
                    return; /* bugfix - don't log this exception */
                }
                catch (Exception ex) { ex.LogException(); }

                Thread.Sleep(1);
            }
        }

        private static readonly Guid READ_PENDING_TASKS_LOCK = new Guid("9325897C-0D87-418C-8473-24505657EC51");
        private static readonly TaskExecutionInfo[] _EmtpyPendingTasks = new TaskExecutionInfo[0];

        private IList<TaskExecutionInfo> ProcessPendingTasks(int qty)
        {
            LockInfo lockInfo = null;
            IList<TaskExecutionInfo> list = _EmtpyPendingTasks;

            try
            {
                //Inter-Process lock
                if (_taskManager.LockManager.TryLock(READ_PENDING_TASKS_LOCK, OneMinute, out lockInfo))
                {
                    List<SingleTaskDefinition> pendingTasks;

                    using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                    using (var context = _taskManager.CreateRepository())
                    {
                        pendingTasks = context.SingleTasks.AsNoTracking().Where(t =>
                                                    (t.Status == Distributed.SingleTaskStatus.Scheduled && t.ScheduledAt < DateTime.UtcNow))
                                                .OrderBy(t => t.ScheduledAt).ThenBy(t => t.Id)
                                                .Take(qty).ToList();
                    }

                    list = pendingTasks.Select(t => ProcessTaskDefinition(t)).ToList();
                }
            }
            finally
            {
                try { _taskManager.LockManager.Release(lockInfo); }
                catch (Exception ex) { ex.LogException(); }
            }

            return list;
        }

        private static readonly Guid RECURRENT_SINGLE_TASK_LOCK = new Guid("ACA1DEDE-2DF7-4E6B-AB55-A938B65C1E06");

        internal class RecurrentTaskProjection
        {
            public RecurrentTaskDefinition RecurrentTask { get; set; }
            public SingleTaskDefinition LatestSingleTask { get; set; }
        }

        private void CleanupAndRescheduleTasks()
        {
            List<RecurrentTaskProjection> recurrentTasks;
            LockInfo lockInfo = null;

            CleanupTasks();

            try
            {
                //Inter-Process lock
                if (_taskManager.LockManager.TryLock(RECURRENT_SINGLE_TASK_LOCK, OneMinute, out lockInfo))
                {
                    using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                    using (var context = _taskManager.CreateRepository())
                    {
                        recurrentTasks = context.RecurrentTasks.AsNoTracking()
                                                .Where(r => r.Enabled)
                                                .Select(r => new RecurrentTaskProjection()
                                                {
                                                    RecurrentTask = r,
                                                    LatestSingleTask = r.SingleTasks.OrderByDescending(t => t.ScheduledAt).Take(1).FirstOrDefault()
                                                }).Where(a => a.LatestSingleTask == null
                                                             || (a.LatestSingleTask.Status != Distributed.SingleTaskStatus.Scheduled && a.LatestSingleTask.Status != Distributed.SingleTaskStatus.Running))
                                                .ToList();
                    }

                    foreach (var projection in recurrentTasks)
                    {
                        var taskInfo = _taskManager.GetTaskInfo(projection.RecurrentTask.Identifier);
                        if (taskInfo == null)
                            continue;

                        var interval = projection.RecurrentTask.Interval;
                        if (projection.LatestSingleTask == null || projection.LatestSingleTask.StatusChangedAt.AddMilliseconds(interval) < DateTime.UtcNow)
                        {
                            _taskManager.EnqueueSingleTask(taskInfo.Identifier, recurrentTaskId: projection.RecurrentTask.Id, delay: 0);
                        }
                    }
                }
            }
            finally
            {
                try { _taskManager.LockManager.Release(lockInfo); }
                catch (Exception ex) { ex.LogException(); }
            }
        }

        //Monitors tasks
        private int ManageTasksLifeTime(IList<TaskExecutionInfo> taskInfos)
        {
            var ctSource = _listenerCtSource;
            var runningCount = 0;

            foreach (var taskInfo in taskInfos.Where(i => i.IsTaskRunningOrWaiting()))
            {
                runningCount++;

                //If lock has not been acquired yet, skip it (because the THREAD hasn't started)     
                //Also skip it, if it's known that the lock is about to be released by the task (LockWillBeReleasedFlag - when it's finalizing)
                if (!taskInfo.RealTaskInvokedFlag || taskInfo.LockWillBeReleasedFlag)
                    continue;

                var lockState = taskInfo.LockInfo.AsImmutable();
                //Half [LockInfo.Timeout] period has passed
                if (lockState.HasLock && lockState.IsLockHalfTimePassed())
                {
                    RenewTaskLock(taskInfo);
                }

                //refreshes lock info state
                lockState = taskInfo.LockInfo.AsImmutable();

                //Sends Cancel Signal if no alive signal has been received in the last HALF [LockInfo.Timeout] period
                //The task will have the other HALF [LockInfo.Timeout] period to finish, or it will be aborted.
                if (lockState.HasLock
                    && ((ctSource != null && ctSource.IsCancellationRequested) || !taskInfo.IsSignaledAlive(lockState.HalfTimeout)))
                {
                    taskInfo.SignalCancelTask();
                }

                //refreshes lock info state
                lockState = taskInfo.LockInfo.AsImmutable();

                //If acquired and lost lock;
                //Or not alive: force cancel task.                
                if (!lockState.HasLock || !taskInfo.IsSignaledAlive(lockState.Timeout))
                {
                    taskInfo.ForceCancelTask();
                }
            }

            return runningCount;
        }

        private void RenewTaskLock(TaskExecutionInfo taskInfo)
        {
            //renews lock
            if (_taskManager.LockManager.TryRenewLock(taskInfo.LockInfo))
            {
                var lockedUntil = taskInfo.LockInfo.AsImmutable().LockedUntil;
                if (lockedUntil == null)
                    return;

                using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                using (var context = _taskManager.CreateRepository())
                {
                    var data = context.SingleTasks
                                      .Where(t => t.Id == taskInfo.TaskDefinitionId && t.LastRunAt == taskInfo.LastRunAt && t.Status == Distributed.SingleTaskStatus.Running).FirstOrDefault();
                    if (data != null)
                    {
                        data.LockedUntil = lockedUntil;

                        try
                        {
                            context.SaveChanges();
                        }
                        catch (Exception ctxEx)
                        {
                            ctxEx.LogException();
                        }
                    }
                }
            }
        }

        private readonly static string COUNT_SINGLE_TASKS_SQL = $"SELECT COUNT(*) from dbo.SingleTasks";
        private readonly static string TRUNCATE_SINGLE_TASKS_SQL = $"TRUNCATE TABLE dbo.SingleTasks";

        private const long PURGE_ALL_THRESHOLD = 50000;

        private void PurgeRecurrentLogs()
        {
            LockInfo lockInfo = null;

            try
            {
                //Inter-Process lock
                if (_taskManager.LockManager.WaitForLock(RECURRENT_SINGLE_TASK_LOCK, TwoMinutes, TwoMinutes, out lockInfo))
                {
                    using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                    using (var conn = _taskManager.CreateConnection())
                    using (var countCmd = DbAccessHelper.CreateDbCommand(conn, COUNT_SINGLE_TASKS_SQL))
                    {
                        var count = Convert.ToInt64(countCmd.ExecuteScalar());
                        if (count > PURGE_ALL_THRESHOLD)
                        {
                            using (var truncateCmd = DbAccessHelper.CreateDbCommand(conn, TRUNCATE_SINGLE_TASKS_SQL))
                            {
                                truncateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ex.LogException(); }
            finally
            {
                try { _taskManager.LockManager.Release(lockInfo); }
                catch (Exception ex) { ex.LogException(); }
            }
        }

        private readonly static string FIX_ABANDONED_TASK_SQL = String.Format("UPDATE dbo.SingleTasks SET [Status] = {0}, StatusChangedAt = SYSUTCDATETIME() WHERE ScheduledAt < SYSUTCDATETIME()" +
                                                                              " AND ScheduledAt > DATEADD(day, -7, SYSUTCDATETIME()) AND [Status] = {1} AND LockedUntil IS NOT NULL AND LockedUntil < SYSUTCDATETIME();",
                                                                                (int)Distributed.SingleTaskStatus.CompletedAbandoned, (int)Distributed.SingleTaskStatus.Running);

        private static readonly int MAX_RECURRENT_LOG = 100;

        //Leave at most MAX_RECURRENT_LOG SingleTask records for each RecurrentTask, delete all other records.
        private static readonly string CLEANUP_RECCURENT_LOG_SQL =
            $@"DELETE FROM [dbo].[SingleTasks] 
                WHERE  Id in (Select Skip1.Id from 
                 (SELECT DISTINCT [Extent2].[RecurrentTaskId] AS [RecurrentTaskId] 
	                FROM [dbo].[SingleTasks] AS [Extent2] 
	                WHERE ({(int)Distributed.SingleTaskStatus.Running} <> [Extent2].[Status]) AND ({(int)Distributed.SingleTaskStatus.Scheduled} <> [Extent2].[Status]) AND ([Extent2].[RecurrentTaskId] IS NOT NULL) 
                  ) AS [Distinct1] 
                  CROSS APPLY  
                  (SELECT [Project2].[Id] AS [Id] 
                            FROM ( SELECT [Extent3].[Id] AS [Id], [Extent3].[ScheduledAt] AS [ScheduledAt] 
                                    FROM [dbo].[SingleTasks] AS [Extent3] 
                                   WHERE ({(int)Distributed.SingleTaskStatus.Running} <> [Extent3].[Status]) AND ({(int)Distributed.SingleTaskStatus.Scheduled} <> [Extent3].[Status]) AND ([Distinct1].[RecurrentTaskId] = [Extent3].[RecurrentTaskId]) 
                                  )  AS [Project2] 
                            ORDER BY [Project2].[ScheduledAt] DESC, [Project2].[Id] DESC 
                            OFFSET {MAX_RECURRENT_LOG} ROWS 
                    ) 
	                AS [Skip1]);";

        private DateTime CleanupLastRunAt = DateTime.MinValue;
        private void CleanupTasks()
        {
            using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            using (var conn = _taskManager.CreateConnection())
            using (var abandonedCmd = DbAccessHelper.CreateDbCommand(conn, FIX_ABANDONED_TASK_SQL))
            {
                abandonedCmd.ExecuteNonQuery();

                var now = DateTime.UtcNow;
                if (CleanupLastRunAt > now) //datetime change
                    CleanupLastRunAt = DateTime.MinValue;

                //Runs every minute
                if (now.Subtract(CleanupLastRunAt) > OneMinute)
                {
                    CleanupLastRunAt = now;
                    using (var cleanupCmd = DbAccessHelper.CreateDbCommand(conn, CLEANUP_RECCURENT_LOG_SQL))
                    {
                        cleanupCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private readonly static string SET_RUNNING_STATUS_SQL = String.Format("UPDATE dbo.SingleTasks SET [Status] = {0}, StatusChangedAt = SYSUTCDATETIME(), LastRunAt = @LastRunAt, LockedUntil = @LockedUntil" +
                                                                              " WHERE Id = @Id AND [Status] = {1}",
                                                                              (int)Distributed.SingleTaskStatus.Running, (int)Distributed.SingleTaskStatus.Scheduled);

        public TaskExecutionInfo ProcessTaskDefinition(SingleTaskDefinition taskDefinition)
        {
            var lockInfo = LockInfo.Empty(taskDefinition.Identifier);
            var taskTypeInfo = _taskManager.GetTaskInfo(taskDefinition.Identifier);
            //links this task's cancellation token to the manager's token
            var ctSource = CancellationTokenSource.CreateLinkedTokenSource(_listenerCtSource.Token);

            try
            {
                //Inter-process lock. Ignores the task if it doesn't acquire lock
                if (taskTypeInfo == null || !_taskManager.LockManager.TryRenewLock(lockInfo, taskTypeInfo.LockCycle, retryLock: true))
                {
                    return new TaskExecutionInfo(_managedThreadPool.CreateCompletedTask(), ctSource, lockInfo, taskDefinition);
                }

                var executionInfoLocal = new TaskExecutionInfo(null, ctSource, lockInfo, taskDefinition);

                var taskBody = CreateTaskBody(executionInfoLocal, taskTypeInfo);
                var task = _managedThreadPool.QueueUserWorkItem((o) => taskBody(), null);

                executionInfoLocal.SetTask(task);

                return executionInfoLocal;
            }
            catch (Exception ex)
            {
                ex.LogException();

                return new TaskExecutionInfo(_managedThreadPool.CreateCompletedTask(), ctSource, lockInfo, taskDefinition);
            }
        }

        private Action CreateTaskBody(TaskExecutionInfo executionInfoLocal, TaskTypeInfo taskTypeInfo)
        {
            return () =>
            {
                try
                {
                    executionInfoLocal.SendAliveSignal();
                    executionInfoLocal.SetTaskThread(Thread.CurrentThread);

                    if (executionInfoLocal.CancellationTokenSource.IsCancellationRequested)
                        return;

                    if (!SetRunningStatus(executionInfoLocal, taskTypeInfo))
                        return;

                    executionInfoLocal.SendAliveSignal();

                    //Flags executionInfoLocal right before invoking the task implementation
                    executionInfoLocal.RealTaskInvokedFlag = true;
                    taskTypeInfo.Invoke(executionInfoLocal.TaskDefinition.JsonParameters, executionInfoLocal);

                    executionInfoLocal.SendAliveSignal();

                    SetCompletedOrCancelledStatus(executionInfoLocal, taskTypeInfo);
                }
                catch (ThreadAbortException ex)
                {
                    //Tries to save information about the task status.
                    //Reverts ThreadAbort
                    Thread.ResetAbort();

                    SetAbortedStatus(executionInfoLocal);
                }
                catch (Exception ex)
                {
                    ex.LogException();
                }
                finally
                {
                    try
                    {
                        //See comment below
                        executionInfoLocal.LockWillBeReleasedFlag = true;
                        _taskManager.LockManager.Release(executionInfoLocal.LockInfo);

                        //**** LockWillBeReleasedFlag avoids having the task being aborted right here when it's about to end, because we just released the lock
                        // and the lifetime manager could try to cancel it forcefully.
                    }
                    catch (Exception ex)
                    {
                        ex.LogException();
                    }
                }
            };
        }

        private void SetAbortedStatus(TaskExecutionInfo executionInfoLocal)
        {
            var lockInfo = executionInfoLocal.LockInfo;
            var taskDefinition = executionInfoLocal.TaskDefinition;

            if (!_taskManager.LockManager.TryRenewLock(lockInfo, retryLock: true))
                return;

            using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            using (var context = _taskManager.CreateRepository())
            {
                var data = context.SingleTasks
                                .Where(t => t.Id == taskDefinition.Id && t.LastRunAt == executionInfoLocal.LastRunAt && t.Status == Distributed.SingleTaskStatus.Running).FirstOrDefault();

                if (data != null)
                {
                    data.SetStatus(Distributed.SingleTaskStatus.CompletedAborted);

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (Exception ctxEx)
                    {
                        ctxEx.LogException();
                    }
                }
            }
        }

        private void SetCompletedOrCancelledStatus(TaskExecutionInfo executionInfoLocal, TaskTypeInfo taskTypeInfo)
        {
            var lockInfo = executionInfoLocal.LockInfo;
            var taskDefinition = executionInfoLocal.TaskDefinition;

            using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            using (var context = _taskManager.CreateRepository())
            {
                if (_taskManager.LockManager.TryRenewLock(lockInfo, taskTypeInfo.LockCycle, retryLock: true))
                {
                    var data = context.SingleTasks
                                    .Where(t => t.Id == taskDefinition.Id && t.LastRunAt == executionInfoLocal.LastRunAt && t.Status == Distributed.SingleTaskStatus.Running).FirstOrDefault();
                    if (data != null)
                    {
                        data.SetStatus(executionInfoLocal.IsCancellationRequested ? Distributed.SingleTaskStatus.CompletedCancelled : Distributed.SingleTaskStatus.Completed);

                        try
                        {
                            context.SaveChanges();
                        }
                        catch (Exception ctxEx)
                        {
                            ctxEx.LogException();
                        }
                    }
                }
            }
        }

        private bool SetRunningStatus(TaskExecutionInfo executionInfoLocal, TaskTypeInfo taskTypeInfo)
        {
            var lockInfo = executionInfoLocal.LockInfo;
            var taskDefinition = executionInfoLocal.TaskDefinition;

            //It may be that the task scheduler took too long to start the thread.
            //Renews the lock. Ignores the task if it doesn't acquire lock
            if (!_taskManager.LockManager.TryRenewLock(lockInfo, taskTypeInfo.LockCycle, retryLock: true))
                return false;

            using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            using (var conn = _taskManager.CreateConnection())
            {
                var now = DateTime.UtcNow;
                //precision up to milliseconds.
                var lastRunAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
                var lockState = lockInfo.AsImmutable();

                using (var command = DbAccessHelper.CreateDbCommand(conn, SET_RUNNING_STATUS_SQL))
                {
                    command.Parameters.Add(new SqlParameter("Id", taskDefinition.Id));
                    command.Parameters.Add(new SqlParameter("LastRunAt", System.Data.SqlDbType.DateTime2) { Value = lastRunAt });
                    command.Parameters.Add(new SqlParameter("LockedUntil", System.Data.SqlDbType.DateTime2) { Value = lockState.LockedUntil });

                    //Has the task been deleted or already handled?
                    if (command.ExecuteNonQuery() != 1)
                        return false;

                    executionInfoLocal.LastRunAt = lastRunAt;

                    return true;
                }
            }
        }

        internal bool IsListenning()
        {
            return _listenerStartedSignal != null;
        }
    }
}
