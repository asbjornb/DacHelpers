# DacHelpers

Wrapper for Microsoft.SqlServer.Dac and Docker.DotNet to simplify integration testing against databases with DacPacs.

## Usage

To deploy a DacPac to localhost or to local docker simply supply a path and a name:

```c#
var dacpacPath = "TestDb.dacpac";
var databaseName = "TestDb";
var testDatabaseHelper = await DacHelper.DeployLocal(dacpacPath, databaseName);
//Or
var testDatabaseHelper = await DacHelper.DeployDocker(dacpacPath, databaseName);
```

The databaseTestHelper has a few helper functions to aid tests like supplying connectionstrings, resetting the database or cleaning up after use:

```c#
var connectionString = testDatabaseHelper.GetConnectionString();
await testDatabaseHelper.ResetDatabase();
await testDatabaseHelper.CleanUp();
```

To drop an existing database before deployment simply use DropAndDeploy instead:

```c#
var testDatabaseHelper = await DacHelper.DropAndDeployLocal(dacpacPath, databaseName);
```

If using sqlcmd variables in the dacpac you can supply a dictionary with mappings:

```c#
var variableMap = new Dictionary<string, string>() { { "Registration", "Registration" } }
var testDatabaseHelper = DacHelper.DeployDocker(dacpacPath, databaseName, variableMap);
```

To deploy a list of changescripts instead of a DacPac locally:

```c#
var folder = "changescripts/"
var changescripts = new List<string> = { $"{folder}001-addSchemas.sql", $"{folder}002-addEmployeeTable", $"{folder}003-addOfficeTable" }
DacHelper.DeployChangescriptsLocal(databaseName, changescripts);
```

Or to simply deploy a folder of changescripts locally (in alphabetical order):

```c#
var folder = "changescripts/"
DacHelper.DeployChangescriptsLocal(databaseName, folder);
```

Testing that a list of changescripts match a dacpac as simple as:

```c#
var changes = DacHelper.CompareLocal(dacpacPath, changescriptFolder);
Assert.That(changes, Has.Count.EqualTo(0));
```
