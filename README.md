# DacHelpers

Wrapper for Microsoft.SqlServer.Dac to simplify verifying dacpacs and testing with them.

## Usage

To deploy a dacpac to localhost or to local docker simply supply a path and a name:

```c#
var dacpacPath = "TestDb.dacpac";
var databaseName = "TestDb";
string connectionstring;
var disposableLocal = await DacHelper.DeployLocal(dacpacPath, databaseName, out connectionstring);
//Or
var disposableDocker = await DacHelper.DeployDocker(dacpacPath, databaseName, out connectionstring);
```

You get the connectionstring out for use in tests and also get a disposeable that you can use to clean up when finished testing. This allows the option to not dispose in order to be able to manually inspect the database after use. To clean up just dispose:

```c#
disposableLocal.Dispose();
//Or
disposableDocker.Dispose();
```

To drop the existing database before deployment simply use DropAndDeploy instead:

```c#
var disposableLocal = await DacHelper.DropAndDeployLocal(dacpacPath, databaseName, out connectionstring);
var disposableDocker = await DacHelper.DropAndDeployDocker(dacpacPath, databaseName, out connectionstring);
```

If using sqlcmd variables in the dacpac you can supply a dictionary with mappings:

```c#
var variableMap = new Dictionary<string, string>() { { "Registration", "Registration" } }
var disposableDocker = DacHelper.DeployDocker(dacpacPath, databaseName, out connectionstring, variableMap);
```

To check drift or changes from a dacpac vs an actual database supply a connectionstring:

```c#
var connectionString = "Data Source=(local); " +
            "Integrated Security=true; " +
            $"Initial Catalog=master};";
var changes = DacHelper.Compare(dacpacPath, databaseName, readOnlyConnectionString);
```

To deploy a list of changescripts locally:

```c#
var folder = "changescripts/"
var changescripts = new List<string> = { $"{folder}001-addSchemas.sql", $"{folder}002-addEmployeeTable", $"{folder}003-addOfficeTable" }
DacHelper.DeployChangescriptsLocal(databaseName, changescripts);
```

Or simply to deploy a folder of changescripts locally (in alphabetical order):

```c#
var folder = "changescripts/"
DacHelper.DeployChangescriptsLocal(databaseName, folder);
```

Testing that changescripts match dacpac is then as simple as:

```c#
DacHelper.DeployChangescriptsLocal(databaseName, folder);
var changes = DacHelper.CompareLocal(dacpacPath, databaseName);
Assert.That(changes, Has.Count.EqualTo(0));
```
