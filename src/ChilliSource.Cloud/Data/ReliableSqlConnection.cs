/* 
Implementation based on MS Enterprise Library 6 – April 2013. https://msdn.microsoft.com/en-us/library/dn169621.aspx
Find the full Microsoft Public License at ThirdPartyLicenses/MS-PL.md 
*/

using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace ChilliSource.Cloud.Data
{
    internal class ReliableSqlConnection : IDbConnectionAsync, IDbConnection
    {
        private readonly SqlConnection _innerConnection;
        private readonly RetryPolicy _connectionRetryPolicy;
        private readonly RetryPolicy _commandRetryPolicy;
        private readonly RetryPolicy _connectionStringFailoverPolicy;
        private readonly ReliableConnectionFactory _factory;

        public ReliableSqlConnection(ReliableConnectionFactory factory, SqlConnection sqlConnection, RetryPolicy connectionRetryPolicy, RetryPolicy commandRetryPolicy)
        {
            this._factory = factory;
            this._innerConnection = sqlConnection;
            this._connectionRetryPolicy = connectionRetryPolicy;
            this._commandRetryPolicy = commandRetryPolicy;
            this._connectionStringFailoverPolicy = new RetryPolicy<ReliableSqlConnection.NetworkConnectivityErrorDetectionStrategy>(1, TimeSpan.FromMilliseconds(1.0));
        }

        public string ConnectionString
        {
            get
            {
                return this._innerConnection.ConnectionString;
            }
            set
            {
                this._innerConnection.ConnectionString = value;
            }
        }

        public int ConnectionTimeout
        {
            get
            {
                return this._innerConnection.ConnectionTimeout;
            }
        }

        public string Database
        {
            get
            {
                return this._innerConnection.Database;
            }
        }

        internal void SetCommandConnection(SqlCommand dbCommand)
        {
            dbCommand.Connection = _innerConnection;
        }

        public ConnectionState State
        {
            get
            {
                return this._innerConnection.State;
            }
        }

        private Task ExecuteFailoverPolicyAsync(RetryPolicy retryPolicy, Func<Task> taskFunc, CancellationToken cancellationToken)
        {
            return retryPolicy.ExecuteAsync(new Func<Task>(() =>
                this._connectionStringFailoverPolicy.ExecuteAsync(taskFunc, cancellationToken)
            ), cancellationToken);
        }

        private Task<T> ExecuteFailoverPolicyAsync<T>(RetryPolicy retryPolicy, Func<Task<T>> taskFunc, CancellationToken cancellationToken)
        {
            return retryPolicy.ExecuteAsync<T>(new Func<Task<T>>(() =>
                this._connectionStringFailoverPolicy.ExecuteAsync<T>(taskFunc, cancellationToken)
            ), cancellationToken);
        }

        private void ExecuteFailoverPolicy(RetryPolicy retryPolicy, Action action)
        {
            retryPolicy.ExecuteAction(() => this._connectionStringFailoverPolicy.ExecuteAction(action));
        }

        private T ExecuteFailoverPolicy<T>(RetryPolicy retryPolicy, Func<T> func)
        {
            return retryPolicy.ExecuteAction(() => this._connectionStringFailoverPolicy.ExecuteAction(func));
        }

        public Task OpenAsync()
        {
            return this.OpenAsync(CancellationToken.None);
        }

        public Task OpenAsync(CancellationToken cancellationToken)
        {
            return ExecuteFailoverPolicyAsync(this._connectionRetryPolicy, new Func<Task>(async () =>
            {
                if (this._innerConnection.State == ConnectionState.Open)
                    return;
                await this._innerConnection.OpenAsync(cancellationToken);
            }), cancellationToken);
        }

        internal async Task<int> ExecuteNonQueryAsync(SqlCommand command, CancellationToken cancellationToken)
        {
            var result = await this.ExecuteCommandAsync<ReliableSqlConnection.NonQueryResult>(command, cancellationToken);
            return result.RecordsAffected;
        }

        internal int ExecuteNonQuery(SqlCommand command)
        {
            var result = this.ExecuteCommand<ReliableSqlConnection.NonQueryResult>(command);
            return result.RecordsAffected;
        }

        internal Task<T> ExecuteCommandAsync<T>(SqlCommand command, CancellationToken cancellationToken)
        {
            return this.ExecuteCommandAsync<T>(command, CommandBehavior.Default, cancellationToken);
        }

        internal async Task<T> ExecuteCommandAsync<T>(SqlCommand command, CommandBehavior behavior, CancellationToken cancellationToken)
        {
            Type resultType = typeof(T);

            if (command.Connection == null)
            {
                command.Connection = this._innerConnection;
            }

            var actionResult = await ExecuteFailoverPolicyAsync<T>(this._commandRetryPolicy, new Func<Task<T>>(async () =>
            {
                if (typeof(IDataReader).IsAssignableFrom(resultType))
                {
                    var result = await command.ExecuteReaderAsync(behavior, cancellationToken);
                    return (T)(IDataReader)result;
                }
                if (resultType == typeof(XmlReader))
                {
                    throw new NotSupportedException("XmlReader is not supported");
                }

                if (resultType == typeof(ReliableSqlConnection.NonQueryResult))
                {
                    ReliableSqlConnection.NonQueryResult nonQueryResult = new ReliableSqlConnection.NonQueryResult()
                    {
                        RecordsAffected = await command.ExecuteNonQueryAsync(cancellationToken)
                    };
                    return (T)(object)nonQueryResult;
                }

                var obj = await command.ExecuteScalarAsync(cancellationToken);
                if (obj != null)
                {
                    if (resultType == typeof(object))
                        return (T)obj;
                    return (T)Convert.ChangeType(obj, resultType, (IFormatProvider)CultureInfo.InvariantCulture);
                }
                return default(T);
            }), cancellationToken);

            return actionResult;
        }

        internal T ExecuteCommand<T>(SqlCommand command)
        {
            return this.ExecuteCommand<T>(command, CommandBehavior.Default);
        }

        internal T ExecuteCommand<T>(SqlCommand command, CommandBehavior behavior)
        {
            Type resultType = typeof(T);

            if (command.Connection == null)
            {
                command.Connection = this._innerConnection;
            }

            var actionResult = ExecuteFailoverPolicy<T>(this._commandRetryPolicy, () =>
            {
                if (typeof(IDataReader).IsAssignableFrom(resultType))
                {
                    var result = command.ExecuteReader(behavior);
                    return (T)(IDataReader)result;
                }
                if (resultType == typeof(XmlReader))
                {
                    throw new NotSupportedException("XmlReader is not supported");
                }

                if (resultType == typeof(ReliableSqlConnection.NonQueryResult))
                {
                    ReliableSqlConnection.NonQueryResult nonQueryResult = new ReliableSqlConnection.NonQueryResult()
                    {
                        RecordsAffected = command.ExecuteNonQuery()
                    };
                    return (T)(object)nonQueryResult;
                }

                var obj = command.ExecuteScalar();
                if (obj != null)
                {
                    if (resultType == typeof(object))
                        return (T)obj;
                    return (T)Convert.ChangeType(obj, resultType, (IFormatProvider)CultureInfo.InvariantCulture);
                }
                return default(T);
            });

            return actionResult;
        }

        public void Close()
        {
            this._innerConnection.Close();
        }

        public IDbCommandAsync CreateCommand()
        {
            return new ReliableDbCommand(this._factory, this._innerConnection.CreateCommand(), this);
        }

        public IDbTransaction BeginTransaction()
        {
            return this._innerConnection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return this._innerConnection.BeginTransaction(il);
        }

        public void ChangeDatabase(string databaseName)
        {
            this._innerConnection.ChangeDatabase(databaseName);
        }

        IDbCommand IDbConnection.CreateCommand()
        {
            return new ReliableDbCommand(this._factory, this._innerConnection.CreateCommand(), this);
        }

        public void Open()
        {
            ExecuteFailoverPolicy(this._connectionRetryPolicy, () =>
            {
                if (this._innerConnection.State == ConnectionState.Open)
                    return;
                this._innerConnection.Open();
            });
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
            if (this._innerConnection.State == ConnectionState.Open)
                this._innerConnection.Close();
            this._innerConnection.Dispose();
        }

        public IDbConnection AsSync()
        {
            return this;
        }

        private sealed class NonQueryResult
        {
            public int RecordsAffected { get; set; }
        }

        private sealed class NetworkConnectivityErrorDetectionStrategy : ITransientErrorDetectionStrategy
        {
            public bool IsTransient(Exception ex)
            {
                SqlException sqlException;
                return ex != null && (sqlException = ex as SqlException) != null && sqlException.Number == 11001;
            }
        }
    }
}
