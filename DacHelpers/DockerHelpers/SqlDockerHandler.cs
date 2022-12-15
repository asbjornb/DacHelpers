using Docker.DotNet;
using Docker.DotNet.Models;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Data.SqlClient;

namespace DacHelpers.DockerHelpers;

internal class SqlDockerHandler
{
    private const string containerImage = "mcr.microsoft.com/mssql/server";
    private const string SqlPort = "1433";

    private readonly IDockerClient dockerClient;
    private readonly int hostPort;
    private readonly string containerName;

    private string? containerId;
    private bool isInitialised;

    public string ConnectionString(string databaseName) => $"Data Source=localhost,{hostPort};Initial Catalog={databaseName};User Id=sa;Password=Pa$$word;";

    /// <summary>Helps manage a SqlDockerContainer</summary>
    /// <param name="containerName">Name for the container, can be empty for random name</param>
    /// <param name="hostPort">Port from the host mapped to 1433 on the container</param>
    public SqlDockerHandler(string containerName, int hostPort)
    {
        dockerClient = CreateDockerClient();
        this.hostPort = hostPort;
        this.containerName = containerName;
        isInitialised = false;
    }

    /// <summary>Helps manage a SqlDockerContainer</summary>
    /// <param name="containerName">Name for the container, can be empty for random name</param>
    public SqlDockerHandler(string containerName)
    {
        dockerClient = CreateDockerClient();
        hostPort = GetFirstFreePort();
        this.containerName = containerName;
        isInitialised = false;
    }

    /// <summary>Builds and runs a docker container. Defaults to use 2019-latest tag but can be targetted to a specific version</summary>
    /// <param name="containerImageTag">Tag used to select version of sql server</param>
    /// <returns>Returns bool specifying if it was successful</returns>
    public async Task<Status> RunDockerSqlContainerAsync(string containerImageTag = "2019-latest")
    {
        var imageParameters = new ImagesCreateParameters()
        {
            FromImage = containerImage,
            Tag = containerImageTag,
        };

        await dockerClient.Images.CreateImageAsync(imageParameters, new AuthConfig(), new Progress<JSONMessage>()).ConfigureAwait(false);

        var createContainerParameters = new CreateContainerParameters()
        {
            Name = containerName,
            Image = $"{containerImage}:{containerImageTag}",
        };

        ConfigPorts(createContainerParameters);

        var containerResponse = await dockerClient.Containers.CreateContainerAsync(createContainerParameters).ConfigureAwait(false);

        var started = await dockerClient.Containers.StartContainerAsync(containerResponse.ID, null).ConfigureAwait(false);

        if (!started)
        {
            return Status.Faillure("Failed to start container");
        }

        var ready = await WaitTillReady(containerResponse.ID);

        if (!ready.IsSuccess)
        {
            return Status.Faillure("Failed to connect to SQL-database in container");
        }

        containerId = containerResponse.ID;
        isInitialised = true;
        return Status.Success();
    }

    public async Task DisposeAsync()
    {
        if (isInitialised)
        {
            await CleanUpContainerAsync();
        }
        dockerClient?.Dispose();
    }

    public async Task CleanUpContainerAsync()
    {
        await dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters() { WaitBeforeKillSeconds = 2 }).ConfigureAwait(false);
        await dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters() { Force = true }).ConfigureAwait(false);
        isInitialised = false;
    }

    private void ConfigPorts(CreateContainerParameters createContainerParameters)
    {
        //Map ports
        createContainerParameters.ExposedPorts = new Dictionary<string, EmptyStruct>() { { SqlPort, default } };
        IDictionary<string, IList<PortBinding>> portBindingsDictionary = new Dictionary<string, IList<PortBinding>>()
            { { SqlPort, new List<PortBinding>() { new PortBinding() { HostPort = hostPort.ToString() } } } };
        createContainerParameters.HostConfig = new HostConfig() { PortBindings = portBindingsDictionary, PublishAllPorts = true };
    }

    private async Task<Status> WaitTillReady(string containerId)
    {
        //Wait until the container and sql server is ready
        var attempts = 0;
        const int maxAttempts = 10;
        var containerReady = false;
        var sqlReady = false;
        while (!containerReady && !sqlReady && attempts < maxAttempts)
        {
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
                    using var connection = new SqlConnection(ConnectionString("master"));
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
            await Task.Delay(1000).ConfigureAwait(false);
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
}
