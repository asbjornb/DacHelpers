using DacHelpers.DeployReporting;

namespace DacHelpers.Test.DeployReporting;

[TestFixture, Parallelizable(ParallelScope.Self)]
internal class ShouldCompare
{
    [Test]
    public async Task ShouldCompareChangescriptsToDacpac()
    {
        var report = await DacHelper.CompareLocalAsync("TestAssets/Changescripts/", "TestDatabase.dacpac");

        Assert.Multiple(() =>
        {
            Assert.That(report.Alerts, Has.Count.EqualTo(1));
            Assert.That(report.Alerts.ConvertAll(x => x.AlertType), Does.Contain("DataIssue"));
            Assert.That(report.Operations, Has.Count.EqualTo(2));
            Assert.That(report.Operations, Does.Contain(new SqlOperation(SqlOperationType.Drop, "SqlTable", "[dbo].[TestTable]")));
            Assert.That(report.Operations, Does.Contain(new SqlOperation(SqlOperationType.Create, "SqlTable", "[dbo].[Table1]")));
        });
    }
}
