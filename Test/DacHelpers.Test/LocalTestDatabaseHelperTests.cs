using DacHelpers.Helper;
using PetaPoco;

namespace DacHelpers.Test;

[TestFixture]
public class LocalTestDatabaseHelperTests
{
    private const string databaseName= "LocalTestDatabaseHelperTests";
    private ITestDatabaseHelper sut;

    [SetUp]
    public async Task SetUp()
    {
        //Deploy a local database to test against, dropping the existing
        var createDatabaseQuery = SqlQueryStrings.DropAndCreateDatabase(databaseName);
        using var masterDatabase = new Database("Data Source=localhost;Initial Catalog=master;Encrypt=False;Integrated Security=SSPI;", "Microsoft.Data.SqlClient");
        await masterDatabase.ExecuteAsync(createDatabaseQuery);
        sut = new LocalTestDatabaseHelper($"Data Source=localhost;Initial Catalog={databaseName};Encrypt=False;Integrated Security=SSPI;", databaseName);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        //Drop the database to clean up
        var dropDatabaseQuery = SqlQueryStrings.DropDatabaseSql(databaseName);
        using var masterDatabase = new Database("Data Source=localhost;Initial Catalog=master;Encrypt=False;Integrated Security=SSPI;", "Microsoft.Data.SqlClient");
        await masterDatabase.ExecuteAsync(dropDatabaseQuery);
    }

    [Test]
    public async Task ResetDatabaseAsync_ClearsOrdinaryTable()
    {
        //Arrange
        using var database = new Database(sut.ConnectionString, "Microsoft.Data.SqlClient");
        await database.ExecuteAsync("CREATE TABLE [dbo].[TestTable] ([Id] int NOT NULL PRIMARY KEY, [Name] nvarchar(50) NOT NULL);");
        await database.InsertAsync(new TestTablePoco(1, "SomeName"));
        await database.InsertAsync(new TestTablePoco(2, "SomeOtherName"));
        var result = await database.FetchAsync<TestTablePoco>();
        Assert.That(result, Has.Count.EqualTo(2));

        //Act
        await sut.ResetDatabaseAsync();

        //Assert
        result = await database.FetchAsync<TestTablePoco>();
        Assert.That(result, Is.Empty);
    }

    //PetaPoco attributes for tablename and primary key
    [TableName("TestTable")]
    [PrimaryKey("Id", AutoIncrement=false)]
    private class TestTablePoco
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public TestTablePoco(int id, string name)
        {
            Id = id;
            Name = name;
        }

        //Disable nullability for petapocos reflection constructor
        #nullable disable
        private TestTablePoco() //Used for petapoco
        {
        }
        #nullable enable
    }
}
