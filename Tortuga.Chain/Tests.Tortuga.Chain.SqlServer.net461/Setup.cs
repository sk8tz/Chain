using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Tortuga.Chain;
using Tortuga.Chain.DataSources;
namespace Tests
{
    [TestClass]
    public class Setup
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)

        {
            DataSource.GlobalExecutionCanceled += DefaultDispatcher_ExecutionCanceled;
            DataSource.GlobalExecutionError += DefaultDispatcher_ExecutionError;
            DataSource.GlobalExecutionFinished += DefaultDispatcher_ExecutionFinished;
            DataSource.GlobalExecutionStarted += DefaultDispatcher_ExecutionStarted;
        }

        static void DefaultDispatcher_ExecutionCanceled(object sender, ExecutionEventArgs e)
        {
            Debug.WriteLine("******");
            Debug.WriteLine($"Execution canceled: {e.ExecutionDetails.OperationName}. Duration: {e.Duration.Value.TotalSeconds.ToString("N3")} sec.");
            WriteDetails(e);
        }

        static void DefaultDispatcher_ExecutionError(object sender, ExecutionEventArgs e)
        {
            Debug.WriteLine("******");
            Debug.WriteLine($"Execution error: {e.ExecutionDetails.OperationName}. Duration: {e.Duration.Value.TotalSeconds.ToString("N3")} sec.");
            //WriteDetails(e);
        }

        static void DefaultDispatcher_ExecutionFinished(object sender, ExecutionEventArgs e)
        {
            Debug.WriteLine("******");
            Debug.WriteLine($"Execution finished: {e.ExecutionDetails.OperationName}. Duration: {e.Duration.Value.TotalSeconds.ToString("N3")} sec. Rows affected: {(e.RowsAffected != null ? e.RowsAffected.Value.ToString("N0") : "<NULL>")}.");
            //WriteDetails(e);
        }

        static void DefaultDispatcher_ExecutionStarted(object sender, ExecutionEventArgs e)
        {
            Debug.WriteLine("******");
            Debug.WriteLine($"Execution started: {e.ExecutionDetails.OperationName}");
            WriteDetails(e);
        }

        static void WriteDetails(ExecutionEventArgs e)
        {
            Debug.WriteLine("");
            Debug.WriteLine("Command text: ");
            Debug.WriteLine(e.ExecutionDetails.CommandText);
            //TODO: add parameter dump
            Debug.WriteLine("******");
            Debug.WriteLine("");
        }
    }
}
