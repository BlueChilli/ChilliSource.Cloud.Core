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
    internal class DbCommandAsyncAdapter : IDbCommandAsync
    {
        private DbCommand _innerDbCommand;
        private DbConnectionAsyncAdapter _connection;

        public DbCommandAsyncAdapter(DbCommand dbCommand, DbConnectionAsyncAdapter connection)
        {
            _innerDbCommand = dbCommand;
            _connection = connection;
        }

        public IDataParameterCollection Parameters { get { return _innerDbCommand.Parameters; } }

        public string CommandText
        {
            get
            {
                return _innerDbCommand.CommandText;
            }
            set
            {
                _innerDbCommand.CommandText = value;
            }
        }

        public CommandType CommandType
        {
            get
            {
                return _innerDbCommand.CommandType;
            }
            set
            {
                _innerDbCommand.CommandType = value;
            }
        }

        public IDbConnectionAsync Connection
        {
            get
            {
                return _connection;
            }
        }

        public int CommandTimeout
        {
            get
            {
                return _innerDbCommand.CommandTimeout;
            }

            set
            {
                _innerDbCommand.CommandTimeout = value;
            }
        }

        public UpdateRowSource UpdatedRowSource
        {
            get
            {
                return _innerDbCommand.UpdatedRowSource;
            }

            set
            {
                _innerDbCommand.UpdatedRowSource = value;
            }
        }

        public Task<int> ExecuteNonQueryAsync()
        {
            return _innerDbCommand.ExecuteNonQueryAsync();
        }

        public Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            return _innerDbCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<IDataReader> ExecuteReaderAsync()
        {
            return await _innerDbCommand.ExecuteReaderAsync();
        }

        public async Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
        {
            return await _innerDbCommand.ExecuteReaderAsync(cancellationToken);
        }

        public async Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior)
        {
            return await _innerDbCommand.ExecuteReaderAsync(behavior);
        }

        public async Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            return await _innerDbCommand.ExecuteReaderAsync(behavior, cancellationToken);
        }

        public Task<object> ExecuteScalarAsync()
        {
            return _innerDbCommand.ExecuteScalarAsync();
        }

        public Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            return _innerDbCommand.ExecuteScalarAsync(cancellationToken);
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

            _innerDbCommand.Dispose();
        }
    }
}
