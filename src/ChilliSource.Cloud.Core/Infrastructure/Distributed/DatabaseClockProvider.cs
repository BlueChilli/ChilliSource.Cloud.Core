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
using System.Threading;

namespace ChilliSource.Cloud.Core
{
    internal class DatabaseClockProvider : IClockProvider
    {
        private const string SQL_SELECT_UTCTIME = "SELECT SYSUTCDATETIME()";
        private const int REFRESH_INTERVAL = 60000;

        private string _connectionString;
        private Func<IDistributedLockRepository> _repositoryFactory;
        private IClock _clock = null;
        private CancellationTokenSource _ctSource;
        bool _isDisposed;

        private DatabaseClockProvider() { }

        public static DatabaseClockProvider Create(Func<IDistributedLockRepository> repositoryFactory)
        {
            var manager = new DatabaseClockProvider();
            manager.Init(repositoryFactory);
            return manager;
        }

        private void Init(Func<IDistributedLockRepository> repositoryFactory)
        {
            _ctSource = new CancellationTokenSource();
            _repositoryFactory = repositoryFactory;

            using (var repository = repositoryFactory())
            {
                _connectionString = repository.Database.Connection.ConnectionString;
            }

            //Waits first execution
            TaskHelper.WaitSafeSync(() => this.StartRefreshTask(0, REFRESH_INTERVAL));

            this.GetClock();
        }

        private Task StartRefreshTask(int delay, int interval)
        {
            if (_ctSource.IsCancellationRequested)
                return Task.CompletedTask;

            return Task.Run(async () =>
            {
                try
                {
                    try { await Task.Delay(delay, _ctSource.Token); } catch (TaskCanceledException) { }

                    if (_ctSource.IsCancellationRequested)
                        return;

                    _clock = await GetUpdatedClockAsync();
                }
                catch (Exception ex)
                {
                    ex.LogException();
                }
            })
            .ContinueWith(t =>
            {
                StartRefreshTask(interval, interval);
            });
        }

        public IClock GetClock()
        {
            var clock = this._clock;
            if (clock != null)
            {
                return clock;
            }
            else
            {
                throw new ApplicationException("Database clock not initialized yet.");
            }
        }

        private async Task<IClock> GetUpdatedClockAsync()
        {
            try
            {
                return await GetClockWithMinLatency(attempts: 3);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not obtain database time information", ex);
            }
        }

        private async Task<DatabaseClock> GetClockWithMinLatency(int attempts)
        {
            if (attempts < 1)
                throw new ArgumentException("Number of attempts must be at least 1");

            using (var tr = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            using (var conn = DbAccessHelperAsync.CreateDbConnection(_connectionString))
            using (var command = await DbAccessHelperAsync.CreateDbCommand(conn, SQL_SELECT_UTCTIME))
            {
                var clocks = new List<DatabaseClock>();
                for (int i = 0; i < attempts; i++)
                {
                    clocks.Add(await DatabaseClock.CreateAsync(async () => (DateTime)await command.ExecuteScalarAsync()));
                }

                var minLatency = clocks.Select(c => c.Latency).Min();
                return clocks.Where(clock => clock.Latency == minLatency).First();
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _ctSource.Cancel();
        }
    }
}
