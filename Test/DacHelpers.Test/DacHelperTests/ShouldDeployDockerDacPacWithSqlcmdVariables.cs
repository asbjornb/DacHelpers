using DacHelpers.Helper;
using PetaPoco;

namespace DacHelpers.Test.DacHelperTests;

[TestFixture, Parallelizable(ParallelScope.Self)]
public class ShouldDeployDockerDacPacWithSqlcmdVariables
{
    private ITestDatabaseHelper? testDatabaseHelper;

    [TearDown]
    public async Task TearDown()
    {
        if (testDatabaseHelper is not null)
        {
            await testDatabaseHelper.CleanUpAsync();
        }
        //Need not dispose of second database inside, since container is already disposed and databases gone with it
    }

    [Test]
    public async Task ShouldDeploy()
    {
        //Deploy first dacpac
        const string databaseName1 = "ShouldDeployDockerDacPacWithSqlcmdVariables";
        testDatabaseHelper = await DacHelper.DeployDockerAsync("TestDatabase.dacpac", databaseName1);

        //Deploy database with dependency and sqlcmd variables referencing first database
        const string databaseName2 = "ShouldDeployDockerDacPacWithSqlcmdVariables2";
        var variableMap = new Dictionary<string, string>() { { "TestDatabase", databaseName1 } };
        var testDatabaseHelper2 = await DacHelper.DropAndDeployAsync("TestDatabase2.dacpac", databaseName2, testDatabaseHelper.ConnectionString, variableMap);
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
