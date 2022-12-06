using Microsoft.Data.SqlClient;

namespace DacHelpers.Helper;

public interface ITestDatabaseHelper
{
    public string ConnectionString { get; }
    public string DatabaseName { get; }

    /// <summary>
    /// Reset all tables by disabling triggers and constraints, deleting and reenabling triggers and constraints - then reseeding identities at 0.
    /// More specific resetting might require your own implementation.
    /// Note also that this can be slow - so for simple tests consider just manually resetting with TRUNCATE TABLE.
    /// </summary>
    public Task ResetDatabaseAsync();

    /// <summary>
    /// Clean up after this database. For localhost just drop the database. For docker dispose of the container as well.
    /// </summary>
    /// <returns></returns>
    public Task CleanUpAsync();
}
