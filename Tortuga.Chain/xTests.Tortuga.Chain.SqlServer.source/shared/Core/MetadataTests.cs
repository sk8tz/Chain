using Xunit;
using Xunit.Abstractions;

namespace Tests.Core
{
    public class MetadataTests : TestBase
    {
        public static BasicData Basic = new BasicData(s_DataSources.Values);
        public static TableData Tables = new TableData(s_DataSources.Values);
        public static ViewData Views = new ViewData(s_DataSources.Values);

        public MetadataTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory, MemberData("Basic")]
        public void Preload(string assemblyName, string dataSourceName, DataSourceType mode)
        {
            var dataSource = DataSource(dataSourceName, mode);
            try
            {
                dataSource.DatabaseMetadata.Preload();
            }
            finally
            {
                Release(dataSource);
            }

        }

        [Theory, MemberData("Tables")]
        public void GetTable(string assemblyName, string dataSourceName, DataSourceType mode, string tableName)
        {
            var dataSource = DataSource(dataSourceName, mode);
            try
            {
                dataSource.DatabaseMetadata.Reset();
                var table = dataSource.DatabaseMetadata.GetTableOrView(tableName);
                Assert.Equal(tableName, table.Name.ToString());
                Assert.NotEmpty(table.Columns);
            }
            finally
            {
                Release(dataSource);
            }

        }


        [Theory, MemberData("Tables")]
        public void GetTable_LowerCase(string assemblyName, string dataSourceName, DataSourceType mode, string tableName)
        {
            var dataSource = DataSource(dataSourceName, mode);
            try
            {
                dataSource.DatabaseMetadata.Reset();
                var table = dataSource.DatabaseMetadata.GetTableOrView(tableName.ToLower());
                Assert.Equal(tableName, table.Name.ToString());
                Assert.NotEmpty(table.Columns);
            }
            finally
            {
                Release(dataSource);
            }

        }

        [Theory, MemberData("Tables")]
        public void GetTable_UpperCase(string assemblyName, string dataSourceName, DataSourceType mode, string tableName)
        {
            var dataSource = DataSource(dataSourceName, mode);
            try
            {
                dataSource.DatabaseMetadata.Reset();
                var table = dataSource.DatabaseMetadata.GetTableOrView(tableName.ToUpper());
                Assert.Equal(tableName, table.Name.ToString());
                Assert.NotEmpty(table.Columns);
            }
            finally
            {
                Release(dataSource);
            }

        }

        [Theory, MemberData("Views")]
        public void GetView(string assemblyName, string dataSourceName, DataSourceType mode, string viewName)
        {
            var dataSource = DataSource(dataSourceName, mode);
            try
            {
                dataSource.DatabaseMetadata.Reset();
                var view = dataSource.DatabaseMetadata.GetTableOrView(viewName);
                Assert.Equal(viewName, view.Name.ToString());
                Assert.NotEmpty(view.Columns);
            }
            finally
            {
                Release(dataSource);
            }

        }

#if SQL_SERVER || POSTGRESQL
        [Theory, MemberData("Basic")]
        public void VerifyFunction1(string assemblyName, string dataSourceName, DataSourceType mode)
        {
            var dataSource = DataSource(dataSourceName, mode);
            try
            {
                dataSource.DatabaseMetadata.PreloadTableFunctions();
                var function = dataSource.DatabaseMetadata.GetTableFunction(TableFunction1Name);
                Assert.IsNotNull(function, $"Error reading function {TableFunction1Name}");

            }
            finally
            {
                Release(dataSource);
            }
        }
#endif

#if SQL_SERVER 
        [Theory, MemberData("Basic")]
        public void VerifyFunction2(string assemblyName, string dataSourceName, DataSourceType mode)
        {
            var dataSource = DataSource(dataSourceName, mode);
            try
            {
                dataSource.DatabaseMetadata.PreloadTableFunctions();
                var function = dataSource.DatabaseMetadata.GetTableFunction(TableFunction2Name);
                Assert.IsNotNull(function, $"Error reading function {TableFunction2Name}");

            }
            finally
            {
                Release(dataSource);
            }
        }
#endif

    }
}



