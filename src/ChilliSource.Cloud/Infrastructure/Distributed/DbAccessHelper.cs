using ChilliSource.Cloud.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Infrastructure.Distributed
{
    internal class DbAccessHelperAsync
    {
        readonly static RetryStrategyFactory _retryFactory = RetryStrategyFactory.Get();
        readonly static ReliableConnectionFactory _factory = ReliableConnectionFactory.Create(_retryFactory.CreateFixed(1, TimeSpan.FromSeconds(5)), _retryFactory.CreateFixed(1, TimeSpan.FromSeconds(5)));

        internal static IDbConnectionAsync CreateDbConnection(string connectionString)
        {
            return _factory.CreateConnectionAsync(connectionString);
        }

        internal static async Task<IDbCommandAsync> CreateDbCommand(IDbConnectionAsync connection, string text)
        {
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                await connection.OpenAsync();
            }

            var command = connection.CreateCommand();
            command.CommandText = text;
            command.CommandType = System.Data.CommandType.Text;

            return command;
        }
    }

    internal class DbAccessHelper
    {
        internal static IDbConnection CreateDbConnection(string connStr)
        {
            return DbAccessHelperAsync.CreateDbConnection(connStr).AsSync();
        }

        internal static IDbCommand CreateDbCommand(IDbConnection connection, string text)
        {
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
            }

            var command = connection.CreateCommand();
            command.CommandText = text;
            command.CommandType = System.Data.CommandType.Text;

            return command;
        }
    }
}
