using Microsoft.Data.SqlClient;

namespace DacHelpers.Helper;

/// <summary>
/// Abstract base class for test database helpers. Can be used to extend the functionality of the project
/// </summary>
public abstract class AbstractTestDatabaseHelper : ITestDatabaseHelper
{
    ///<inheritdoc />
    public string ConnectionString { get; }
    ///<inheritdoc />
    public string DatabaseName { get; }
    ///<inheritdoc />
    protected string ConnectionStringMaster { get; }

    /// <summary>
    /// Base implementation of helper to work with test-databases
    /// </summary>
    /// <param name="connectionString">Connectionstring to the database</param>
    /// <param name="databaseName">Name of the database</param>
    protected AbstractTestDatabaseHelper(string connectionString, string databaseName)
    {
        ConnectionString = connectionString;
        DatabaseName = databaseName;
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };
        ConnectionStringMaster = builder.ConnectionString;
    }

    ///<inheritdoc />
    public virtual async Task ResetDatabaseAsync()
    {
        //Using the undocumented sp_MSforeachtable with parameter @whereand
        const string whereClause = "@whereand='and o.Name NOT IN (SELECT t.name from sys.tables t where t.temporal_type = 1)'";
        const string resetDatabaseSql = "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';" +
                                        "EXEC sp_MSforeachtable 'ALTER TABLE ? DISABLE TRIGGER ALL';" +
                                       $"EXEC sp_MSforeachtable 'SET QUOTED_IDENTIFIER ON; DELETE FROM ?', {whereClause};" +
                                        "EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';" +
                                        "EXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL';";

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = resetDatabaseSql;
        await command.ExecuteNonQueryAsync();
    }

    ///<inheritdoc />
    public abstract Task CleanUpAsync();

    /// <summary>
    /// Drop the database if it exists. This can be used in implementations of CleanUp if it makes sense.
    /// </summary>
    protected virtual async Task DropDatabaseAsync()
    {
        var dropDatabaseSql = SqlQueryStrings.DropDatabaseSql(DatabaseName);

        using var connection = new SqlConnection(ConnectionStringMaster);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = dropDatabaseSql;
        await command.ExecuteNonQueryAsync();
    }
}
