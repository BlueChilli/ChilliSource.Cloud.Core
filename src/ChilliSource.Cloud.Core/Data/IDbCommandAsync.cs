using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Core
{
    /// <summary>
    /// Describes asynchronous operations for a database command.
    /// </summary>
    public interface IDbCommandAsync : IDisposable
    {
        /// <summary>Gets or sets the <see cref="T:System.Data.IDbConnection" /> used by this instance of the <see cref="T:System.Data.IDbCommand" />.</summary>
        /// <returns>The connection to the data source.</returns>
        IDbConnectionAsync Connection { get; }

        /// <summary>Gets or sets the text command to run against the data source.</summary>
        /// <returns>The text command to execute. The default value is an empty string ("").</returns>
        string CommandText { get; set; }

        /// <summary>Gets or sets the wait time before terminating the attempt to execute a command and generating an error.</summary>
        /// <returns>The time (in seconds) to wait for the command to execute. The default value is 30 seconds.</returns>
        /// <exception cref="T:System.ArgumentException">The property value assigned is less than 0. </exception>
        int CommandTimeout { get; set; }

        /// <summary>Indicates or specifies how the <see cref="P:System.Data.IDbCommand.CommandText" /> property is interpreted.</summary>
        /// <returns>One of the <see cref="T:System.Data.CommandType" /> values. The default is Text.</returns>
        CommandType CommandType { get; set; }

        /// <summary>Gets the <see cref="T:System.Data.IDataParameterCollection" />.</summary>
        /// <returns>The parameters of the SQL statement or stored procedure.</returns>
        IDataParameterCollection Parameters { get; }

        /// <summary>Gets or sets how command results are applied to the <see cref="T:System.Data.DataRow" /> when used by the <see cref="M:System.Data.IDataAdapter.Update(System.Data.DataSet)" /> method of a <see cref="T:System.Data.Common.DbDataAdapter" />.</summary>
        /// <returns>One of the <see cref="T:System.Data.UpdateRowSource" /> values. The default is Both unless the command is automatically generated. Then the default is None.</returns>
        /// <exception cref="T:System.ArgumentException">The value entered was not one of the <see cref="T:System.Data.UpdateRowSource" /> values. </exception>
        UpdateRowSource UpdatedRowSource { get; set; }

        /// <summary>An asynchronous version of <see cref="M:ChilliSource.Cloud.Core.Data.IDbCommandAsync.ExecuteNonQuery" />, which executes a SQL statement against a connection object.Invokes <see cref="M:ChilliSource.Cloud.Core.Data.IDbCommandAsync.ExecuteNonQueryAsync(System.Threading.CancellationToken)" /> with CancellationToken.None.</summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
        Task<int> ExecuteNonQueryAsync();

        /// <summary>This is the asynchronous version of <see cref="M:ChilliSource.Cloud.Core.Data.IDbCommandAsync.ExecuteNonQuery" />. Providers should override with an appropriate implementation. The cancellation token may optionally be ignored.The default implementation invokes the synchronous <see cref="M:ChilliSource.Cloud.Core.Data.IDbCommandAsync.ExecuteNonQuery" /> method and returns a completed task, blocking the calling thread. The default implementation will return a cancelled task if passed an already cancelled cancellation token.  Exceptions thrown by <see cref="M:ChilliSource.Cloud.Core.Data.IDbCommandAsync.ExecuteNonQuery" /> will be communicated via the returned Task Exception property.Do not invoke other methods and properties of the DbCommand object until the returned Task is complete.</summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
        Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken);

        /// <summary>An asynchronous version of ExecuteReader, which executes the <see cref="P:ChilliSource.Cloud.Core.Data.IDbCommandAsync.CommandText" /> against the <see cref="P:ChilliSource.Cloud.Core.Data.IDbCommandAsync.Connection" /> and returns a <see cref="T:System.Data.Common.DbDataReader" />.Invokes <see cref="M:ChilliSource.Cloud.Core.Data.IDbCommandAsync.ExecuteDbDataReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken)" /> with CancellationToken.None.</summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
        /// <exception cref="T:System.ArgumentException">An invalid <see cref="T:System.Data.CommandBehavior" /> value.</exception>
        Task<IDataReader> ExecuteReaderAsync();


        /// <summary>An asynchronous version of ExecuteReader, which executes the <see cref="P:ChilliSource.Cloud.Core.Data.IDbCommandAsync.CommandText" /> against the <see cref="P:ChilliSource.Cloud.Core.Data.IDbCommandAsync.Connection" /> and returns a <see cref="T:System.Data.Common.DbDataReader" />.Invokes <see cref="M:ChilliSource.Cloud.Core.Data.IDbCommandAsync.ExecuteDbDataReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken)" />.</summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <param name="behavior">One of the <see cref="T:System.Data.CommandBehavior" /> values.</param>
        /// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
        /// <exception cref="T:System.ArgumentException">An invalid <see cref="T:System.Data.CommandBehavior" /> value.</exception>
        Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior);

        /// <summary>An asynchronous version of ExecuteReader, which executes the <see cref="P:ChilliSource.Cloud.Core.Data.IDbCommandAsync.CommandText" /> against the <see cref="P:ChilliSource.Cloud.Core.Data.IDbCommandAsync.Connection" /> and returns a <see cref="T:System.Data.Common.DbDataReader" />. This method propagates a notification that operations should be canceled.Invokes <see cref="M:ChilliSource.Cloud.Core.Data.IDbCommandAsync.ExecuteDbDataReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken)" />.</summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
        /// <exception cref="T:System.ArgumentException">An invalid <see cref="T:System.Data.CommandBehavior" /> value.</exception>
        Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken);

        /// <summary>Invokes <see cref="M:ChilliSource.Cloud.Core.Data.IDbCommandAsync.ExecuteReaderAsync(System.Data.CommandBehavior,System.Threading.CancellationToken)" />.</summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <param name="behavior">One of the <see cref="T:System.Data.CommandBehavior" /> values.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
        /// <exception cref="T:System.ArgumentException">An invalid <see cref="T:System.Data.CommandBehavior" /> value.</exception>
        Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken);

        /// <summary>An asynchronous version of ExecuteScalar, which executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.Invokes <see cref="M:ChilliSource.Cloud.Core.Data.IDbCommandAsync.ExecuteScalarAsync(System.Threading.CancellationToken)" /> with CancellationToken.None.</summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
        Task<object> ExecuteScalarAsync();

        /// <summary>This is the asynchronous version of ExecuteScalar. Providers should override with an appropriate implementation. The cancellation token may optionally be ignored.The default implementation invokes the synchronous <see cref="M:ChilliSource.Cloud.Core.Data.IDbCommandAsync.ExecuteScalar" /> method and returns a completed task, blocking the calling thread. The default implementation will return a cancelled task if passed an already cancelled cancellation token. Exceptions thrown by ExecuteScalar will be communicated via the returned Task Exception property.Do not invoke other methods and properties of the DbCommand object until the returned Task is complete.</summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="T:System.Data.Common.DbException">An error occurred while executing the command text.</exception>
        Task<object> ExecuteScalarAsync(CancellationToken cancellationToken);
    }
}
