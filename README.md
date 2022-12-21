# DacHelpers

Wrapper for Microsoft.SqlServer.Dac and Docker.DotNet to simplify integration testing against databases with DacPacs.

## Usage

To deploy a DacPac to localhost or to local docker simply supply a path and a name. Note that it drops the existing database first.

```c#
var dacpacPath = "TestDb.dacpac";
var databaseName = "TestDb";
var testDatabaseHelper = await DacHelper.DropAndDeployLocalAsync(dacpacPath, databaseName);
//Or
var testDatabaseHelper = await DacHelper.DeployDockerAsync(dacpacPath, databaseName);
```

The databaseTestHelper has a few helper functions to aid tests like supplying connectionstrings, resetting the database or cleaning up after use:

```c#
var connectionString = testDatabaseHelper.GetConnectionString();
await testDatabaseHelper.ResetDatabaseAsync();
await testDatabaseHelper.CleanUpAsync();
```

If deploying to a docker container or server that already exist just supply a ConnectionString. Note though that the returned testDatabaseHelper can't be relied on then to dispose a container':

```c#
var connectionString = testDatabaseHelper.GetConnectionString();
var testDatabaseHelper = await DacHelper.DeployAsync(dacpacPath, databaseName, connectionString);
```

Specifically for docker you can specify an image if you need to target a specific version of sql server:

```c#
var testDatabaseHelper = await DacHelper.DeployDockerAsync(dacpacPath, databaseName, "mcr.microsoft.com/mssql/server:2019-latest");
```

If using SqlCmd variables in the DacPac you can supply a dictionary with mappings:

```c#
var variableMap = new Dictionary<string, string>() { { "Registration", "Registration" } }
var testDatabaseHelper = DacHelper.DropAndDeployLocalAsync(dacpacPath, databaseName, variableMap);
```

To deploy a list of changescripts instead of a DacPac locally:

```c#
var folder = "changescripts/"
var changescripts = new List<string> = { $"{folder}001-addSchemas.sql", $"{folder}002-addEmployeeTable", $"{folder}003-addOfficeTable" }
await DacHelper.DeployChangescriptsLocalAsync(databaseName, changescripts);
```

Or to simply deploy a folder of changescripts locally (in alphabetical order):

```c#
var folder = "changescripts/"
await DacHelper.DeployChangescriptsLocalAsync(databaseName, folder);
```

Testing that a list of changescripts match a DacPac is as simple as:

```c#
var changes = await DacHelper.CompareLocalAsync(dacpacPath, changescriptFolder);
Assert.That(changes, Has.Count.EqualTo(0));
```
