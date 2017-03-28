using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Adapters
{
    internal class DbConnectionAsyncAdapter : IDbConnectionAsync
    {
        DbConnection _connection;
        public DbConnectionAsyncAdapter(DbConnection connection)
        {
            _connection = connection;
        }

        public ConnectionState State
        {
            get
            {
                return _connection.State;
            }
        }

        public string ConnectionString
        {
            get
            {
                return _connection.ConnectionString;
            }
        }

        public int ConnectionTimeout
        {
            get
            {
                return _connection.ConnectionTimeout;
            }
        }

        public string Database
        {
            get
            {
                return _connection.Database;
            }
        }

        public IDbCommandAsync CreateCommand()
        {
            return new DbCommandAsyncAdapter(_connection.CreateCommand(), this);
        }

        public Task OpenAsync()
        {
            return _connection.OpenAsync();
        }

        public Task OpenAsync(CancellationToken cancellationToken)
        {
            return _connection.OpenAsync(cancellationToken);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            if (this._connection.State == ConnectionState.Open)
                this._connection.Close();
            this._connection.Dispose();
        }

        public IDbConnection AsSync()
        {
            return this._connection;
        }
    }
}
