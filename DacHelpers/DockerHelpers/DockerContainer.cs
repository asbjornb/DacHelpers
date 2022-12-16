using Docker.DotNet;
using Docker.DotNet.Models;

namespace DacHelpers.DockerHelpers;

internal class DockerContainer
{
    private readonly IDockerClient dockerClient;
    private readonly string containerId;
    private readonly Func<string, string> connectionStringFunc;

    public string GetConnectionString(string databaseName) => connectionStringFunc(databaseName);

    public DockerContainer(IDockerClient dockerClient, string containerId, Func<string, string> connectionStringFunc)
    {
        this.dockerClient = dockerClient;
        this.containerId = containerId;
        this.connectionStringFunc = connectionStringFunc;
    }

    public async Task DisposeAsync()
    {
        await dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters() { WaitBeforeKillSeconds = 2 });
        await dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters() { Force = true });
        dockerClient?.Dispose();
    }
}
