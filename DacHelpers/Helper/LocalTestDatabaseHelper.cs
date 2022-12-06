namespace DacHelpers.Helper;

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
