using DacHelpers.Helper;
using Microsoft.Build.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace DacHelpers;

public static class DacHelper
{
    /// <summary>
    /// Deploys a DACPAC at the given path to localhost at the given database name
    /// </summary>
    /// <param name="dacpacPath">Path to the DACPAC</param>
    /// <param name="databaseName">Name of the database to deploy to</param>
    public static async Task<ITestDatabaseHelper> DropAndDeployLocal(string dacpacPath, string databaseName)
    {
        return await DropAndDeployLocal(dacpacPath, databaseName, new Dictionary<string, string>());
    }

    /// <summary>
    /// Deploys a DACPAC at the given path to localhost at the given database name
    /// </summary>
    /// <param name="dacpacPath">Path to the DACPAC</param>
    /// <param name="databaseName">Name of the database to deploy to</param>
    /// <param name="sqlCmdVariables">SQLCMD variables to pass to the DACPAC</param>
    public static async Task<ITestDatabaseHelper> DropAndDeployLocal(string dacpacPath, string databaseName, Dictionary<string, string> sqlCmdVariables)
    {
        var connectionString = GetConnectionStringLocal(databaseName);
        await DropAndCreateDatabase(connectionString, databaseName);

        var dacOptions = new DacDeployOptions
        {
            BlockOnPossibleDataLoss = false
        };

        foreach (var keyValuePair in sqlCmdVariables)
        {
            dacOptions.SqlCommandVariableValues.Add(keyValuePair);
        }

        var dacServiceInstance = new DacServices(connectionString);
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

        return new LocalTestDatabaseHelper(connectionString, databaseName);
    }

    private static async System.Threading.Tasks.Task DropAndCreateDatabase(string connectionString, string databaseName)
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
