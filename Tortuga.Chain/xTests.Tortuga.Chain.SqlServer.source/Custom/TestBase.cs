using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tortuga.Chain;
using Tortuga.Chain.DataSources;
using Tortuga.Chain.SqlServer;
using Xunit.Abstractions;

namespace Tests
{
    public abstract partial class TestBase
    {
        public TestBase(ITestOutputHelper output)
        {
            m_Output = output;
        }

        protected readonly ITestOutputHelper m_Output;

        static protected readonly Dictionary<string, SqlServerDataSource> s_DataSources = new Dictionary<string, SqlServerDataSource>();
        static public readonly string AssemblyName = "SQLite";

        static TestBase()
        {
            foreach (ConnectionStringSettings con in ConfigurationManager.ConnectionStrings)
            {
                var ds = new SqlServerDataSource(con.Name, con.ConnectionString);
                s_DataSources.Add(con.Name, ds);
            }
        }

        public SqlServerDataSourceBase DataSource(string name, DataSourceType mode, [CallerMemberName] string caller = null)
        {
            m_Output.WriteLine($"{caller} requested Data Source {name} with mode {mode}");

            var ds = s_DataSources[name];
            switch (mode)
            {
                case DataSourceType.Normal: return AttachTracers(ds);
                case DataSourceType.Transactional: return AttachTracers(ds.BeginTransaction());
                case DataSourceType.Open:
                    var root = (IRootDataSource)ds;
                    return AttachTracers((SqlServerDataSourceBase)root.CreateOpenDataSource(root.CreateConnection(), null));
            }
            throw new ArgumentException($"Unkown mode {mode}");
        }

        public async Task<SqlServerDataSourceBase> DataSourceAsync(string name, DataSourceType mode, [CallerMemberName] string caller = null)
        {
            m_Output.WriteLine($"{caller} requested Data Source {name} with mode {mode}");

            var ds = s_DataSources[name];
            switch (mode)
            {
                case DataSourceType.Normal: return AttachTracers(ds);
                case DataSourceType.Transactional: return AttachTracers(await ds.BeginTransactionAsync());
                case DataSourceType.Open:
                    var root = (IRootDataSource)ds;
                    return AttachTracers((SqlServerDataSourceBase)root.CreateOpenDataSource(await root.CreateConnectionAsync(), null));
            }
            throw new ArgumentException($"Unkown mode {mode}");
        }

        void WriteDetails(ExecutionEventArgs e)
        {
            if (e.ExecutionDetails is SqlServerCommandExecutionToken)
            {
                m_Output.WriteLine("");
                m_Output.WriteLine("Command text: ");
                m_Output.WriteLine(e.ExecutionDetails.CommandText);
                //m_Output.Indent();
                foreach (var item in ((SqlServerCommandExecutionToken)e.ExecutionDetails).Parameters)
                    m_Output.WriteLine(item.ParameterName + ": " + (item.Value == null || item.Value == DBNull.Value ? "<NULL>" : item.Value));
                //m_Output.Unindent();
                m_Output.WriteLine("******");
                m_Output.WriteLine("");
            }
        }
    }
}