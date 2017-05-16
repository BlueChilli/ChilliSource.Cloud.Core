using ChilliSource.Cloud.Core.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ChilliSource.Cloud.Core.Distributed
{
    /// <summary>
    /// Allows customization of the task manager.
    /// </summary>
    public sealed class TaskManagerOptions
    {
        int _mainLoopWait = 10000;
        /// <summary>
        /// Interval between tasks' definition reads. It defaults to 10000 milliseconds
        /// </summary>
        public int MainLoopWait
        {
            get { return _mainLoopWait; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("MainLoopWait value has to be at least 1 millisecond.");

                _mainLoopWait = value;
            }
        }

        int _maxWorkerThreads = 7;
        /// <summary>
        ///  Maximum number of concurrent threads to process tasks. It defaults to 7 threads.
        /// </summary>
        public int MaxWorkerThreads
        {
            get { return _maxWorkerThreads; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("MaxWorkerThreads value has to be at least 1.");

                _maxWorkerThreads = value;
            }
        }

        /// <summary>
        /// Default Options object
        /// </summary>
        public static TaskManagerOptions Default
        {
            get { return new TaskManagerOptions(); }
        }
    }

    /// <summary>
    /// Represents a distributed (cross-machine or process) task manager.
    /// </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// Links a task GUID with a type definition.
        /// </summary>
        /// <param name="type">A Task type implementation. It must implement IDistributedTask&lt;p&gt;.</param>
        /// <param name="settings">A task settings instance containing the task GUID.</param>
        void RegisterTaskType(Type type, TaskSettings settings);

        /// <summary>
        /// Enqueues a single task schedule.
        /// </summary>
        /// <typeparam name="T">A task type implementation. It needs to be registered via RegisterTaskType().</typeparam>
        /// <param name="delay">How long to wait (in milliseconds) before running this task.</param>
        /// <returns>Returns the id a newly created task schedule.</returns>
        int EnqueueSingleTask<T>(long delay = 0);

        /// <summary>
        /// Enqueues a single task schedule.
        /// </summary>
        /// <param name="identifier">Identifier previously registered via RegisterTaskType.</param>
        /// <param name="delay">How long to wait (in milliseconds) before running this task.</param>
        /// <returns>Returns the id a newly created task schedule.</returns>    
        int EnqueueSingleTask(Guid identifier, long delay = 0);

        /// <summary>
        /// Enqueues a single task schedule with a parameter value.
        /// </summary>
        /// <typeparam name="T">A task type implementation. It needs to be registered via RegisterTaskType().</typeparam>
        /// <typeparam name="P">A paramter type. It must have a parameterless constructor</typeparam>
        /// <param name="parameter">The parameter value.</param>
        /// <param name="delay">How long to wait (in milliseconds) before running this task.</param>
        /// <returns>Returns the id a newly created task schedule.</returns>
        int EnqueueSingleTask<T, P>(P parameter, long delay = 0) where T : IDistributedTask<P>;

        /// <summary>
        /// Enqueues a single task schedule with a parameter value.
        /// </summary>
        /// <param name="identifier">Identifier previously registered via RegisterTaskType.</param>
        /// <typeparam name="P">A paramter type. It must have a parameterless constructor</typeparam>
        /// <param name="parameter">The parameter value.</param>
        /// <param name="delay">How long to wait (in milliseconds) before running this task.</param>
        /// <returns>Returns the id a newly created task schedule.</returns>
        int EnqueueSingleTask<P>(Guid identifier, P parameter, long delay = 0);

        /// <summary>
        /// Removes a single task schedule if it is not executed yet.
        /// </summary>
        /// <param name="id">The task Id.</param>
        /// <returns>Returns whether the task was removed.</returns>
        bool RemoveSingleTask(int id);

        /// <summary>
        /// Enqueues a recurrent task.
        /// </summary>
        /// <typeparam name="T">A task type implementation. It needs to be registered via RegisterTaskType().</typeparam>
        /// <param name="interval">The interval between single task executions.</param>
        /// <returns>Returns the id a newly created recurrent task.</returns>
        int EnqueueRecurrentTask<T>(long interval);

        /// <summary>
        /// Enqueues a recurrent task.
        /// </summary>
        /// <param name="identifier">Identifier previously registered via RegisterTaskType.</param>
        /// <param name="interval">The interval between single task executions.</param>
        /// <returns>Returns the id a newly created recurrent task.</returns>
        int EnqueueRecurrentTask(Guid identifier, long interval);

        /// <summary>
        /// Starts listenning to the queue of tasks and processes them.
        /// </summary>
        /// <param name="delay">Delay start value in milliseconds</param>
        void StartListener(int delay = 0);

        /// <summary>
        /// Runs until the end of the next listener cycle, and stops reading further tasks.
        /// </summary>
        /// <param name="waitTillStops">Specifies whether to halt the current thread until the listener stops.</param>
        void StopListener(bool waitTillStops = false);

        /// <summary>
        /// Halts the current thread until the listener stops
        /// </summary>
        void WaitTillListenerStops();

        /// <summary>
        /// Subscribes a callback action to the end of every listener cycle.
        /// All callbacks are executed on a single thread (but separate from the listener).
        /// </summary>
        /// <param name="action"></param>
        void SubscribeToListener(Action action);

        /// <summary>
        /// Returns whether the manager is listenning to new tasks.
        /// </summary>        
        bool IsListenning { get; }

        /// <summary>
        /// Returns the latest listener exception (if any)
        /// </summary>
        Exception LatestListenerException { get; }

        /// <summary>
        /// Returns the total amount of tasks executed by the manager.
        /// </summary>
        int TasksExecutedCount { get; }
    }

    /// <summary>
    /// Provides a way to create ITaskManager instances.
    /// </summary>
    public class TaskManagerFactory
    {
        private TaskManagerFactory() { }

        /// <summary>
        /// Creates an ITaskManager instance.
        /// </summary>
        /// <param name="repositoryFactory">Delegate that creates an ITaskRepository instance.</param>
        /// <param name="options">(Optional)A task manager options object.</param>
        /// <returns>Returns an ITaskManager instance.</returns>
        public static ITaskManager Create(Func<ITaskRepository> repositoryFactory, TaskManagerOptions options = null)
        {
            var lockManager = LockManagerFactory.Create(repositoryFactory, minTimeout: new TimeSpan(TimeSpan.TicksPerSecond));
            return new TaskManager(repositoryFactory, lockManager, options);
        }
    }

    internal class TaskManager : ITaskManager
    {
        static readonly Type _genericTaskDefinition = typeof(IDistributedTask<>);

        Func<ITaskRepository> _repositoryFactory;
        readonly object _localLock = new object();
        Dictionary<Guid, TaskTypeInfo> _taskTypeInfos = new Dictionary<Guid, TaskTypeInfo>();
        Dictionary<Type, TaskTypeInfo> _taskTypes = new Dictionary<Type, TaskTypeInfo>();

        TaskManagerListener _listener;

        internal TaskManager(Func<ITaskRepository> repositoryFactory, ILockManager lockManager, TaskManagerOptions options = null)
        {
            options = options ?? TaskManagerOptions.Default;
            this.LockManager = lockManager;
            _repositoryFactory = repositoryFactory;
            _listener = new TaskManagerListener(this, options.MainLoopWait, options.MaxWorkerThreads);

            using (var repository = repositoryFactory())
            {
                _connectionString = repository.Database.Connection.ConnectionString;
            }
        }

        private string _connectionString;
        internal IDbConnection CreateConnection()
        {
            return DbAccessHelper.CreateDbConnection(_connectionString);
        }

        internal ILockManager LockManager { get; private set; }

        internal ITaskRepository CreateRepository()
        {
            return _repositoryFactory();
        }

        private Type GetGenericTaskType(Type type, out Type paramType)
        {
            paramType = null;
            if (type == null)
                return null;

            foreach (var interfaceType in type.GetInterfaces().Where(t => t.IsGenericType))
            {
                var genericDefinition = interfaceType.GetGenericTypeDefinition();
                if (genericDefinition == _genericTaskDefinition)
                {
                    paramType = interfaceType.GetGenericArguments().FirstOrDefault();
                    return type;
                }
            }

            return type;
        }

        private TaskTypeInfo CreateTaskInfo(Type type, TaskSettings settings)
        {
            Type paramType = null;
            var genericType = GetGenericTaskType(type, out paramType);
            if (genericType == null)
                throw new ApplicationException(String.Format("The type [{0}] is not a valid Task or Task<P> type.", type.FullName));

            TaskTypeInfo info = TaskTypeInfo.CreateFromType(type, paramType, settings);

            return info;
        }

        public void RegisterTaskType(Type type, TaskSettings settings)
        {
            lock (_localLock)
            {
                var info = CreateTaskInfo(type, settings);

                if (info.LockCycle < LockManager.MinTimeout || info.LockCycle > LockManager.MaxTimeout)
                    throw new ArgumentException("Invalid AliveCycle value.");

                TaskTypeInfo duplicate;
                if (_taskTypeInfos.TryGetValue(info.Identifier, out duplicate))
                {
                    throw new ApplicationException(String.Format("They identifier [{0}] is already linked to type [{1}].", duplicate.Identifier, duplicate.TaskType.FullName));
                }
                _taskTypeInfos.Add(info.Identifier, info);
                _taskTypes.Add(type, info);
            }
        }

        public TaskTypeInfo GetTaskInfo(Guid identifier)
        {
            TaskTypeInfo info;
            return _taskTypeInfos.TryGetValue(identifier, out info) ? info : null;
        }

        protected TaskTypeInfo ValidateRegistration(Type type)
        {
            lock (_localLock)
            {
                if (!_taskTypes.ContainsKey(type))
                    throw new ApplicationException(String.Format("The type [{0}] must be registered before enqueuing tasks.", type.FullName));

                return _taskTypes[type];
            }
        }

        public int EnqueueSingleTask<T>(long delay = 0)
        {
            if (delay < 0) throw new ArgumentException("delay");

            var info = ValidateRegistration(typeof(T));
            return EnqueueSingleTask(info.Identifier, null, delay);
        }

        public int EnqueueSingleTask(Guid identifier, long delay = 0)
        {
            return this.EnqueueSingleTask(identifier, null, delay);
        }

        internal int EnqueueSingleTask(Guid identifier, int? recurrentTaskId, long delay, bool throwOnNotFound = true)
        {
            var info = this.GetTaskInfo(identifier);
            if (info == null)
            {
                if (throwOnNotFound)
                {
                    throw new ApplicationException(String.Format("Task identifier {0} not registered.", identifier));
                }
                else
                {
                    return 0;
                }
            }

            using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            using (var context = CreateRepository())
            {
                var data = new SingleTaskDefinition()
                {
                    Identifier = info.Identifier,
                    JsonParameters = null,
                    ScheduledAt = DateTime.UtcNow.AddMilliseconds(delay),
                    RecurrentTaskId = recurrentTaskId
                };

                data.SetStatus(Distributed.SingleTaskStatus.Scheduled);

                context.SingleTasks.Add(data);
                context.SaveChanges();

                return data.Id;
            }
        }

        public int EnqueueSingleTask<T, P>(P parameter, long delay = 0) where T : IDistributedTask<P>
        {
            var info = ValidateRegistration(typeof(T));
            return EnqueueSingleTask<P>(info.Identifier, parameter, delay);
        }

        public int EnqueueSingleTask<P>(Guid identifier, P parameter, long delay = 0)
        {
            if (delay < 0) throw new ArgumentException("delay");

            var info = this.GetTaskInfo(identifier);
            if (info == null)
                throw new ArgumentException(String.Format("Task identifier [{0}] not registered.", identifier.ToString()));

            if (info.ParamType != typeof(P))
                throw new ArgumentException("Parameter type mismatch. Review registered Task and parameter types.");

            var paramStr = JsonConvert.SerializeObject(parameter);

            using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            using (var context = CreateRepository())
            {
                var data = new SingleTaskDefinition()
                {
                    Identifier = info.Identifier,
                    JsonParameters = paramStr,
                    ScheduledAt = DateTime.UtcNow.AddMilliseconds(delay),
                };

                data.SetStatus(Distributed.SingleTaskStatus.Scheduled);

                context.SingleTasks.Add(data);
                context.SaveChanges();

                return data.Id;
            }
        }

        private const string SQL_REMOVE_SINGLE = "Delete from SingleTasks where id = @id AND [status] = @status";

        public bool RemoveSingleTask(int id)
        {
            using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            using (var conn = CreateConnection())
            using (var cmd = DbAccessHelper.CreateDbCommand(conn, SQL_REMOVE_SINGLE))
            {
                cmd.Parameters.Add(new SqlParameter("id", id));
                cmd.Parameters.Add(new SqlParameter("status", (int)Distributed.SingleTaskStatus.Scheduled));

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        private static readonly Guid ENQUEUE_RECURRENT_TASK_LOCK = new Guid("73C87F42-37D1-4E8C-ABEC-E32046A3D3E9");

        public int EnqueueRecurrentTask<T>(long interval)
        {
            var info = ValidateRegistration(typeof(T));
            return EnqueueRecurrentTask(info.Identifier, interval);
        }

        public int EnqueueRecurrentTask(Guid identifier, long interval)
        {
            if (interval < 1000)
                throw new ArgumentException("interval");

            var info = this.GetTaskInfo(identifier);
            if (info == null)
                throw new ArgumentException(String.Format("Task identifier [{0}] not registered.", identifier.ToString()));

            LockInfo lockInfo = null;
            try
            {
                if (LockManager.WaitForLock(ENQUEUE_RECURRENT_TASK_LOCK, new TimeSpan(TimeSpan.TicksPerMinute), new TimeSpan(TimeSpan.TicksPerSecond * 10), out lockInfo))
                {
                    using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                    using (var context = CreateRepository())
                    {
                        //Adds or updates a recurrent task
                        var data = context.RecurrentTasks
                                          .Where(t => t.Identifier == info.Identifier)
                                          .OrderBy(t => t.Id).FirstOrDefault();
                        data = data ?? new RecurrentTaskDefinition()
                        {
                            Identifier = info.Identifier
                        };

                        data.Enabled = true;
                        data.Interval = interval;
                        if (data.Id == 0) context.RecurrentTasks.Add(data);

                        context.SaveChanges();

                        var copies = context.RecurrentTasks
                                            .Where(t => t.Identifier == info.Identifier)
                                            .OrderBy(t => t.Id).ToList();

                        if (copies.Count == 0)
                            throw new ApplicationException("Database task entry was modified while enqueuing task.");

                        //Recurrent tasks cannot have duplicates
                        foreach (var duplicate in copies.Skip(1))
                            context.RecurrentTasks.Remove(duplicate);

                        var first = copies.FirstOrDefault();
                        if (first.Interval != interval) first.Interval = interval;

                        context.SaveChanges();

                        return first.Id;
                    }
                }
                else
                {
                    throw new ApplicationException("Lock for resource ENQUEUE_RECURRENT_TASK_LOCK has been constantly denied.");
                }
            }
            finally
            {
                LockManager.Release(lockInfo);
            }
        }

        public void StartListener(int delay = 0) { _listener.StartListener(delay); }
        public void StopListener(bool waitTillStops = false)
        {
            _listener.StopListener();
            if (waitTillStops) this.WaitTillListenerStops();
        }
        public void WaitTillListenerStops() { _listener.JoinListener(); }
        public void SubscribeToListener(Action action) { _listener.SubscribeToListener(action); }

        public bool IsListenning { get { return _listener.IsListenning(); } }
        public Exception LatestListenerException { get { return _listener.LatestListenerException; } }

        public int TasksExecutedCount { get { return _listener.TasksExecutedCount; } }
    }

    internal class TaskTypeInfo
    {
        public Guid Identifier { get; private set; }
        public Type TaskType { get; private set; }
        public Type ParamType { get; private set; }
        public TimeSpan LockCycle { get; private set; }

        private TaskTypeInfo() { }

        public static TaskTypeInfo CreateFromType(Type type, Type paramType, TaskSettings settings)
        {
            if (settings.Identifier == Guid.Empty)
                throw new ArgumentException("GUID cannot be empty.");

            return new TaskTypeInfo()
            {
                Identifier = settings.Identifier,
                TaskType = type,
                ParamType = paramType,
                LockCycle = settings.AliveCycle.Ticks < (TimeSpan.MaxValue.Ticks / 2) ? new TimeSpan(settings.AliveCycle.Ticks * 2) : settings.AliveCycle
            };
        }

        internal void Invoke(string jsonParameter, TaskExecutionInfo executionInfo)
        {
            object parameter = null;
            if (jsonParameter != null)
                parameter = JsonConvert.DeserializeObject(jsonParameter, this.ParamType);

            var instance = Activator.CreateInstance(this.TaskType);

            TaskTypeInfo.TypedInvoke((dynamic)instance, (dynamic)parameter, executionInfo);
        }

        internal static void TypedInvoke<P>(IDistributedTask<P> instance, P parameter, TaskExecutionInfo executionInfo)
        {
            instance.Run(parameter, executionInfo);
        }
    }

    /// <summary>
    /// Represents a generic distributed task definition with a paramater
    /// </summary>
    /// <typeparam name="P">A parameter type. It must have a parameterless constructor.</typeparam>
    public interface IDistributedTask<P>
    {
        /// <summary>
        /// Body of the task. This method is called when the manager processes the task.
        /// </summary>
        /// <param name="parameter">A parameter value.</param>
        /// <param name="executionInfo">Tracks information about the task health.<br/>
        /// ITaskExecutionInfo.SendAliveSignal() MUST be called periodically to ensure the task is kept alive.<br/>
        /// ITaskExecutionInfo.IsCancellationRequested MUST be read periodically.<br/>
        /// If cancellation has been requested, the task MUST end as soon as possible.<br/>
        /// </param>               
        void Run(P parameter, ITaskExecutionInfo executionInfo);
    }

    /// <summary>
    /// Task Settings class
    /// </summary>
    public sealed class TaskSettings
    {
        private static readonly TimeSpan OneMinute = new TimeSpan(TimeSpan.TicksPerMinute);

        /// <summary>
        /// TaskSettings constructor
        /// </summary>
        /// <param name="identifier">The GUID linked to this task implementation. It MUST be unique per task type.</param>
        public TaskSettings(Guid identifier)
        {
            if (identifier == Guid.Empty)
                throw new ArgumentException("GUID cannot be empty.");

            this.AliveCycle = OneMinute;
            this.Identifier = identifier;
        }

        /// <summary>
        /// TaskSettings constructor
        /// </summary>
        /// <param name="identifier">The GUID linked to this task implementation. It MUST be unique per task type.</param>
        public TaskSettings(string identifier) : this(new Guid(identifier)) { }

        /// <summary>
        /// Specified the alive cycle period, i.e, the maximum period that task can stay alive without notifying the manager via ITaskExecutionInfo.SendAliveSignal().<br/>
        /// It the task doesn't notifying the manager within this period, it will receive a cancellation request via ITaskExecutionInfo.IsCancellationRequested.<br/>
        /// After receiving the cancellation request the task has another [AliveCycle] period to finish or a ThreadAbortException will be thrown.
        /// Defaults to 60 seconds.
        /// </summary>
        public TimeSpan AliveCycle { get; set; }

        /// <summary>
        /// The GUID linked to this task implementation. It MUST be unique per task type.
        /// </summary>
        public Guid Identifier { get; set; }
    }
}
