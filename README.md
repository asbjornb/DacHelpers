# DacHelpers

Wrapper for Microsoft.SqlServer.Dac to simplify verifying and testing dacpacs.

## Usage

To deploy a dacpac simply supply a path, a connectionstring and a name:

```c#
var dacpacPath = "TestDb.dacpac";
var databaseName = "TestDb";
var connectionString = "Data Source=(local); " +
            "Integrated Security=true; " +
            $"Initial Catalog=master};";
DacHelper.Deploy(dacpacPath, connectionString, databaseName);
```

To drop the existing database simply use DropAndDeploy instead:

```c#
DacHelper.DropAndDeploy(dacpacPath, connectionString, databaseName);
```

To simplify even further a docker container can be set up and the database deployed there or alternatively a database deployed to localhost:

```c#
var connectionString = DacHelper.DeployNewDocker(dacpacPath, databaseName);
var connectionString = DacHelper.DropAndDeployLocal(dacpacPath, databaseName);
```

If using sqlcmd variables in the dacpac you can supply a dictionary with mappings:

```c#
var variableMap = new Dictionary<string, string>() { { "Registration", "Registration" } }
var connectionString = DacHelper.DeployNewDocker(dacpacPath, databaseName, variableMap);
```

To let you inspect databases after use they are not dropped by default. You can drop the database manually if you want to clean up after use:

```c#
DacHelper.DropDatabase(connectionString, databaseName);
```

To check drift:

```c#
var changes = DacHelper.Compare(dacpacPath, readOnlyConnectionString, databaseName);
```

To deploy a list of changescripts:

```c#
var folder = "changescripts/"
var changescripts = new List<string> = { $"{folder}001-addSchemas.sql", $"{folder}002-addEmployeeTable", $"{folder}003-addOfficeTable" }
DacHelper.DeployChangescripts(changescripts, connectionString, databaseName);
```

Or simply to deploy a folder of changescripts (in alphabetical order):

```c#
var folder = "changescripts/"
DacHelper.DeployChangescripts(folder, connectionString, databaseName);
```

Testing that changescripts match dacpac is then as simple as:

```c#
DacHelper.DeployChangescripts(folder, connectionString, databaseName);
var changes = DacHelper.Compare(dacpacPath, readOnlyConnectionString, databaseName);
Assert.That(changes, Has.Count.EqualTo(0));
```
