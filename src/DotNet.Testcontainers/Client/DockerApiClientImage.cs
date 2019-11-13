namespace DotNet.Testcontainers.Client
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Docker.DotNet.Models;

  /// <summary>
  /// This class represents a Docker API client and interacts with Docker images.
  /// </summary>
  internal sealed class DockerApiClientImage : AbstractContainerImageClient<ImagesListResponse>
  {
    public DockerApiClientImage() : this(DockerApiEndpoint.LocalEndpoint)
    {
    }

    public DockerApiClientImage(Uri endpoint) : base(endpoint)
    {
    }

    public override async Task<IReadOnlyCollection<ImagesListResponse>> GetAllAsync()
    {
      return (await this.Docker.Images.ListImagesAsync(new ImagesListParameters { All = true })).ToList();
    }

    public override async Task<ImagesListResponse> ByIdAsync(string id)
    {
      return (await this.GetAllAsync()).FirstOrDefault(image => image.ID.Equals(id));
    }

    public override async Task<ImagesListResponse> ByNameAsync(string name)
    {
      return await this.ByPropertyAsync("label", name);
    }

    public override async Task<ImagesListResponse> ByPropertyAsync(string property, string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        return null;
      }

      var response = this.Docker.Images.ListImagesAsync(new ImagesListParameters
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
  }
}
