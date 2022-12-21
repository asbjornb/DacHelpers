using DacHelpers.Helper;
using PetaPoco;

namespace DacHelpers.Test.DacHelperTests;

[TestFixture, Parallelizable(ParallelScope.Self)]
public class ShouldDeployToLocalDatabase
{
    private ITestDatabaseHelper? testDatabaseHelper;

    [TearDown]
    public async Task TearDown()
    {
        if (testDatabaseHelper is not null)
        {
            await testDatabaseHelper.CleanUpAsync();
        }
    }

    [Test]
    public async Task ShouldDeploy()
    {
        const string databaseName = "ShouldDeployToLocalDatabase";
        testDatabaseHelper = await DacHelper.DropAndDeployLocalAsync(dacpacPath: "TestDatabase.dacpac", databaseName);
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
