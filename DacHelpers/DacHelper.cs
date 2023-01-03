using DacHelpers.DockerHelpers;
using DacHelpers.Helper;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace DacHelpers;

/// <summary>
/// Helper class for deploying dacpacs for use in tests
/// </summary>
public static class DacHelper
{
    /// <summary>
    /// Deploys a DACPAC to localhost
    /// </summary>
    /// <param name="dacpacPath">Path to the DACPAC</param>
    /// <param name="databaseName">Name of the database to deploy to</param>
    public static async Task<ITestDatabaseHelper> DropAndDeployLocalAsync(string dacpacPath, string databaseName)
    {
        return await DropAndDeployLocalAsync(dacpacPath, databaseName, new Dictionary<string, string>());
    }

    /// <summary>
    /// Deploys a DACPAC to localhost
    /// </summary>
    /// <param name="dacpacPath">Path to the DACPAC</param>
    /// <param name="databaseName">Name of the database to deploy to</param>
    /// <param name="sqlCmdVariables">SQLCMD variables to pass to the DACPAC</param>
    public static async Task<ITestDatabaseHelper> DropAndDeployLocalAsync(string dacpacPath, string databaseName, Dictionary<string, string> sqlCmdVariables)
    {
        var connectionStringMaster = GetConnectionStringLocal("master"); //Connect to master since database might not yet exist
        await DropAndCreateDatabaseAsync(connectionStringMaster, databaseName);
        DeployDacpac(dacpacPath, databaseName, sqlCmdVariables, connectionStringMaster);

        return new TestDatabaseHelper(GetConnectionStringLocal(databaseName), databaseName);
    }

    /// <summary>
    /// Creates a local docker container and deploys a dacpac there
    /// </summary>
    /// <param name="dacpacPath">Path to the DACPAC</param>
    /// <param name="databaseName">Name of the database to deploy to</param>
    public static async Task<ITestDatabaseHelper> DeployDockerAsync(string dacpacPath, string databaseName)
    {
        return await DeployDockerAsync(dacpacPath, databaseName, new Dictionary<string, string>());
    }

    /// <summary>
    /// Creates a local docker container and deploys a dacpac there
    /// </summary>
    /// <param name="dacpacPath">Path to the DACPAC</param>
    /// <param name="databaseName">Name of the database to deploy to</param>
    /// <param name="sqlCmdVariables">SQLCMD variables to pass to the DACPAC</param>
    public static async Task<ITestDatabaseHelper> DeployDockerAsync(string dacpacPath, string databaseName, Dictionary<string, string> sqlCmdVariables)
    {
        var (status, container) = await SqlDockerHandler.RunDockerSqlContainerAsync($"DacHelper{databaseName}");

        if (!status.IsSuccess || container == null)
        {
            throw new Exception($"Could not start docker container. Status: {status.Error}");
        }

        var connectionStringMaster = container.GetConnectionString("master");

        await DropAndCreateDatabaseAsync(connectionStringMaster, databaseName);

        DeployDacpac(dacpacPath, databaseName, sqlCmdVariables, connectionStringMaster);

        return new DockerTestDatabaseHelper(container, databaseName);
    }

    /// <summary>
    /// Deploys dacpac to existing server given a ConnectionString
    /// </summary>
    /// <param name="dacpacPath">Path to the DACPAC</param>
    /// <param name="databaseName">Name of the database to deploy to</param>
    /// <param name="connectionString">ConnectionString to the server.</param>
    public static async Task<ITestDatabaseHelper> DropAndDeployAsync(string dacpacPath, string databaseName, string connectionString)
    {
        return await DropAndDeployAsync(dacpacPath, databaseName, connectionString, new Dictionary<string, string>());
    }

