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
        var version = await database.ExecuteScalarAsync<string>("SELECT @@@Version");
        Assert.That(version, Does.Contain("Microsoft SQL Server"));
        var rows = await database.FetchAsync<dynamic>("SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='Table1';");
        Assert.That(rows, Has.Count.EqualTo(1));
        var tableData = rows.Single();
        Assert.That(tableData.TABLE_CATALOG, Is.EqualTo(databaseName));
        Assert.That(tableData.TABLE_SCHEMA, Is.EqualTo("dbo"));
        Assert.That(tableData.TABLE_NAME, Is.EqualTo("Table1"));
    }
}
