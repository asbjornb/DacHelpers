using DacHelpers.DeployReporting;

namespace DacHelpers.Test.DeployReporting;

internal class DeployReportShouldParseXml
{
    const string xml1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DeploymentReport xmlns=""http://schemas.microsoft.com/sqlserver/dac/DeployReport/2012/02"">
	<Alerts>
		<Alert Name=""CreateClusteredIndex"">
			<Issue Value=""[dbo].[PK_Table2] on [dbo].[Table2]"" />
		</Alert>
	</Alerts>
	<Operations>
		<Operation Name=""Alter"">
			<Item Value=""[dbo].[Table2]"" Type=""SqlTable"" />
		</Operation>
		<Operation Name=""Create"">
			<Item Value=""[dbo].[PK_Table2]"" Type=""SqlPrimaryKeyConstraint"" />
		</Operation>
	</Operations>
</DeploymentReport>";

    const string xml2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DeploymentReport xmlns=""http://schemas.microsoft.com/sqlserver/dac/DeployReport/2012/02"">
    <Alerts>
        <Alert Name=""DataIssue"">
            <Issue Value=""The table [dbo].[test_table] is being dropped, data loss could occur."" Id=""1"" />
        </Alert>
    </Alerts>
    <Operations>
        <Operation Name=""Drop"">
            <Item Value=""[dbo].[test_table]"" Type=""SqlTable"">
                <Issue Id=""1"" />
            </Item>
        </Operation>
    </Operations>
</DeploymentReport>";

    [Test]
    public void ShouldParseXml1()
    {
        var report = DeployReport.Parse(xml1);

        Assert.Multiple(() =>
        {
            Assert.That(report.Alerts, Has.Count.EqualTo(1));
            Assert.That(report.Alerts, Does.Contain(new Alert("CreateClusteredIndex", "[dbo].[PK_Table2] on [dbo].[Table2]")));
            Assert.That(report.Operations, Has.Count.EqualTo(2));
            Assert.That(report.Operations, Does.Contain(new SqlOperation(SqlOperationType.Alter, "SqlTable", "[dbo].[Table2]")));
            Assert.That(report.Operations, Does.Contain(new SqlOperation(SqlOperationType.Create, "SqlPrimaryKeyConstraint", "[dbo].[PK_Table2]")));
        });
    }

    [Test]
    public void ShouldParseXml2()
    {
        var report = DeployReport.Parse(xml2);

        Assert.Multiple(() =>
        {
            Assert.That(report.Alerts, Has.Count.EqualTo(1));
            Assert.That(report.Alerts, Does.Contain(new Alert("DataIssue", "The table [dbo].[test_table] is being dropped, data loss could occur.")));
            Assert.That(report.Operations, Has.Count.EqualTo(1));
            Assert.That(report.Operations, Does.Contain(new SqlOperation(SqlOperationType.Drop, "SqlTable", "[dbo].[test_table]")));
        });
    }
}
