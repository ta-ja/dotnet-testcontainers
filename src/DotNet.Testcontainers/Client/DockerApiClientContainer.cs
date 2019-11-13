namespace DotNet.Testcontainers.Client
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Docker.DotNet.Models;

  /// <summary>
  /// This class represents a Docker API client and interacts with Docker containers.
  /// </summary>
  internal sealed class DockerApiClientContainer : AbstractContainerImageClient<ContainerListResponse>
  {
    public DockerApiClientContainer() : this(DockerApiEndpoint.LocalEndpoint)
    {
    }

    public DockerApiClientContainer(Uri endpoint) : base(endpoint)
    {
    }

    public override async Task<IReadOnlyCollection<ContainerListResponse>> GetAllAsync()
    {
      return (await this.Docker.Containers.ListContainersAsync(new ContainersListParameters { All = true })).ToList();
    }

    public override async Task<ContainerListResponse> ByIdAsync(string id)
    {
      return await this.ByPropertyAsync("id", id);
    }

    public override async Task<ContainerListResponse> ByNameAsync(string name)
    {
      return await this.ByPropertyAsync("name", name);
    }

    public override async Task<ContainerListResponse> ByPropertyAsync(string property, string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        return null;
      }

      var response = this.Docker.Containers.ListContainersAsync(new ContainersListParameters
      {
        All = true,
        Filters = new Dictionary<string, IDictionary<string, bool>>
        {
          {
            property, new Dictionary<string, bool>
            {
              { value, true },
            }
          },
        },
      });

      return (await response).FirstOrDefault();
    }

    internal async Task<long> GetExitCode(string id)
    {
      return (await this.Docker.Containers.WaitContainerAsync(id)).StatusCode;
    }
  }
}
