using DacHelpers.DockerHelpers;

namespace DacHelpers.Helper;

/// <summary>
/// Small helper class for working with a deployed test-database on docker on localhost
/// </summary>
public class DockerTestDatabaseHelper : AbstractTestDatabaseHelper
{
    private readonly DockerContainer container;

    /// <summary>
    /// Constructs a helper for working with local docker test-databases
    /// </summary>
    /// <param name="container">DockerContainer abstraction mostly used for disposing</param>
    /// <param name="databaseName">Name of the database</param>
    internal DockerTestDatabaseHelper(DockerContainer container, string databaseName) : base(container.GetConnectionString(databaseName), databaseName)
    {
        this.container = container;
    }

    /// <summary>
    /// Stops and kills the container
    /// </summary>
    public async override Task CleanUpAsync()
    {
        await container.DisposeAsync();
    }
}
