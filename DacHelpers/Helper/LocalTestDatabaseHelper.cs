namespace DacHelpers.Helper;

/// <summary>
/// Small helper class for working with a deployed test-database on localhost
/// </summary>
public class LocalTestDatabaseHelper : AbstractTestDatabaseHelper
{
    /// <summary>
    /// Constructs a helper for working with localhost test-databases
    /// </summary>
    /// <param name="connectionString">Connectionstring to the database</param>
    /// <param name="databaseName">Name of the database</param>
    public LocalTestDatabaseHelper(string connectionString, string databaseName) : base(connectionString, databaseName)
    {
    }

    /// <summary>
    /// Drops the database
    /// </summary>
    public async override Task CleanUpAsync()
    {
        await DropDatabaseAsync();
    }
}
