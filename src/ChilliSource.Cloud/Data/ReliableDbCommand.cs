using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud
{
    internal class ReliableDbCommand : IDbCommandAsync, IDbCommand
    {
        private readonly SqlCommand _innerDbCommand;
        private readonly ReliableConnectionFactory _factory;
        private ReliableSqlConnection _connection;

        internal ReliableDbCommand(ReliableConnectionFactory factory, SqlCommand innerDbCommand, ReliableSqlConnection connection)
        {
            _factory = factory;
            _innerDbCommand = innerDbCommand;
            _connection = connection;
        }

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

        public IDataParameterCollection Parameters
        {
            get
            {
                return _innerDbCommand.Parameters;
            }
        }

        public IDbConnection Connection
        {
            get
            {
                return _connection;
            }

            set
            {
                var connection = _factory.Adapt(value);
                if (connection != null && !(connection is ReliableSqlConnection))
                    throw new ArgumentException("This connection is not compatible with a reliable database command. Try creating a new command object instead.");

                _connection = connection as ReliableSqlConnection;
                this._innerDbCommand.Connection = null;

                if (_connection != null)
                    _connection.SetCommandConnection(this._innerDbCommand);
            }
        }

        public IDbTransaction Transaction
        {
            get
            {
                return _innerDbCommand.Transaction;
            }

            set
            {
                _innerDbCommand.Transaction = (SqlTransaction)value;
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

        IDataParameterCollection IDbCommand.Parameters
        {
            get
            {
                return _innerDbCommand.Parameters;
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

        IDbConnectionAsync IDbCommandAsync.Connection
        {
            get
            {
                return _connection;
            }
        }

        public Task<int> ExecuteNonQueryAsync()
        {
            return _connection.ExecuteNonQueryAsync(_innerDbCommand, CancellationToken.None);
        }

        public Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            return _connection.ExecuteNonQueryAsync(_innerDbCommand, cancellationToken);
        }

        public Task<IDataReader> ExecuteReaderAsync()
        {
            return _connection.ExecuteCommandAsync<IDataReader>(_innerDbCommand, CancellationToken.None);
        }

        public Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
        {
            return _connection.ExecuteCommandAsync<IDataReader>(_innerDbCommand, cancellationToken);
        }

        public Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior)
        {
            return _connection.ExecuteCommandAsync<IDataReader>(_innerDbCommand, behavior, CancellationToken.None);
        }

        public Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            return _connection.ExecuteCommandAsync<IDataReader>(_innerDbCommand, behavior, cancellationToken);
        }

        public Task<object> ExecuteScalarAsync()
        {
            return _connection.ExecuteCommandAsync<object>(_innerDbCommand, CancellationToken.None);
        }

        public Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            return _connection.ExecuteCommandAsync<object>(_innerDbCommand, cancellationToken);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">A flag indicating that managed resources must be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            this._innerDbCommand.Dispose();
        }

        public void Prepare()
        {
            this._innerDbCommand.Prepare();
        }

        public void Cancel()
        {
            this._innerDbCommand.Cancel();
        }

        public IDbDataParameter CreateParameter()
        {
            return this._innerDbCommand.CreateParameter();
        }

        public int ExecuteNonQuery()
        {
            return _connection.ExecuteNonQuery(_innerDbCommand);
        }

        public IDataReader ExecuteReader()
        {
            return _connection.ExecuteCommand<IDataReader>(_innerDbCommand);
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return _connection.ExecuteCommand<IDataReader>(_innerDbCommand, behavior);
        }

        public object ExecuteScalar()
        {
            return _connection.ExecuteCommand<object>(_innerDbCommand);
        }
    }
}
