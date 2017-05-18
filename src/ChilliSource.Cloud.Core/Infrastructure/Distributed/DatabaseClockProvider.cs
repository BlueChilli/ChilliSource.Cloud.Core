using ChilliSource.Cloud.Core.Distributed;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ChilliSource.Core.Extensions;
using Humanizer;
using System.Data;

namespace ChilliSource.Cloud.Core
{
    internal class DatabaseClockProvider : IClockProvider
    {
        private const string SQL_SELECT_UTCTIME = "SELECT SYSUTCDATETIME()";

        private string _connectionString;
        private Func<IDistributedLockRepository> _repositoryFactory;
        private readonly AsyncLock _mutex = new AsyncLock();
        private IClockProvider _staticClockProvider;
        private IClock _clock = null;

        private DatabaseClockProvider() { }

        public static DatabaseClockProvider Create(Func<IDistributedLockRepository> repositoryFactory, IClockProvider staticClockProvider)
        {
            var manager = new DatabaseClockProvider();
            manager.Init(repositoryFactory, staticClockProvider);
            return manager;
        }

        private void Init(Func<IDistributedLockRepository> repositoryFactory, IClockProvider staticClockProvider)
        {
            _repositoryFactory = repositoryFactory;
            _staticClockProvider = staticClockProvider;

            using (var repository = repositoryFactory())
            {
                _connectionString = repository.Database.Connection.ConnectionString;
            }
        }

        public async Task<IClock> GetClockAsync()
        {
            var clock = this._clock;
            if (clock != null)
            {
                return clock;
            }

            using (await _mutex.LockAsync())
            {
                clock = this._clock;
                if (clock != null)
                {
                    return clock;
                }

                this._clock = clock = await GetUpdatedClockAsync();
                return clock;
            }
        }

        private async Task<IClock> GetUpdatedClockAsync()
        {
            try
            {
                var staticClock = await _staticClockProvider.GetClockAsync();
                var discrepancyMin = await this.GetTimeDiscrepancyMin(4, staticClock);

                return new RelativeClock(staticClock, discrepancyMin);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not obtain database time information", ex);
            }
        }

        private async Task<TimeSpan> GetTimeDiscrepancyMin(int attempts, IClock clock)
        {
            if (attempts < 1)
                throw new ArgumentException("Number of attempts must be at least 1");

            using (var conn = DbAccessHelperAsync.CreateDbConnection(_connectionString))
            using (var command = await DbAccessHelperAsync.CreateDbCommand(conn, SQL_SELECT_UTCTIME))
            {
                var values = new List<TimeSpan>();
                for (int i = 0; i < attempts; i++)
                {
                    values.Add(await TimeDiscrepancy(command, clock));
                }

                return values.Min(t => t);
            }
        }

        private async Task<TimeSpan> TimeDiscrepancy(IDbCommandAsync command, IClock clock)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var dbDate = (DateTime)await command.ExecuteScalarAsync();
            watch.Stop();
            var now = clock.UtcNow;
            var additionalTicks = watch.Elapsed.Ticks / 2;

            var timeDiscrepancy = new TimeSpan((dbDate.Ticks + additionalTicks) - now.Ticks);

            return timeDiscrepancy;
        }
    }
}
