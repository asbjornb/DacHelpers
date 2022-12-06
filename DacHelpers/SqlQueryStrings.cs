namespace DacHelpers;

public static class SqlQueryStrings
{
    public static string DropDatabaseSql(string databaseName)
    {
        return "USE master;" +
              $"IF EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}')" +
               "BEGIN" +
              $"ALTER DATABASE {databaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" +
              $"DROP DATABASE {databaseName};" +
               "END";
    }
}
