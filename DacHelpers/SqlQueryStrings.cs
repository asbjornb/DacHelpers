namespace DacHelpers;

/// <summary>
/// Helper class to use some sql strings that are used in multiple places
/// </summary>
public static class SqlQueryStrings
{
    /// <summary>
    /// SQL query (as a string) to drop a database if it exists. If unsure if it exists make sure to use connection string to master as connecting directly will fail.
    /// </summary>
    /// <param name="databaseName">Name of the database to drop</param>
    /// <returns>Sql drop query as string</returns>
    public static string DropDatabaseSql(string databaseName)
    {
        return $"USE master;{Environment.NewLine}" +
               $"IF EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}'){Environment.NewLine}" +
               $"BEGIN{Environment.NewLine}" +
               $"ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;{Environment.NewLine}" +
               $"DROP DATABASE {databaseName};{Environment.NewLine}" +
               $"END{Environment.NewLine}";
    }

    /// <summary>
    /// SQL query (as a string) to drop a database if it exists and then create a new database again with that new.
    /// If unsure if it exists make sure to use connection string to master as connecting directly will fail.
    /// </summary>
    /// <param name="databaseName">Name of the database to drop</param>
    /// <returns>Sql drop and create query as string</returns>
    public static string DropAndCreateDatabase(string databaseName)
    {
        return DropDatabaseSql(databaseName) + $"CREATE DATABASE {databaseName};{Environment.NewLine}";
    }
}
