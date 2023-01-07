using System.Xml;

namespace DacHelpers.Test.ExplorationTests;

[TestFixture, Parallelizable(ParallelScope.Self)]
public class XmlParsing
{
    const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
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

    [Test]
    public void ShouldParse()
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);
        var root = doc.DocumentElement!;

        var operations = new List<string>();

        foreach (XmlElement element in root.GetElementsByTagName("Operation"))
        {
            var operationName = element.GetAttribute("Name");
            foreach (XmlElement item in element.GetElementsByTagName("Item"))
            {
                var objectName = item.GetAttribute("Value");
                var objectType = item.GetAttribute("Type");
                operations.Add($"{operationName} type: <{objectType}> with name <{objectName}>");
            }
        }
        //Assert
        Assert.That(operations, Is.EquivalentTo(new[]
        {
            "Alter type: <SqlTable> with name <[dbo].[Table2]>",
            "Create type: <SqlPrimaryKeyConstraint> with name <[dbo].[PK_Table2]>"
        }));
    }
}
