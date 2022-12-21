namespace DacHelpers.Helper;

/// <summary>
/// Small helper class for working with a deployed test-database
/// </summary>
public class TestDatabaseHelper : AbstractTestDatabaseHelper
{
    /// <summary>
    /// Constructs a helper for working with test-databases
    /// </summary>
    /// <param name="connectionString">Connectionstring to the database</param>
    /// <param name="databaseName">Name of the database</param>
    public TestDatabaseHelper(string connectionString, string databaseName) : base(connectionString, databaseName)
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
