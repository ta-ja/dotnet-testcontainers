namespace DotNet.Testcontainers.Client
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Docker.DotNet;

  /// <summary>
  /// This class represents a Docker API client.
  /// </summary>
  internal abstract class DockerApiClient
  {
    private static readonly IDictionary<Uri, DockerClient> DockerClients = new Dictionary<Uri, DockerClient>();

    protected DockerApiClient(Uri endpoint)
    {
      if (!DockerClients.TryGetValue(endpoint, out var dockerClient))
      {
        dockerClient = new DockerClientConfiguration(endpoint).CreateClient();
        DockerClients.Add(endpoint, dockerClient);
      }

      this.Docker = dockerClient;
    }

    protected DockerClient Docker { get; }

    public async Task<bool> GetIsWindowsEngineEnabled()
    {
      return (await this.Docker.System.GetSystemInfoAsync()).OperatingSystem.Contains("Windows");
    }
  }
}
