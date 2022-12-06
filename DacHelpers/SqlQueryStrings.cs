namespace DacHelpers;

public static class SqlQueryStrings
{
    public static string DropDatabaseSql(string databaseName)
    {
        return $"USE master;{Environment.NewLine}" +
               $"IF EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}'){Environment.NewLine}" +
               $"BEGIN{Environment.NewLine}" +
               $"ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;{Environment.NewLine}" +
               $"DROP DATABASE {databaseName};{Environment.NewLine}" +
               $"END{Environment.NewLine}";
    }

    public static string DropAndCreateDatabase(string databaseName)
    {
        return DropDatabaseSql(databaseName) + $"CREATE DATABASE {databaseName};{Environment.NewLine}";
    }
}
