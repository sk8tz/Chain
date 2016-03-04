using System;

namespace Tortuga.Chain.DataSources
{
    /// <summary>
    /// Class DataSource.
    /// </summary>
    public abstract class DataSource
    {

        /// <summary>
        /// Gets or sets the default connection timeout.
        /// </summary>
        /// <value>
        /// The default connection timeout.
        /// </value>
        public TimeSpan? DefaultConnectionTimeout { get; set; }

        /// <summary>
        /// Gets or sets the default command timeout.
        /// </summary>
        /// <value>The default command timeout.</value>
        public TimeSpan? DefaultCommandTimeout { get; set; }
        /// <summary>
        /// Gets the name of the data source.
        /// </summary>
        /// <value>
        /// The name of the data source.
        /// </value>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether strict mode is enabled.
        /// </summary>
        /// <remarks>Strict mode requires all properties that don't represent columns to be marked with the NotMapped attribute.</remarks>
        public bool StrictMode { get; set; }

        /// <summary>
        /// Data sources can use this to indicate an error has occurred.
        /// </summary>
        /// <param name="executionDetails">The executionDetails.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="error">The error.</param>
        /// <param name="state">User defined state, usually used for logging.</param>
        protected void OnExecutionError(ExecutionToken executionDetails, DateTimeOffset startTime, DateTimeOffset endTime, Exception error, object state)
        {
            if (executionDetails == null)
                throw new ArgumentNullException("executionDetails", "executionDetails is null.");
            if (error == null)
                throw new ArgumentNullException("error", "error is null.");

            if (ExecutionError != null)
                ExecutionError(this, new ExecutionEventArgs(executionDetails, startTime, endTime, error, state));
            if (!SuppressGlobalEvents && GlobalExecutionError != null)
                GlobalExecutionError(this, new ExecutionEventArgs(executionDetails, startTime, endTime, error, state));
        }

        /// <summary>
        /// Data sources can use this to indicate an executionDetails has finished.
        /// </summary>
        /// <param name="executionDetails">The executionDetails.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="rowsAffected">The number of rows affected.</param>
        /// <param name="state">User defined state, usually used for logging.</param>
        protected void OnExecutionFinished(ExecutionToken executionDetails, DateTimeOffset startTime, DateTimeOffset endTime, int? rowsAffected, object state)
        {
            if (executionDetails == null)
                throw new ArgumentNullException("executionDetails", "executionDetails is null.");

            if (ExecutionFinished != null)
                ExecutionFinished(this, new ExecutionEventArgs(executionDetails, startTime, endTime, rowsAffected, state));
            if (!SuppressGlobalEvents && GlobalExecutionFinished != null)
                GlobalExecutionFinished(this, new ExecutionEventArgs(executionDetails, startTime, endTime, rowsAffected, state));
        }

        /// <summary>
        /// Data sources can use this to indicate an executionDetails has begun.
        /// </summary>
        /// <param name="executionDetails">The executionDetails.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="state">User defined state, usually used for logging.</param>
        protected void OnExecutionStarted(ExecutionToken executionDetails, DateTimeOffset startTime, object state)
        {
            if (executionDetails == null)
                throw new ArgumentNullException("executionDetails", "executionDetails is null.");


            if (ExecutionStarted != null)
                ExecutionStarted(this, new ExecutionEventArgs(executionDetails, startTime, state));
            if (!SuppressGlobalEvents && GlobalExecutionStarted != null)
                GlobalExecutionStarted(this, new ExecutionEventArgs(executionDetails, startTime, state));
        }

        /// <summary>
        /// Data sources can use this to indicate an executionDetails was canceled.
        /// </summary>
        /// <param name="executionDetails">The executionDetails.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="state">User defined state, usually used for logging.</param>
        /// <remarks>This is not used for timeouts.</remarks>
        protected void OnExecutionCanceled(ExecutionToken executionDetails, DateTimeOffset startTime, DateTimeOffset endTime, object state)
        {
            if (executionDetails == null)
                throw new ArgumentNullException("executionDetails", "executionDetails is null.");

            if (ExecutionCanceled != null)
                ExecutionCanceled(this, new ExecutionEventArgs(executionDetails, startTime, endTime, state));
            if (!SuppressGlobalEvents && GlobalExecutionCanceled != null)
                GlobalExecutionCanceled(this, new ExecutionEventArgs(executionDetails, startTime, endTime, state));
        }

        /// <summary>
        /// Raised when a procedure call fails.
        /// </summary>
        public event EventHandler<ExecutionEventArgs> ExecutionError;

        /// <summary>
        /// Raised when a procedure call is successfully completed
        /// </summary>
        public event EventHandler<ExecutionEventArgs> ExecutionFinished;

        /// <summary>
        /// Raised when a executionDetails is canceled.
        /// </summary>
        /// <remarks>This is not used for timeouts.</remarks>
        public event EventHandler<ExecutionEventArgs> ExecutionCanceled;

        /// <summary>
        /// Raised when a procedure call is started
        /// </summary>
        public event EventHandler<ExecutionEventArgs> ExecutionStarted;

        /// <summary>
        /// Raises the <see cref="E:ExecutionError" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ExecutionEventArgs"/> instance containing the event data.</param>
        public void OnExecutionError(ExecutionEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e", "e is null.");

            if (ExecutionError != null)
                ExecutionError(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:ExecutionFinished" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ExecutionEventArgs"/> instance containing the event data.</param>
        public void OnExecutionFinished(ExecutionEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e", "e is null.");

            if (ExecutionFinished != null)
                ExecutionFinished(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:ExecutionStarted" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ExecutionEventArgs"/> instance containing the event data.</param>
        public void OnExecutionStarted(ExecutionEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e", "e is null.");

            if (ExecutionStarted != null)
                ExecutionStarted(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:ExecutionCanceled" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ExecutionEventArgs"/> instance containing the event data.</param>
        public void OnExecutionCanceled(ExecutionEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e", "e is null.");

            if (ExecutionCanceled != null)
                ExecutionCanceled(this, e);
        }

        /// <summary>
        /// Raised when a procedure call fails in any dispatcher.
        /// </summary>
        public static event EventHandler<ExecutionEventArgs> GlobalExecutionError;

        /// <summary>
        /// Raised when a procedure call is successfully completed in any dispatcher
        /// </summary>
        public static event EventHandler<ExecutionEventArgs> GlobalExecutionFinished;

        /// <summary>
        /// Raised when a executionDetails is canceled in any dispatcher.
        /// </summary>
        /// <remarks>This is not used for timeouts.</remarks>
        public static event EventHandler<ExecutionEventArgs> GlobalExecutionCanceled;

        /// <summary>
        /// Raised when a procedure call is started in any dispatcher
        /// </summary>
        public static event EventHandler<ExecutionEventArgs> GlobalExecutionStarted;

        /// <summary>
        /// Gets or sets a value indicating whether to suppress global events.
        /// </summary>
        /// <value>If <c>true</c>, this data source will not honor global event handlers.</value>
        public bool SuppressGlobalEvents { get; set; }

    }
}
