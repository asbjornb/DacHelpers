namespace DacHelpers.Helper;

/// <summary>
/// Small helper class for working with a deployed test-database on docker on localhost
/// </summary>
public class DockerTestDatabaseHelper : AbstractTestDatabaseHelper
{
    private readonly Func<Task> disposeContainerActionAsync;

    /// <summary>
    /// Constructs a helper for working with local docker test-databases
    /// </summary>
    /// <param name="connectionString">Connectionstring to the database</param>
    /// <param name="databaseName">Name of the database</param>
    /// <param name="disposeContainerActionAsync">An async function that disposes the container</param>
    public DockerTestDatabaseHelper(string connectionString, string databaseName, Func<Task> disposeContainerActionAsync) : base(connectionString, databaseName)
    {
        this.disposeContainerActionAsync = disposeContainerActionAsync;
    }

    /// <summary>
    /// Stops and kills the container
    /// </summary>
    public async override Task CleanUpAsync()
    {
        await disposeContainerActionAsync();
    }
}
