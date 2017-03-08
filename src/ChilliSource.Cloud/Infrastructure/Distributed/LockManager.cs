using ChilliSource.Cloud.Data;
using ChilliSource.Cloud.Data.Distributed;
using ChilliSource.Cloud.Extensions;
using ChilliSource.Cloud.Logging.Extensions;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ChilliSource.Cloud.Infrastructure.Distributed
{
    /// <summary>
    /// Represents a distributed (cross-machine or process) lock manager.
    /// </summary>
    public interface ILockManager : ILockManagerAsync
    {
        /// <summary>
        /// Returns the minimum valid lock timeout for this manager.
        /// </summary>
        TimeSpan MinTimeout { get; }
        /// <summary>
        /// Returns the maximum valid lock timeout for this manager.
        /// </summary>
        TimeSpan MaxTimeout { get; }

        /// <summary>
        /// Waits for a maximum specified period or until a lock is acquired.
        /// </summary>
        /// <param name="resource">Resource GUID that needs to be locked</param>
        /// <param name="lockTimeout">(Optional) Lock timeout. Defaults to one minute.</param>
        /// <param name="waitTime">Maximum time to wait for the lock to be acquired. must be in the timeout range [MinTimeout, MaxTimeout]</param>
        /// <param name="lockInfo">Returns a lock object containing information about the lock acquisition or failure.</param>
        /// <returns>Returns whether the lock was acquired.</returns>
        bool WaitForLock(Guid resource, TimeSpan? lockTimeout, TimeSpan waitTime, out LockInfo lockInfo);

        /// <summary>
        /// Tries to acquire a lock and immediately returns the result.
        /// </summary>
        /// <param name="resource">Resource GUID that needs to be locked</param>
        /// <param name="lockTimeout">(Optional) Lock timeout. Defaults to one minute.</param>
        /// <param name="lockInfo">Returns a lock object containing information about the lock acquisition or failure.</param>
        /// <returns>Returns whether the lock was acquired.</returns>
        bool TryLock(Guid resource, TimeSpan? lockTimeout, out LockInfo lockInfo);

        /// <summary>
        /// Tries to renew an existing lock object.
        /// </summary>
        /// <param name="lockInfo">A lock object that has been previously acquired.</param>
        /// <param name="renewalTimeout">(Optional) Renewal timeout. Defaults to current lock timeout value.</param>
        /// <param name="retryLock">Specifies whether the lock acquisition should be attempted even when the lock is expired.</param>
        /// <returns>Returns whether the lock was acquired.</returns>
        bool TryRenewLock(LockInfo lockInfo, TimeSpan? renewalTimeout = null, bool retryLock = false);

        /// <summary>
        /// Releases a lock if it is still valid.
        /// </summary>
        /// <param name="lockInfo">A lock object that has been previously acquired.</param>
        /// <returns>Returns whether the lock was released.</returns>
        bool Release(LockInfo lockInfo);

        /// <summary>
        /// Releases a lock if it is still valid.
        /// </summary>
        /// <param name="lockInfo">A lock object that has been previously acquired.</param>
        /// <returns>(Async) Returns whether the lock was released.</returns>
        Task<bool> ReleaseAsync(LockInfo lockInfo);
    }

    /// <summary>
    /// (Async) Represents a distributed (cross-machine or process) lock manager.
    /// </summary>
    public interface ILockManagerAsync
    {
        /// <summary>
        /// Waits for a maximum specified period or until a lock is acquired.
        /// </summary>
        /// <param name="resource">Resource GUID that needs to be locked</param>
        /// <param name="lockTimeout">(Optional) Lock timeout. Defaults to one minute.</param>
        /// <param name="waitTime">Maximum time to wait for the lock to be acquired. must be in the timeout range [MinTimeout, MaxTimeout]</param>        
        /// <returns>(Async) Returns a lock object containing information about the lock acquisition or failure.</returns>
        Task<LockInfo> WaitForLockAsync(Guid resource, TimeSpan? lockTimeout, TimeSpan waitTime);

        /// <summary>
        /// Tries to acquire a lock and immediately returns the result.
        /// </summary>
        /// <param name="resource">Resource GUID that needs to be locked</param>
        /// <param name="lockTimeout">(Optional) Lock timeout. Defaults to one minute.</param>
        /// <returns>(Async) Returns a lock object containing information about the lock acquisition or failure.</returns>
        Task<LockInfo> TryLockAsync(Guid resource, TimeSpan? lockTimeout);

        /// <summary>
        /// Tries to renew an existing lock object.
        /// </summary>
        /// <param name="lockInfo">A lock object that has been previously acquired.</param>
        /// <param name="renewalTimeout">(Optional) Renewal timeout. Defaults to current lock timeout value.</param>
        /// <param name="retryLock">Specifies whether the lock acquisition should be attempted even when the lock is expired.</param>
        /// <returns>(Async) Returns whether the lock was acquired.</returns>
        Task<bool> TryRenewLockAsync(LockInfo lockInfo, TimeSpan? renewalTimeout = null, bool retryLock = false);

        /// <summary>
        /// Releases a lock if it is still valid.
        /// </summary>
        /// <param name="lockInfo">A lock object that has been previously acquired.</param>
        /// <returns>(Async) Returns whether the lock was released.</returns>
        Task<bool> ReleaseAsync(LockInfo lockInfo);
    }

    /// <summary>
    /// Provides a way to create ILockManager instances.
    /// </summary>
    public class LockManagerFactory
    {
        private LockManagerFactory() { }

        /// <summary>
        /// Creates an ILockManager instance.
        /// </summary>
        /// <param name="repositoryFactory">Delegate that creates an IDistributedLockRepository instance.</param>
        /// <param name="minTimeout">The minimum valid lock timeout for this manager.</param>
        /// <param name="maxTimeout">The maximum valid lock timeout for this manager.</param>
        /// <returns>Returns an ILockManager instance.</returns>
        public static ILockManager Create(Func<IDistributedLockRepository> repositoryFactory, TimeSpan? minTimeout = null, TimeSpan? maxTimeout = null)
        {
            return new LockManager(repositoryFactory, minTimeout, maxTimeout);
        }
    }

    internal class LockManager : ILockManager
    {
        private string _machineName;
        private int _PID;
        Func<IDistributedLockRepository> _repositoryFactory;
        TimeSpan _minTimeout;
        TimeSpan _maxTimeout;
        private const long DEFAULT_MIN_TIMEOUT_TICKS = TimeSpan.TicksPerSecond;
        private const long DEFAULT_MAX_TIMEOUT_TICKS = TimeSpan.TicksPerMinute * 5;
        private readonly TimeSpan defaultTimeout = new TimeSpan(TimeSpan.TicksPerMinute);

        internal LockManager(Func<IDistributedLockRepository> repositoryFactory, TimeSpan? minTimeout = null, TimeSpan? maxTimeout = null)
        {
            _minTimeout = minTimeout ?? new TimeSpan(DEFAULT_MIN_TIMEOUT_TICKS);
            _maxTimeout = maxTimeout ?? new TimeSpan(DEFAULT_MAX_TIMEOUT_TICKS);
            if (_minTimeout == TimeSpan.Zero || _maxTimeout < _minTimeout || _maxTimeout < defaultTimeout)
                throw new ArgumentException("invalid minTimeout/maxTimeout pair.");

            _machineName = Environment.MachineName.Truncate(100);
            _PID = Process.GetCurrentProcess().Id;
            _repositoryFactory = repositoryFactory;

            using (var repository = repositoryFactory())
            {
                _connectionString = repository.Database.Connection.ConnectionString;
            }
        }

        private string _connectionString;
        private IDbConnectionAsync CreateConnection()
        {
            return DbAccessHelperAsync.CreateDbConnection(_connectionString);
        }

        private const string SQL_INSERT = "BEGIN"
                                          + " IF NOT EXISTS (SELECT Resource from dbo.DistributedLocks where Resource = @resource)"
                                          + "     BEGIN Insert into dbo.DistributedLocks(Resource, LockReference, Timeout)"
                                          + "           values (@resource, 0, 0)"
                                          + "     END"
                                          + " END";

        private const string SELECT_REFERENCE = "SELECT TOP (1) [LockReference] FROM [dbo].[DistributedLocks]"
                                               + " WHERE ([Resource] = @resource) AND (([LockedUntil] IS NULL) OR ([LockedUntil] < SYSUTCDATETIME()))";

        private const string SELECT_LOCKEDUNTIL = "SELECT TOP (1) [LockedUntil] FROM [dbo].[DistributedLocks]"
                                               + " where Resource = @resource and LockReference = @newLockRef";

        private const string SQL_LOCK = "UPDATE dbo.DistributedLocks Set LockReference = @newLockRef, Timeout = @timeout, LockedAt = SYSUTCDATETIME(), LockedUntil = DATEADD(ms, @timeout, SYSUTCDATETIME()), LockedByMachine = @lockedByMachine, LockedByPID = @lockedByPID"
                                      + " where Resource = @resource and LockReference = @lockReference and (LockedUntil is NULL or LockedUntil < SYSUTCDATETIME())";

        private const string RENEW_LOCK = "UPDATE dbo.DistributedLocks Set LockReference = @newLockRef, Timeout = @timeout, LockedAt = SYSUTCDATETIME(), LockedUntil = DATEADD(ms, @timeout, SYSUTCDATETIME())"
                                        + " where Resource = @resource and LockReference = @lockReference and LockedByPID = @lockedByPID and LockedUntil > SYSUTCDATETIME() and Timeout > 0";

        private const string RELEASE_LOCK = "UPDATE dbo.DistributedLocks Set LockReference = @newLockRef, LockedAt = NULL, LockedUntil = NULL"
                                         + " where Resource = @resource and LockReference = @lockReference and LockedByPID = @lockedByPID and LockedUntil > SYSUTCDATETIME()";

        public TimeSpan MinTimeout { get { return _minTimeout; } }
        public TimeSpan MaxTimeout { get { return _maxTimeout; } }

        private async Task<bool> insertEntryRecordAsync(Guid resource)
        {
            using (var conn = CreateConnection())
            using (var command = await DbAccessHelperAsync.CreateDbCommand(conn, SQL_INSERT))
            {
                command.Parameters.Add(new SqlParameter("resource", resource));
                try
                {
                    return (await command.ExecuteNonQueryAsync() > 0);
                }
                catch (Exception ex)
                {
                    ex.LogException();
                    return false;
                }
            }
        }

        private async Task<LockInfo> acquireLockAsync(Guid resource, int lockReference, TimeSpan timeout)
        {
            var lockInfo = LockInfo.Empty(resource);

            var newLockRef = Math.Max(1, lockReference + 1);

            //only acquires lock if old lock reference number matches, and not locked yet.
            using (var conn = CreateConnection())
            using (var command = await DbAccessHelperAsync.CreateDbCommand(conn, SQL_LOCK))
            {
                command.Parameters.Add(new SqlParameter("newLockRef", newLockRef));
                command.Parameters.Add(new SqlParameter("timeout", Convert.ToInt64(timeout.TotalMilliseconds)));
                command.Parameters.Add(new SqlParameter("lockedByMachine", _machineName));
                command.Parameters.Add(new SqlParameter("lockedByPID", _PID));
                command.Parameters.Add(new SqlParameter("resource", resource));
                command.Parameters.Add(new SqlParameter("lockReference", lockReference));

                try
                {
                    if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        command.Parameters.Clear();
                        command.CommandText = SELECT_LOCKEDUNTIL;
                        command.Parameters.Add(new SqlParameter("newLockRef", newLockRef));
                        command.Parameters.Add(new SqlParameter("resource", resource));
                        var lockedTill = (DateTime?)await command.ExecuteScalarAsync();

                        lockInfo.Update(lockedTill, timeout, newLockRef);
                    }
                }
                catch (Exception ex)
                {
                    ex.LogException();
                }

                return lockInfo;
            }
        }

        public bool WaitForLock(Guid resource, TimeSpan? lockTimeout, TimeSpan waitTime, out LockInfo lockInfo)
        {
            lockInfo = TaskHelper.GetResultSafeSync(() => this.WaitForLockAsync(resource, lockTimeout, waitTime));
            return lockInfo.HasLock;
        }

        public async Task<LockInfo> WaitForLockAsync(Guid resource, TimeSpan? lockTimeout, TimeSpan waitTime)
        {
            if (waitTime > _maxTimeout)
            {
                throw new ArgumentException(String.Format("[maxWaitTime] cannot be greater than {0} ticks.", _maxTimeout));
            }

            LockInfo acquiredLock = null;
            DateTime beginTime = DateTime.UtcNow;
            DateTime waitUntil = beginTime.Add(waitTime);
            while (true)
            {
                acquiredLock = await TryLockAsync(resource, lockTimeout);

                var now = DateTime.UtcNow;
                if (acquiredLock.HasLock || waitUntil < now || beginTime > now)
                    break;

                await Task.Delay(1000);
            }

            return acquiredLock;
        }

        private void verifyTimeoutLimits(TimeSpan lockTimeout)
        {
            if (lockTimeout < _minTimeout)
            {
                throw new ApplicationException(String.Format("Timeout must be at least {0} ticks.", _minTimeout.Ticks));
            }
            if (lockTimeout > _maxTimeout)
            {
                throw new ApplicationException(String.Format("Timeout must be less than {0} ticks.", _maxTimeout.Ticks));
            }
        }

        public bool TryLock(Guid resource, TimeSpan? lockTimeout, out LockInfo lockInfo)
        {
            lockInfo = TaskHelper.GetResultSafeSync(() => TryLockAsync(resource, lockTimeout));
            return lockInfo.HasLock;
        }

        public async Task<LockInfo> TryLockAsync(Guid resource, TimeSpan? lockTimeout)
        {
            var now = DateTime.UtcNow;
            lockTimeout = lockTimeout ?? defaultTimeout;
            verifyTimeoutLimits(lockTimeout.Value);

            using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                var selectRef = await GetLatestReferenceAsync(resource);

                //failed to insert record or resource already locked
                if (selectRef == null)
                {
                    // Ensures an entry for the resource is persisted.
                    if (await insertEntryRecordAsync(resource))
                    {
                        selectRef = 0;
                    }
                    else
                    {
                        return LockInfo.Empty(resource);
                    }
                }

                //tries to acquire lock
                return await acquireLockAsync(resource, selectRef.Value, lockTimeout.Value);
            }
        }

        private async Task<int?> GetLatestReferenceAsync(Guid resource)
        {
            using (var conn = CreateConnection())
            using (var command = await DbAccessHelperAsync.CreateDbCommand(conn, SELECT_REFERENCE))
            {
                command.Parameters.Add(new SqlParameter("resource", resource));
                var result = await command.ExecuteScalarAsync();

                return (int?)result;
            }
        }

        private async Task<LockInfo> renewLockAsync(TimeSpan renewTimeout, LockInfo lockInfo)
        {
            var emptyLock = LockInfo.Empty(lockInfo.Resource);

            if (!lockInfo.HasLock)
                return emptyLock;

            var state = lockInfo.GetInternalState();
            var newLockRef = Math.Max(1, state.LockReference + 1);

            using (var conn = CreateConnection())
            using (var command = await DbAccessHelperAsync.CreateDbCommand(conn, RENEW_LOCK))
            {
                command.Parameters.Add(new SqlParameter("newLockRef", newLockRef));
                command.Parameters.Add(new SqlParameter("resource", state.Resource));
                command.Parameters.Add(new SqlParameter("lockReference", state.LockReference));
                command.Parameters.Add(new SqlParameter("timeout", Convert.ToInt64(renewTimeout.TotalMilliseconds)));
                command.Parameters.Add(new SqlParameter("lockedByPID", _PID));

                try
                {
                    if (await command.ExecuteNonQueryAsync() == 0)
                        return emptyLock;

                    command.Parameters.Clear();
                    command.CommandText = SELECT_LOCKEDUNTIL;
                    command.Parameters.Add(new SqlParameter("newLockRef", newLockRef));
                    command.Parameters.Add(new SqlParameter("resource", state.Resource));
                    var lockedTill = (DateTime?)await command.ExecuteScalarAsync();

                    lockInfo.Update(lockedTill, renewTimeout, newLockRef);

                    return lockInfo;
                }
                catch (Exception ex)
                {
                    return emptyLock;
                }
            }
        }

        private async Task<bool> releaseAsync(LockInfo lockInfo)
        {
            if (!lockInfo.HasLock)
                return true; //released already

            var state = lockInfo.GetInternalState();
            var newLockRef = Math.Max(1, state.LockReference + 1);

            using (var conn = CreateConnection())
            using (var command = await DbAccessHelperAsync.CreateDbCommand(conn, RELEASE_LOCK))
            {
                command.Parameters.Add(new SqlParameter("newLockRef", newLockRef));
                command.Parameters.Add(new SqlParameter("resource", state.Resource));
                command.Parameters.Add(new SqlParameter("lockReference", state.LockReference));
                command.Parameters.Add(new SqlParameter("lockedByPID", _PID));

                try
                {
                    await command.ExecuteNonQueryAsync();
                    lockInfo.Update(null, TimeSpan.Zero, 0);

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool TryRenewLock(LockInfo lockInfo, TimeSpan? renewTimeout = null, bool retryLock = false)
        {
            return TaskHelper.GetResultSafeSync(() => TryRenewLockAsync(lockInfo, renewTimeout, retryLock));
        }

        public async Task<bool> TryRenewLockAsync(LockInfo lockInfo, TimeSpan? renewTimeout = null, bool retryLock = false)
        {
            if (lockInfo == null)
                throw new ArgumentNullException("LockInfo is null");

            //Allows only one task to run TryRenewLock on this lockInfo object
            using (await lockInfo.Mutex.LockAsync())
            {
                renewTimeout = renewTimeout ?? lockInfo.Timeout;
                verifyTimeoutLimits(renewTimeout.Value);

                LockInfo lockRenewed = null;
                using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                {
                    lockRenewed = await renewLockAsync(renewTimeout.Value, lockInfo);
                }

                var isRenewed = lockRenewed.HasLock;
                if (!isRenewed && retryLock)
                {
                    LockInfo newLock = await this.TryLockAsync(lockInfo.Resource, renewTimeout.Value);
                    isRenewed = newLock.HasLock;
                    if (isRenewed)
                    {
                        lockInfo.Update(newLock.LockedUntil, newLock.Timeout, newLock.LockReference);
                    }
                }

                return isRenewed;
            }
        }

        public bool Release(LockInfo lockInfo)
        {
            if (lockInfo == null)
                return false;

            return TaskHelper.GetResultSafeSync(() => this.ReleaseAsync(lockInfo));
        }

        public async Task<bool> ReleaseAsync(LockInfo lockInfo)
        {
            if (lockInfo == null)
                return false;

            using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                return await releaseAsync(lockInfo);
            }
        }
    }

    /// <summary>
    /// Contains information about the lock acquisition attempt.
    /// </summary>
    public sealed class LockInfo
    {
        internal readonly AsyncLock Mutex = new AsyncLock();

        internal static LockInfo Empty(Guid resource)
        {
            return new LockInfo(resource, null, TimeSpan.Zero, 0);
        }

        State _state;

        internal State GetInternalState()
        {
            return _state;
        }

        internal LockInfo(Guid resource, DateTime? lockedUntil, TimeSpan timeout, int lockReference)
        {
            this.Set(resource, lockedUntil, timeout, lockReference);
        }

        internal void Update(DateTime? lockedUntil, TimeSpan timeout, int lockReference)
        {
            this.Set(_state.Resource, lockedUntil, timeout, lockReference);
        }

        //Ensures that all properties all set AT ONCE, so we don't have concurrency issues.
        private void Set(Guid resource, DateTime? lockedUntil, TimeSpan timeout, int lockReference)
        {
            var halfTimeout = new TimeSpan(timeout.Ticks / 2);
            var halfTime = (lockedUntil == null) ? (DateTime?)null : lockedUntil.Value.Subtract(halfTimeout);

            _state = new State()
            {
                Resource = resource,
                LockedUntil = lockedUntil,
                LockReference = lockReference,
                Timeout = timeout,
                HalfTimeout = halfTimeout,
                HalfTime = halfTime
            };
        }

        internal TimeSpan HalfTimeout { get { return _state.HalfTimeout; } }
        internal bool IsLockHalfTimePassed() { return _state.IsLockHalfTimePassed(); }
        internal int LockReference { get { return _state.LockReference; } }

        /// <summary>
        /// Resource GUID.
        /// </summary>
        public Guid Resource { get { return _state.Resource; } }

        /// <summary>
        /// (When locked) Upper Date/Time limit of the lock.
        /// </summary>
        public DateTime? LockedUntil { get { return _state.LockedUntil; } }

        /// <summary>
        /// Lock timeout in milliseconds
        /// </summary>
        public TimeSpan Timeout { get { return _state.Timeout; } }

        /// <summary>
        /// Returns whether the lock is valid at this instant.
        /// </summary>
        public bool HasLock { get { return _state.HasLock; } }

        /// <summary>
        /// (When expired) Returns the time period since the lock expired.
        /// </summary>
        public TimeSpan? PeriodSinceLockTimeout { get { return _state.PeriodSinceLockTimeout; } }

        //Ensures that all properties all set AT ONCE, so we don't have concurrency issues.
        internal struct State
        {
            internal Guid Resource;
            internal DateTime? LockedUntil;
            internal int LockReference;
            internal TimeSpan Timeout;
            internal TimeSpan HalfTimeout;
            internal DateTime? HalfTime;

            internal bool IsLockHalfTimePassed()
            {
                return HalfTime != null && DateTime.UtcNow > HalfTime;
            }

            public bool HasLock
            {
                get { return LockedUntil != null && LockedUntil > DateTime.UtcNow; }
            }

            public TimeSpan? PeriodSinceLockTimeout
            {
                get
                {
                    var now = DateTime.UtcNow;

                    if (this.LockedUntil == null || now < this.LockedUntil)
                        return null;

                    return now.Subtract(this.LockedUntil.Value);
                }
            }
        }
    }
}
