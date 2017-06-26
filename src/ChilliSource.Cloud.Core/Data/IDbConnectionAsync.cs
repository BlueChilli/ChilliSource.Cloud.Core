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
    /// Describes asynchronous operations for a database connection.
    /// </summary>
    public interface IDbConnectionAsync : IDisposable
    {
        /// <summary>Gets or sets the string used to open a database.</summary>
        /// <returns>A string containing connection settings.</returns>
        string ConnectionString { get; }

        /// <summary>Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.</summary>
        /// <returns>The time (in seconds) to wait for a connection to open. The default value is 15 seconds.</returns>
        int ConnectionTimeout { get; }

        /// <summary>Gets the name of the current database or the database to be used after a connection is opened.</summary>
        /// <returns>The name of the current database or the name of the database to be used once a connection is open. The default value is an empty string.</returns>        
        string Database { get; }

        /// <summary>Gets the current state of the connection.</summary>
        /// <returns>One of the <see cref="T:System.Data.ConnectionState" /> values.</returns>
        ConnectionState State { get; }

        /// <summary>
        /// Returns a set of synchronous database operations.
        /// </summary>
        /// <returns>Returns a set of synchronous database operations.</returns>
        IDbConnection AsSync();

        /// <summary>
        ///    Opens the database connection asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OpenAsync();

        /// <summary>
        ///    Opens the database connection asynchronously. The task can be cancelled via a CancellationToken.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OpenAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <returns></returns>
        IDbCommandAsync CreateCommand();
    }
}
