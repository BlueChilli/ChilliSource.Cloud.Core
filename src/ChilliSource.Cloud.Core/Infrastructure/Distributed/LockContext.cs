using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core.Distributed
{
    /// <summary>
    /// Encapsulates a lock context when running a action.
    /// </summary>
    public interface ILockContext
    {
        /// <summary>
        /// Attempts to renew the internal resource lock.
        /// </summary>
        /// <param name="throwNoLock">Specifies whether an exception should be thrown when the lock renewal fails. Defaults to true.</param>
        /// <returns>Returns wether the lock was acquired or throws an exception. (see throwNoLock parameter)</returns>
        bool Renew(bool throwNoLock = true);
    }

    /// <summary>
    /// Encapsulates a lock context when running a task.
    /// </summary>
    public interface ILockContextAsync
    {
        /// <summary>
        /// Attempts to renew the internal resource lock.
        /// </summary>
        /// <param name="throwNoLock">Specifies whether an exception should be thrown when the lock renewal fails. Defaults to true.</param>
        /// <returns>Returns wether the lock was acquired or throws an exception. (see throwNoLock parameter)</returns>
        Task<bool> RenewAsync(bool throwNoLock = true);

        /// <summary>
        /// Returns a synchronous version of this lock context
        /// </summary>
        /// <returns></returns>
        ILockContext AsSync();
    }

    internal class LockContext : ILockContext, ILockContextAsync
    {
        private LockInfo _lockInfo;
        private ILockManager _lockmanager;

        public LockContext(ILockManager lockmanager, LockInfo lockInfo)
        {
            this._lockmanager = lockmanager;
            this._lockInfo = lockInfo;
        }

        public ILockContext AsSync()
        {
            return this;
        }

        public bool Renew(bool throwNoLock = true)
        {
            var renewed = this._lockmanager.TryRenewLock(this._lockInfo);

            if (!renewed && throwNoLock)
                throw new ApplicationException($"Couldn't renew lock on resource [{this._lockInfo.Resource}].");

            return renewed;
        }

        public async Task<bool> RenewAsync(bool throwNoLock = true)
        {
            var renewed = await this._lockmanager.TryRenewLockAsync(this._lockInfo);
            if (!renewed && throwNoLock)
                throw new ApplicationException($"Couldn't renew lock on resource [{this._lockInfo.Resource}].");

            return renewed;
        }
    }
}
