namespace DacHelpers.Helper;

/// <summary>
/// Small helper class for working with a deployed test-database on localhost
/// </summary>
public class LocalTestDatabaseHelper : AbstractTestDatabaseHelper
{
    public LocalTestDatabaseHelper(string connectionString, string databaseName) : base(connectionString, databaseName)
    {
    }

    public async override Task CleanUpAsync()
    {
        await DropDatabaseAsync();
    }
}
