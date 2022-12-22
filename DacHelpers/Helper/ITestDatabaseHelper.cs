namespace DacHelpers.Helper;

/// <summary>
/// Small helper class for working with a deployed test-database
/// </summary>
public interface ITestDatabaseHelper
{
    /// <summary>
    /// Connection string to the database
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// Name of the database
    /// </summary>
    public string DatabaseName { get; }

    /// <summary>
    /// Reset all tables by disabling triggers and constraints, deleting and reenabling triggers and constraints.
    /// More specific resetting like reseeding identities or clearing history for temporal tables requires your own implementation.
    /// Note also that this can be slow - so for simple tests consider just manually resetting with TRUNCATE TABLE.
    /// </summary>
    public Task ResetDatabaseAsync();

    /// <summary>
    /// Clean up after this database. For docker dispose of the container. Else just drop the database.
    /// </summary>
    public Task CleanUpAsync();
}
