using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DacHelpers.DockerHelpers;

internal static class SqlDockerHandler
{
    private const string containerImage = "mcr.microsoft.com/mssql/server";
    private const string SqlPort = "1433";
    private const string SAPassword = "Pa$$word";

    private static string ConnectionString(string databaseName, int hostPort) => $"Data Source=localhost,{hostPort};Initial Catalog={databaseName};User Id=sa;Password={SAPassword};TrustServerCertificate=True;";

    public static async Task<(Status, DockerContainer?)> RunDockerSqlContainerAsync(string containername, string containerImageTag = "2019-latest")
    {
        var hostPort = GetFirstFreePort();
        return await RunDockerSqlContainerAsync(containername, hostPort, containerImageTag);
    }

    public static async Task<(Status,DockerContainer?)> RunDockerSqlContainerAsync(string containerName, int hostPort, string containerImageTag = "2019-latest")
    {
        var dockerClient = CreateDockerClient();
        var imageParameters = new ImagesCreateParameters()
        {
            FromImage = containerImage,
            Tag = containerImageTag,
        };

        await dockerClient.Images.CreateImageAsync(imageParameters, new AuthConfig(), new Progress<JSONMessage>()).ConfigureAwait(false);

        var createContainerParameters = GetContainerParams(containerName, containerImage, containerImageTag, hostPort);

        var containerResponse = await dockerClient.Containers.CreateContainerAsync(createContainerParameters).ConfigureAwait(false);

        var started = await dockerClient.Containers.StartContainerAsync(containerResponse.ID, null).ConfigureAwait(false);

        if (!started)
        {
            await DisposeContainerAndClientAsync(dockerClient, containerResponse.ID);
            return (Status.Faillure("Failed to start container"), null);
        }

        var ready = await WaitTillReady(dockerClient, containerResponse.ID, ConnectionString("master", hostPort));

        if (!ready.IsSuccess)
        {
            await DisposeContainerAndClientAsync(dockerClient, containerResponse.ID);
            return (Status.Faillure("Failed to connect to SQL-database in container"), null);
        }

        var dockerContainer = new DockerContainer(dockerClient, containerResponse.ID, (databaseName) => ConnectionString(databaseName, hostPort));
        return (Status.Success(), dockerContainer);
    }

    public static async Task<Status> StopAndRemoveContainerAsync(string containerName)
    {
        using var dockerClient = CreateDockerClient();
        try
        {
            var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters() { All = true, Limit = 1, Filters = new Dictionary<string, IDictionary<string, bool>> { { "name", new Dictionary<string, bool> { { containerName, true } } } } });
            if (containers.Count == 0)
            {
                return Status.Success();
            }
            else
            {
                var containerId = containers[0].ID;
                await dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters() { WaitBeforeKillSeconds = 2 });
                await dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters() { Force = true });
                return Status.Success();
            }
        }
        catch
        {
            return Status.Faillure("Failed to stop and remove container");
        }
    }

    private static CreateContainerParameters GetContainerParams(string containerName, string containerImage, string containerImageTag, int hostPort)
    {
        var createContainerParameters = new CreateContainerParameters()
        {
            Name = containerName,
            Image = $"{containerImage}:{containerImageTag}",
            Env = new List<string>() { "ACCEPT_EULA=Y", $"SA_PASSWORD={SAPassword}" },
        };
        //Map ports
        IDictionary<string, IList<PortBinding>> portBindingsDictionary = new Dictionary<string, IList<PortBinding>>()
            { { SqlPort, new List<PortBinding>() { new PortBinding() { HostPort = hostPort.ToString() } } } };
        createContainerParameters.ExposedPorts = new Dictionary<string, EmptyStruct>() { { SqlPort, default } };
        createContainerParameters.HostConfig = new HostConfig() { PortBindings = portBindingsDictionary, PublishAllPorts = true };
        return createContainerParameters;
    }

    private static async Task<Status> WaitTillReady(IDockerClient dockerClient, string containerId, string connectionStringMaster)
    {
        //Wait until the container and sql server is ready
        var attempts = 0;
        const int maxAttempts = 10;
        var containerReady = false;
        var sqlReady = false;
        while (!sqlReady && attempts < maxAttempts)
        {
            await Task.Delay(1000);
            if (!containerReady)
            {
                try
                {
                    var container = await dockerClient.Containers.InspectContainerAsync(containerId).ConfigureAwait(false);
                    if (container.State.Running)
                    {
                        containerReady = true;
                    }
                }
                catch (Exception)
                {
                    // Just ignore and retry on next pass
                }
            }
            if (containerReady)
            {
                try
                {
                    //Check that we can connect to database
                    using var connection = new SqlConnection(connectionStringMaster);
                    connection.Open();
                    var command = connection.CreateCommand();

                    command.CommandText = "SELECT @@Version";
                    string? version = (string?)await command.ExecuteScalarAsync();
                    if (version?.Contains("Microsoft SQL Server") == true)
                    {
                        sqlReady = true;
                    }
                }
                catch (Exception)
                {
                    // Just ignore and retry on next pass
                }
            }
            attempts++;
        }

        if (!containerReady)
        {
            return Status.Faillure("Failed to inspect container");
        }
        if (!sqlReady)
        {
            return Status.Faillure("Container started but failed to connect to Sql Server");
        }
        return Status.Success();
    }

    private static IDockerClient CreateDockerClient()
    {
        var uri = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Uri("npipe://./pipe/docker_engine") : new Uri("unix:/var/run/docker.sock");
        return new DockerClientConfiguration(uri).CreateClient();
    }

    private static int GetFirstFreePort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var freePort = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return freePort;
    }

    private static async Task DisposeContainerAndClientAsync(IDockerClient dockerClient, string containerId)
    {
        if (containerId != null)
        {
            await dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters() { WaitBeforeKillSeconds = 2 });
            await dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters() { Force = true });
        }
        dockerClient?.Dispose();
    }
}
