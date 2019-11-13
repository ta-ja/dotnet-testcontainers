namespace DotNet.Testcontainers.Client
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Threading;
  using System.Threading.Tasks;
  using Docker.DotNet.Models;
  using DotNet.Testcontainers.Containers.Configurations;
  using DotNet.Testcontainers.Containers.OutputConsumers;
  using DotNet.Testcontainers.Images.Archives;
  using DotNet.Testcontainers.Images.Configurations;
  using DotNet.Testcontainers.Internals.Mappers;
  using DotNet.Testcontainers.Services;

  /// <summary>
  /// This class represents a Docker API client and manages Docker resources.
  /// </summary>
  internal class TestcontainersClient : DockerApiClient, ITestcontainersClient
  {
    static TestcontainersClient()
    {
      AppDomain.CurrentDomain.ProcessExit += (sender, args) => PurgeOrphanedContainers();
      Console.CancelKeyPress += (sender, args) => PurgeOrphanedContainers();
    }

    public TestcontainersClient() : this(DockerApiEndpoint.LocalEndpoint)
    {
    }

    public TestcontainersClient(Uri endpoint) : base(endpoint)
    {
      this.DockerContainerClient = new DockerApiClientContainer(endpoint);
      this.DockerImageClient = new DockerApiClientImage(endpoint);
    }

    public DockerApiClientContainer DockerContainerClient { get; }

    public DockerApiClientImage DockerImageClient { get; }

    private static void PurgeOrphanedContainers()
    {
      var args = string.Join(" ", TestcontainersRegistryService.GetRegisteredContainers());

      if (string.IsNullOrEmpty(args))
      {
        return;
      }

      new Process { StartInfo = { FileName = "docker", Arguments = $"rm --force {args}" } }.Start();
    }

    public async Task StartAsync(string id, CancellationToken ct = default)
    {
      if (await this.DockerContainerClient.ExistsWithIdAsync(id))
      {
        await this.Docker.Containers.StartContainerAsync(id, new ContainerStartParameters(), ct);
      }
    }

    public async Task StopAsync(string id, CancellationToken ct = default)
    {
      if (await this.DockerContainerClient.ExistsWithIdAsync(id))
      {
        await this.Docker.Containers.StopContainerAsync(id, new ContainerStopParameters { WaitBeforeKillSeconds = 15 }, ct);
      }
    }

    public async Task RemoveAsync(string id, CancellationToken ct = default)
    {
      if (await this.DockerContainerClient.ExistsWithIdAsync(id))
      {
        await this.Docker.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters { Force = true }, ct);
      }

      TestcontainersRegistryService.Unregister(id);
    }

    public async Task AttachAsync(string id, IOutputConsumer outputConsumer, CancellationToken ct = default)
    {
      if (outputConsumer is null || outputConsumer is OutputConsumerNull)
      {
        return;
      }

      var attachParameters = new ContainerAttachParameters
      {
        Stdout = true,
        Stderr = true,
        Stream = true,
      };

      var stream = await this.Docker.Containers.AttachContainerAsync(id, false, attachParameters, ct);

      _ = stream.CopyOutputToAsync(Stream.Null, outputConsumer.Stdout, outputConsumer.Stderr, ct);
    }

    public async Task<long> ExecAsync(string id, IList<string> command, CancellationToken ct = default)
    {
      var created = await this.Docker.Containers.ExecCreateContainerAsync(id, new ContainerExecCreateParameters { Cmd = command, }, ct);

      await this.Docker.Containers.StartContainerExecAsync(created.ID, ct);

      for (ContainerExecInspectResponse response; (response = await this.Docker.Containers.InspectContainerExecAsync(created.ID, ct)) != null;)
      {
        if (!response.Running)
        {
          return response.ExitCode;
        }
      }

      return long.MinValue;
    }

    public async Task<string> BuildAsync(ImageFromDockerfileConfiguration config, CancellationToken ct = default)
    {
      var dockerFileArchive = new DockerfileArchive(config.DockerfileDirectory);

      var imageExists = await this.DockerImageClient.ExistsWithNameAsync(config.Image);

      if (imageExists && config.DeleteIfExists)
      {
        await this.Docker.Images.DeleteImageAsync(config.Image, new ImageDeleteParameters { Force = true }, ct);
      }

      using (var stream = new FileStream(dockerFileArchive.Tar(), FileMode.Open))
      {
        using (var builtImage = await this.Docker.Images.BuildImageFromDockerfileAsync(stream, new ImageBuildParameters { Dockerfile = "Dockerfile", Tags = new[] { config.Image } }, ct))
        {
          // New Docker image built, ready to use.
        }
      }

      return config.Image;
    }

    public async Task<string> RunAsync(TestcontainersConfiguration config, CancellationToken ct = default)
    {
      var image = config.Container.Image;

      var imageExistsTask = this.DockerImageClient.ExistsWithNameAsync(image);

      var pullImageTask = Task.CompletedTask;

      if (!await imageExistsTask)
      {
        pullImageTask = this.Docker.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = image }, null, DebugProgress.Instance, ct);
      }

      var name = config.Container.Name;

      var workingDir = config.Container.WorkingDirectory;

      var converter = new TestcontainersConfigurationConverter(config);

      var entrypoint = converter.Entrypoint;

      var cmd = converter.Command;

      var env = converter.Environments;

      var labels = converter.Labels;

      var exposedPorts = converter.ExposedPorts;

      var portBindings = converter.PortBindings;

      var mounts = converter.Mounts;

      var hostConfig = new HostConfig
      {
        PortBindings = portBindings,
        Mounts = mounts,
      };

      var createParameters = new CreateContainerParameters
      {
        Image = image,
        Name = name,
        WorkingDir = workingDir,
        Entrypoint = entrypoint,
        Env = env,
        Labels = labels,
        Cmd = cmd,
        ExposedPorts = exposedPorts,
        HostConfig = hostConfig,
      };

      await pullImageTask;

      var id = (await this.Docker.Containers.CreateContainerAsync(createParameters, ct)).ID;

      TestcontainersRegistryService.Register(id, config.CleanUp);

      return id;
    }
  }
}