    /// <summary>
    /// Deploys dacpac to existing server given a ConnectionString
    /// </summary>
    /// <param name="dacpacPath">Path to the DACPAC</param>
    /// <param name="databaseName">Name of the database to deploy to</param>
    /// <param name="connectionString">ConnectionString to the server.</param>
    /// <param name="sqlCmdVariables">SQLCMD variables to pass to the DACPAC</param>
    public static async Task<ITestDatabaseHelper> DropAndDeployAsync(string dacpacPath, string databaseName, string connectionString, Dictionary<string, string> sqlCmdVariables)
    {
        await DropAndCreateDatabaseAsync(connectionString, databaseName);

        DeployDacpac(dacpacPath, databaseName, sqlCmdVariables, connectionString);

        //Create new connectionstring that points to the new database by setting InitialCatalog
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = databaseName
        };
        return new TestDatabaseHelper(builder.ConnectionString, databaseName);
    }

    /// <summary>
    /// Deploys changescripts to an existing database given a ConnectionString
    /// </summary>
    /// <param name="connectionString">ConnectionString to the server. Must include </param>
    /// <param name="changeScripts">List of paths for changescripts to load</param>
    public static async Task DeployChangeScriptsAsync(string connectionString, IEnumerable<string> changeScripts)
    {
        //Create and open sql connection
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        //Load .sql scripts from paths given in changescripts - then execute them on connection
        foreach (var changeScript in changeScripts)
        {
            var script = File.ReadAllText(changeScript);
            using var command = new SqlCommand(script, connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    /// <summary>
    /// Deploys a folder of changescripts (in alphabetical order) to an existing database given a ConnectionString
    /// </summary>
    /// <param name="connectionString">ConnectionString to the server. Must include </param>
    /// <param name="changeScriptFolder">Folder with changescripts to load</param>
    public static async Task DeployChangeScriptsAsync(string connectionString, string changeScriptFolder)
    {
        var changeScripts = EnumerateSqlFiles(changeScriptFolder);
        await DeployChangeScriptsAsync(connectionString, changeScripts);
    }

    /// <summary>
    /// Deploys changescripts to a database on localhost
    /// </summary>
    /// <param name="databaseName">Name of the database to deploy to</param>
    /// <param name="changeScripts">List of paths for changescripts to load</param>
    public static async Task DeployChangeScriptsLocalAsync(string databaseName, IEnumerable<string> changeScripts)
    {
        await DeployChangeScriptsAsync(GetConnectionStringLocal(databaseName), changeScripts);
    }

    /// <summary>
    /// Deploys changescripts to a database on localhost
    /// </summary>
    /// <param name="databaseName">Name of the database to deploy to</param>
    /// <param name="changeScriptFolder">Folder with changescripts to load</param>
    public static async Task DeployChangeScriptsLocalAsync(string databaseName, string changeScriptFolder)
    {
        var changeScripts = EnumerateSqlFiles(changeScriptFolder);
        await DeployChangeScriptsLocalAsync(databaseName, changeScripts);
    }

    /// <summary>
    /// Drops and recreates a database on localhost, then deploys changescripts to it
    /// </summary>
    /// <param name="databaseName">Name of the database to deploy to</param>
    /// <param name="changeScripts">List of paths for changescripts to load</param>
    public static async Task DropAndDeployChangeScriptsLocalAsync(string databaseName, IEnumerable<string> changeScripts)
    {
        var connectionStringMaster = GetConnectionStringLocal("master"); //Connect to master since database might not yet exist
        await DropAndCreateDatabaseAsync(connectionStringMaster, databaseName);
        await DeployChangeScriptsAsync(GetConnectionStringLocal(databaseName), changeScripts);
    }

    /// <summary>
    /// Drops and recreates a database on localhost, then deploys changescripts to it
    /// </summary>
    /// <param name="databaseName">Name of the database to deploy to</param>
    /// <param name="changeScriptFolder">Folder with changescripts to load</param>
    public static async Task DropAndDeployChangeScriptsLocalAsync(string databaseName, string changeScriptFolder)
    {
        var changeScripts = EnumerateSqlFiles(changeScriptFolder);
        await DropAndDeployChangeScriptsLocalAsync(databaseName, changeScripts);
    }

    private static void DeployDacpac(string dacpacPath, string databaseName, Dictionary<string, string> sqlCmdVariables, string connectionStringServer)
    {
        var dacOptions = new DacDeployOptions
        {
            BlockOnPossibleDataLoss = false //This is for tests
        };

        foreach (var keyValuePair in sqlCmdVariables)
        {
            dacOptions.SqlCommandVariableValues.Add(keyValuePair);
        }

        var dacServiceInstance = new DacServices(connectionStringServer);
        //Could hook up here to dacServiceInstance.ProgressChanged and .Message to get progress updates but not sure where to log them
        try
        {
            using DacPackage dacpac = DacPackage.Load(dacpacPath);
            dacServiceInstance.Deploy(dacpac, databaseName
                                    , upgradeExisting: true
                                    , options: dacOptions
                                    );
        }
        catch (Exception)
        {
            //How do we handle or log these?
            throw;
        }
    }

    private static IOrderedEnumerable<string> EnumerateSqlFiles(string changeScriptFolder)
    {
        //Enumerate the .sql files in the given folder alphabetically
        return Directory.EnumerateFiles(changeScriptFolder, "*.sql", SearchOption.TopDirectoryOnly).OrderBy(x => x);
    }

    private static async Task DropAndCreateDatabaseAsync(string connectionString, string databaseName)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        var command = connection.CreateCommand();

        command.CommandText = SqlQueryStrings.DropAndCreateDatabase(databaseName);
        await command.ExecuteNonQueryAsync();
    }

    private static string GetConnectionStringLocal(string dbName)
    {
        return $"Data Source=localhost;Initial Catalog={dbName};Encrypt=False;Integrated Security=SSPI;";
    }
}
