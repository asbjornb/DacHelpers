using PetaPoco;

namespace DacHelpers.Test;

[TestFixture]
public class LocalDatabaseTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var dir = Path.GetDirectoryName(new Uri(typeof(LocalDatabaseTests).Assembly.Location).LocalPath);
        Directory.SetCurrentDirectory(dir!);
    }

    [Test]
    public async Task ShouldDeployToLocalDatabase()
    {
        const string dacpacPath = "TestDatabase.dacpac";
        const string databaseName = "TestDb";
        var testDatabaseHelper = await DacHelper.DropAndDeployLocalAsync(dacpacPath, databaseName);
        var connectionString = testDatabaseHelper.ConnectionString;

        using var database = new Database(connectionString, "Microsoft.Data.SqlClient");
        var result = await database.ExecuteScalarAsync<string>("SELECT @@@Version");
        Assert.That(result, Does.Contain("Microsoft SQL Server"));
    }
}
