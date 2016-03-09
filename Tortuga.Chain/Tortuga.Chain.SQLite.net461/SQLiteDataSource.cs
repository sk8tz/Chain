﻿using System;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using Tortuga.Chain.CommandBuilders;
using Tortuga.Chain.Metadata;
using Tortuga.Chain.SQLite;

namespace Tortuga.Chain
{
    /// <summary>
    /// Class that represets a SQLite Datasource.
    /// </summary>
    public class SQLiteDataSource : SQLiteDataSourceBase
    {
        private readonly SQLiteConnectionStringBuilder m_ConnectionBuilder;
        private readonly SQLiteMetadataCache m_DatabaseMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteDataSource" /> class.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="connectionString"></param>
        public SQLiteDataSource(string connectionName, string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Connection string is null or emtpy.", "connectionString");

            m_ConnectionBuilder = new SQLiteConnectionStringBuilder(connectionString);
            if (string.IsNullOrEmpty(connectionName))
                Name = m_ConnectionBuilder.DataSource;
            else
                Name = connectionName;

            m_DatabaseMetadata = new SQLiteMetadataCache(m_ConnectionBuilder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteDataSource" /> class.
        /// </summary>
        /// <param name="connectionString"></param>
        public SQLiteDataSource(string connectionString)
            : this(null, connectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteDataSource" /> class. 
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="connectionStringBuilder"></param>
        public SQLiteDataSource(string connectionName, SQLiteConnectionStringBuilder connectionStringBuilder)
        {
            if(connectionStringBuilder == null)
                throw new ArgumentNullException("connectionStringBuilder", "connectionStringBuilder is null.");

            m_ConnectionBuilder = connectionStringBuilder;
            if (string.IsNullOrEmpty(connectionName))
                Name = m_ConnectionBuilder.DataSource;
            else
                Name = connectionName;

            m_DatabaseMetadata = new SQLiteMetadataCache(m_ConnectionBuilder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteDataSource" /> class.
        /// </summary>
        /// <param name="connectionStringBuilder"></param>
        public SQLiteDataSource(SQLiteConnectionStringBuilder connectionStringBuilder)
         : this(null, connectionStringBuilder)
        {
        }

        /// <summary>
        /// This object can be used to lookup database information.
        /// </summary>
        public override SQLiteMetadataCache DatabaseMetadata
        {
            get { return m_DatabaseMetadata; }
        }

        /// <summary>
        /// This object can be used to access the database connection string.
        /// </summary>
        internal string ConnectionString
        {
            get { return m_ConnectionBuilder.ConnectionString; }
        }

        /// <summary>
        /// Creates a new connection using the connection string in the app.config file.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public static SQLiteDataSource CreateFromConfig(string connectionName)
        {
            var settings = ConfigurationManager.ConnectionStrings[connectionName];
            if (settings == null)
                throw new InvalidOperationException("The configuration file does not contain a connection named " + connectionName);

            return new SQLiteDataSource(connectionName, settings.ConnectionString);
        }

        /// <summary>
        /// Creates and opens a new SQLite connection
        /// </summary>
        /// <returns></returns>
        /// <remarks>The caller of this method is responsible for closing the connection.</remarks>
        public SQLiteConnection CreateSQLiteConnection()
        {
            var con = new SQLiteConnection(ConnectionString);
            con.Open();

            //TODO: Research any potential PRAGMA/Rollback options

            return con;
        }

        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <param name="forwardEvents"></param>
        /// <returns></returns>
        /// <remarks>The caller of this method is responsible for closing the connection.</remarks>
        public virtual SQLiteTransactionalDataSource BeginTransaction(IsolationLevel? isolationLevel = null, bool forwardEvents = true)
        {
            return new SQLiteTransactionalDataSource(this, isolationLevel, forwardEvents);
        }

        /// <summary>
        /// Executes the specified operation.
        /// </summary>
        /// <param name="executionToken"></param>
        /// <param name="implementation"></param>
        /// <param name="state"></param>
        protected override void Execute(ExecutionToken<SQLiteCommand, SQLiteParameter> executionToken, Func<SQLiteCommand, int?> implementation, object state)
        {
            if (executionToken == null)
                throw new ArgumentNullException("executionToken", "executionToken is null.");
            if (implementation == null)
                throw new ArgumentNullException("implementation", "implementation is null.");

            var mode = DisableLocks ? LockType.None : (executionToken as SQLiteExecutionToken)?.LockType ?? LockType.Write;

            var startTime = DateTimeOffset.Now;
            OnExecutionStarted(executionToken, startTime, state);

            try
            {
                switch (mode)
                {
                    case LockType.Read: SyncLock.EnterReadLock(); break;
                    case LockType.Write: SyncLock.EnterWriteLock(); break;
                }

                using (var con = CreateSQLiteConnection())
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = executionToken.CommandText;
                        //TODO: add potential check for this type.
                        cmd.CommandType = executionToken.CommandType;
                        foreach (var param in executionToken.Parameters)
                            cmd.Parameters.Add(param);

                        var rows = implementation(cmd);
                        OnExecutionFinished(executionToken, startTime, DateTimeOffset.Now, rows, state);
                    }
                }
            }
            catch (SQLiteException ex)
            {
                ex.Data["DataSource"] = Name;
                ex.Data["Operation"] = executionToken.OperationName;
                ex.Data["CommandText"] = executionToken.CommandText;
                ex.Data["Parameters"] = executionToken.Parameters;
                OnExecutionError(executionToken, startTime, DateTimeOffset.Now, ex, state);
                throw;
            }
            finally
            {
                switch (mode)
                {
                    case LockType.Read: SyncLock.ExitReadLock(); break;
                    case LockType.Write: SyncLock.ExitWriteLock(); break;
                }
            }
        }

        /// <summary>
        /// Executes the specified operation asynchronously.
        /// </summary>
        /// <param name="executionToken"></param>
        /// <param name="implementation"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(ExecutionToken<SQLiteCommand, SQLiteParameter> executionToken, Func<SQLiteCommand, Task<int?>> implementation, CancellationToken cancellationToken, object state)
        {
            if (executionToken == null)
                throw new ArgumentNullException("executionToken", "executionToken is null.");
            if (implementation == null)
                throw new ArgumentNullException("implementation", "implementation is null.");

            var mode = DisableLocks ? LockType.None : (executionToken as SQLiteExecutionToken)?.LockType ?? LockType.Write;

            var startTime = DateTimeOffset.Now;
            OnExecutionStarted(executionToken, startTime, state);

            try
            {
                switch (mode)
                {
                    case LockType.Read: SyncLock.EnterReadLock(); break;
                    case LockType.Write: SyncLock.EnterWriteLock(); break;
                }

                using (var con = await CreateSQLiteConnectionAsync(cancellationToken).ConfigureAwait(false))
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = executionToken.CommandText;
                        cmd.CommandType = executionToken.CommandType;
                        foreach (var param in executionToken.Parameters)
                            cmd.Parameters.Add(param);

                        var rows = await implementation(cmd).ConfigureAwait(false);
                        OnExecutionFinished(executionToken, startTime, DateTimeOffset.Now, rows, state);
                    }
                }
            }
            catch (SQLiteException ex)
            {
                if (cancellationToken.IsCancellationRequested) //convert SQLiteException into a OperationCanceledException 
                {
                    var ex2 = new OperationCanceledException("Operation was canceled.", ex, cancellationToken);
                    ex2.Data["DataSource"] = Name;
                    ex2.Data["Operation"] = executionToken.OperationName;
                    ex2.Data["CommandText"] = executionToken.CommandText;
                    ex2.Data["Parameters"] = executionToken.Parameters;
                    OnExecutionError(executionToken, startTime, DateTimeOffset.Now, ex2, state);
                    throw ex2;
                }
                else
                {
                    ex.Data["DataSource"] = Name;
                    ex.Data["Operation"] = executionToken.OperationName;
                    ex.Data["CommandText"] = executionToken.CommandText;
                    ex.Data["Parameters"] = executionToken.Parameters;
                    OnExecutionError(executionToken, startTime, DateTimeOffset.Now, ex, state);
                    throw;
                }
            }
            finally
            {
                switch (mode)
                {
                    case LockType.Read: SyncLock.ExitReadLock(); break;
                    case LockType.Write: SyncLock.ExitWriteLock(); break;
                }
            }
        }

        private async Task<SQLiteConnection> CreateSQLiteConnectionAsync(CancellationToken cancellationToken)
        {
            var con = new SQLiteConnection(ConnectionString);
            await con.OpenAsync(cancellationToken).ConfigureAwait(false);

            //TODO: Add in needed PRAGMA statements

            return con;
        }
    }
}
