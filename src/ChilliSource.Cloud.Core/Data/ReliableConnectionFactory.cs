using ChilliSource.Cloud.Core.Adapters;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Factory to create retry strategies
    /// </summary>
    public class RetryStrategyFactory
    {
        private RetryStrategyFactory() { }

        /// <summary>
        /// Initializes a new instance of the FixedInterval class with the specified number of retry attempts, time interval, retry strategy, and fast start option.
        /// </summary>        
        /// <param name="retryCount">The number of retry attempts.</param>
        /// <param name="retryInterval">The time interval between retries.</param>
        /// <param name="firstFastRetry">true to immediately retry in the first attempt; otherwise, false. The subsequent retries will remain subject to the configured retry interval.</param>
        public RetryStrategy CreateFixed(int retryCount, TimeSpan retryInterval, bool firstFastRetry = true)
        {
            return new FixedInterval("fixed_" + Guid.NewGuid(), retryCount, retryInterval, firstFastRetry);
        }

        /// <summary>
        /// Creates a factory instance.
        /// </summary>
        /// <returns>A factory instance</returns>
        public static RetryStrategyFactory Get()
        {
            return new RetryStrategyFactory();
        }
    }

    /// <summary>
    /// Factory to create a reliable Sql Connection with retry strategies.
    /// </summary>
    public class ReliableConnectionFactory
    {
        RetryPolicy _connectionRetryPolicy;
        RetryPolicy _commandRetryPolicy;

        private ReliableConnectionFactory(RetryStrategy connectionRetryStrategy, RetryStrategy commandRetryStrategy)
        {
            _connectionRetryPolicy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(connectionRetryStrategy);
            _commandRetryPolicy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(commandRetryStrategy);
        }

        /// <summary>
        /// Creates a reliable Sql Connection with retry strategies.
        /// </summary>
        /// <param name="connectionRetryStrategy">Connection retry Strategy</param>
        /// <param name="commandRetryStrategy">Command retry Strategy</param>
        /// <returns></returns>
        public static ReliableConnectionFactory Create(RetryStrategy connectionRetryStrategy, RetryStrategy commandRetryStrategy)
        {
            return new ReliableConnectionFactory(connectionRetryStrategy, commandRetryStrategy);
        }

        /// <summary>
        /// Creates a reliable sql connection and returns its asynchronous interface. It falls back to a simple connection for other providers.
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <returns>A reliable sql connection with retry policies. </returns>
        public IDbConnectionAsync CreateConnectionAsync(string connectionString)
        {
            var providerName = getProviderName(connectionString);
            var factory = DbProviderFactories.GetFactory(providerName);

            var conn = factory.CreateConnection();
            conn.ConnectionString = connectionString;

            return this.Adapt(conn);
        }

        private string getProviderName(string connectionString)
        {
            var connStrSettings = new ConnectionStringSettings("default", connectionString);
            return String.IsNullOrEmpty(connStrSettings.ProviderName) ? "System.Data.SqlClient" : connStrSettings.ProviderName;
        }

        internal IDbConnectionAsync Adapt(IDbConnection connection)
        {
            if (connection == null)
                return null;

            if (connection is IDbConnectionAsync)
            {
                return connection as IDbConnectionAsync;
            }
            else if (connection is SqlConnection)
            {
                return new ReliableSqlConnection(this, connection as SqlConnection, _connectionRetryPolicy, _commandRetryPolicy);
            }
            else if (connection is DbConnection)
            {
                return new DbConnectionAsyncAdapter(connection as DbConnection);
            }
            else
            {
                throw new ApplicationException("The connection could not be adapted by ReliableConnectionFactory.");
            }
        }

        /// <summary>
        /// Creates a reliable sql connection. It falls back to a simple connection for other providers.
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <returns>A reliable sql connection with retry policies. </returns>
        public IDbConnection CreateConnection(string connectionString)
        {
            return this.CreateConnectionAsync(connectionString).AsSync();
        }
    }
}
