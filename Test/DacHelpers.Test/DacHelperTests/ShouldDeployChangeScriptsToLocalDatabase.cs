using PetaPoco;

namespace DacHelpers.Test.DacHelperTests;

[TestFixture, Parallelizable(ParallelScope.Self)]
public class ShouldDeployChangeScriptsToLocalDatabase
{
    const string databaseName = "ShouldDeployChangeScriptsToLocalDatabase";
    private readonly string connectionString = $"Data Source=localhost;Initial Catalog={databaseName};Encrypt=False;Integrated Security=SSPI;";

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        using var database = new Database(connectionString, "Microsoft.Data.SqlClient");
        var dropDatabaseSql = SqlQueryStrings.DropDatabaseSql(databaseName);
        await database.ExecuteAsync(dropDatabaseSql);
    }

    [Test]
    public async Task ShouldDeployLocalWithFolder()
    {
        await DacHelper.DropAndDeployChangeScriptsLocalAsync(databaseName, "TestAssets/Changescripts");

        //Test table exist and that column [SomeDecimal] decimal(18,2) is added
        using var database = new Database(connectionString, "Microsoft.Data.SqlClient");
        var rows = await database.FetchAsync<dynamic>("SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='TestTable' AND COLUMN_NAME='SomeDecimal';");
        Assert.That(rows, Has.Count.EqualTo(1));
        var tableData = rows.Single();
        Assert.That(tableData.TABLE_CATALOG, Is.EqualTo(databaseName));
        Assert.That(tableData.TABLE_SCHEMA, Is.EqualTo("dbo"));
        Assert.That(tableData.TABLE_NAME, Is.EqualTo("TestTable"));
        Assert.That(tableData.COLUMN_NAME, Is.EqualTo("SomeDecimal"));
        Assert.That(tableData.DATA_TYPE, Is.EqualTo("decimal"));
    }

    [Test]
    public async Task ShouldDeployLocalWithListOfScripts()
    {
        var scripts = new List<string>
        {
            "TestAssets/Changescripts/SomeOldState/OldTable.sql",
            "TestAssets/Changescripts/001AddInitialTable.sql",
            "TestAssets/Changescripts/002AddColumnToTable.sql"
        };
        await DacHelper.DropAndDeployChangeScriptsLocalAsync(databaseName, scripts);

        //Test table exist and that column [SomeDecimal] decimal(18,2) is added
        using var database = new Database(connectionString, "Microsoft.Data.SqlClient");
        var rows = await database.FetchAsync<dynamic>("SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='TestTable' AND COLUMN_NAME='SomeDecimal';");
        Assert.That(rows, Has.Count.EqualTo(1));
        var tableData = rows.Single();
        Assert.That(tableData.TABLE_CATALOG, Is.EqualTo(databaseName));
        Assert.That(tableData.TABLE_SCHEMA, Is.EqualTo("dbo"));
        Assert.That(tableData.TABLE_NAME, Is.EqualTo("TestTable"));
        Assert.That(tableData.COLUMN_NAME, Is.EqualTo("SomeDecimal"));
        Assert.That(tableData.DATA_TYPE, Is.EqualTo("decimal"));

        //Test table OldTable exists
        rows = await database.FetchAsync<dynamic>("SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='OldTable';");
        Assert.That(rows, Has.Count.EqualTo(1));
    }
}
