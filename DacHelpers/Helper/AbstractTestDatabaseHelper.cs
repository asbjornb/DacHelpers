﻿using Microsoft.Data.SqlClient;

namespace DacHelpers.Helper;

/// <summary>
/// Abstract base class for test database helpers. Can be used to extend the functionality of the project
/// </summary>
public abstract class AbstractTestDatabaseHelper : ITestDatabaseHelper
{
    public string ConnectionString { get; }
    public string DatabaseName { get; }
    protected string ConnectionStringMaster { get; }

    protected AbstractTestDatabaseHelper(string connectionString, string databaseName)
    {
        ConnectionString = connectionString;
        DatabaseName = databaseName;
        ConnectionStringMaster = ConnectionString.Replace($"Initial Catalog={DatabaseName};", "Initial Catalog=master;");
    }

    /// <summary>
    /// Reset all tables by disabling triggers and constraints, deleting and reenabling triggers and constraints - then reseeding identities at 0.
    /// More specific resetting might require your own implementation.
    /// Note also that this can be slow - so for simple tests consider just manually resetting with TRUNCATE TABLE.
    /// </summary>
    public virtual async Task ResetDatabaseAsync()
    {
        //Using the undocumented sp_MSforeachtable with parameter @whereand
        const string whereClause = "@@whereand='and o.Name NOT IN (SELECT t.name from sys.tables t where t.temporal_type = 1)'";
        const string resetDatabaseSql = "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';" +
                                        "EXEC sp_MSforeachtable 'ALTER TABLE ? DISABLE TRIGGER ALL';" +
                                       $"EXEC sp_MSforeachtable 'SET QUOTED_IDENTIFIER ON; DELETE FROM ?' {whereClause};" +
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
        var dropDatabaseSql = SqlQueryStrings.DropDatabaseSql(DatabaseName);

        using var connection = new SqlConnection(ConnectionStringMaster);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = dropDatabaseSql;
        await command.ExecuteNonQueryAsync();
    }
}
