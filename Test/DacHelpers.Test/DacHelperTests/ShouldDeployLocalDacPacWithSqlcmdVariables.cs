using DacHelpers.Helper;
using PetaPoco;

namespace DacHelpers.Test.DacHelperTests;

[TestFixture, Parallelizable(ParallelScope.Self)]
public class ShouldDeployLocalDacPacWithSqlcmdVariables
{
    private ITestDatabaseHelper? testDatabaseHelper;
    private ITestDatabaseHelper? testDatabaseHelper2;

    [TearDown]
    public async Task TearDown()
    {
        if (testDatabaseHelper is not null)
        {
            await testDatabaseHelper.CleanUpAsync();
        }
        if (testDatabaseHelper2 is not null)
        {
            await testDatabaseHelper2.CleanUpAsync();
        }
    }

    [Test]
    public async Task ShouldDeploy()
    {
        //Deploy first dacpac
        const string databaseName1 = "ShouldDeployLocalDacPacWithSqlcmdVariables";
        testDatabaseHelper = await DacHelper.DropAndDeployLocalAsync("TestDatabase.dacpac", databaseName1);

        //Deploy database with dependency and sqlcmd variables referencing first database
        const string databaseName2 = "ShouldDeployLocalDacPacWithSqlcmdVariables2";
        var variableMap = new Dictionary<string, string>() { { "TestDatabase", databaseName1 } };
        testDatabaseHelper2 = await DacHelper.DropAndDeployLocalAsync("TestDatabase2.dacpac", databaseName2, variableMap);
        var connectionString = testDatabaseHelper2.ConnectionString;

        //Assert that the second database was deployed successfully
        using var database = new Database(connectionString, "Microsoft.Data.SqlClient");
        var version = await database.ExecuteScalarAsync<string>("SELECT @@@Version");
        Assert.That(version, Does.Contain("Microsoft SQL Server"));
        var rows = await database.FetchAsync<dynamic>("SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='View1';");
        var tableData = rows.Single();
        Assert.That(tableData.TABLE_CATALOG, Is.EqualTo(databaseName2));
        Assert.That(tableData.TABLE_SCHEMA, Is.EqualTo("dbo"));
        Assert.That(tableData.TABLE_NAME, Is.EqualTo("View1"));
        Assert.That(tableData.TABLE_TYPE, Is.EqualTo("VIEW"));
    }
}
