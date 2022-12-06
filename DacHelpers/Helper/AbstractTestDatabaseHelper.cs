using Microsoft.Data.SqlClient;

namespace DacHelpers.Helper;

public abstract class AbstractTestDatabaseHelper : ITestDatabaseHelper
{
    public string ConnectionString { get; }
    public string DatabaseName { get; }

    protected AbstractTestDatabaseHelper(string connectionString, string databaseName)
    {
        ConnectionString = connectionString;
        DatabaseName = databaseName;
    }

    /// <summary>
    /// Reset all tables by disabling triggers and constraints, deleting and reenabling triggers and constraints - then reseeding identities at 0.
    /// More specific resetting might require your own implementation.
    /// Note also that this can be slow - so for simple tests consider just manually resetting with TRUNCATE TABLE.
    /// </summary>
    public virtual async Task ResetDatabaseAsync()
    {
        const string resetDatabaseSql = "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';" +
                                        "EXEC sp_MSforeachtable 'ALTER TABLE ? DISABLE TRIGGER ALL';" +
                                        "EXEC sp_MSforeachtable 'DELETE FROM ?';" +
                                        "EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';" +
                                        "EXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL';" +
                                        "DBCC CHECKIDENT('{DatabaseName}', RESEED, 0);";

        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = resetDatabaseSql;
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Clean up after this database. For localhost just drop the database. For docker dispose of the container as well.
    /// </summary>
    public abstract Task CleanUpAsync();

    /// <summary>
    /// Drop the database if it exists. This can be used in implementations of CleanUp if it makes sense.
    /// </summary>
    protected virtual async Task DropDatabaseAsync()
    {
        var dropDatabaseSql = "USE master;" +
                             $"IF EXISTS (SELECT * FROM sys.databases WHERE name = '{DatabaseName}')" +
                              "BEGIN" +
                             $"ALTER DATABASE {DatabaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" +
                             $"DROP DATABASE {DatabaseName}" +
                              "END";
        
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = dropDatabaseSql;
        await command.ExecuteNonQueryAsync();
    }
}
