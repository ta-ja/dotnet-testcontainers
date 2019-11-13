namespace DotNet.Testcontainers.Containers.Modules
{
  using System;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Docker.DotNet.Models;
  using DotNet.Testcontainers.Client;
  using DotNet.Testcontainers.Containers.Configurations;
  using DotNet.Testcontainers.Containers.WaitStrategies;

  /// <summary>
  /// This class represents a configured and created Testcontainer.
  /// </summary>
  public class TestcontainersContainer : IDockerContainer
  {
    private const string ContainerIsNotRunning = "Testcontainer is not running.";

    private bool disposed;

    private string id;

    private ContainerListResponse container;

    internal TestcontainersContainer(TestcontainersConfiguration configuration)
    {
      this.TestcontainersClient = new TestcontainersClient(configuration.Endpoint);
      this.TestcontainersConfiguration = configuration;
    }

    ~TestcontainersContainer()
    {
      this.Dispose(false);
    }

    public bool HasId
    {
      get
      {
        return !string.IsNullOrEmpty(this.id);
      }
    }

    public string Id
    {
      get
      {
        return this.id ?? string.Empty;
      }
    }

    public string Name
    {
      get
      {
        if (this.container == null)
        {
          throw new InvalidOperationException(ContainerIsNotRunning);
        }

        return this.container.Names.FirstOrDefault() ?? string.Empty;
      }
    }

    public string IpAddress
    {
      get
      {
        if (this.container == null)
        {
          throw new InvalidOperationException(ContainerIsNotRunning);
        }

        var ipAddress = this.container.NetworkSettings.Networks.FirstOrDefault();
        return ipAddress.Value?.IPAddress ?? string.Empty;
      }
    }

    public string MacAddress
    {
      get
      {
        if (this.container == null)
        {
          throw new InvalidOperationException(ContainerIsNotRunning);
        }

        var macAddress = this.container.NetworkSettings.Networks.FirstOrDefault();
        return macAddress.Value?.MacAddress ?? string.Empty;
      }
    }

    private ITestcontainersClient TestcontainersClient { get; }

    private TestcontainersConfiguration TestcontainersConfiguration { get; }

    public ushort GetMappedPublicPort(int privatePort)
    {
      return this.GetMappedPublicPort($"{privatePort}");
    }

    public ushort GetMappedPublicPort(string privatePort)
    {
      if (this.container == null)
      {
        throw new InvalidOperationException(ContainerIsNotRunning);
      }

      var mappedPort = this.container.Ports.FirstOrDefault(port => $"{port.PrivatePort}".Equals(privatePort));
      return mappedPort?.PublicPort ?? ushort.MinValue;
    }

    public Task<long> GetExitCode()
    {
      return this.TestcontainersClient.DockerContainerClient.GetExitCode(this.Id);
    }

    public async Task StartAsync()
    {
      await this.Create();

      await this.Start();

      this.container = await this.TestcontainersClient.DockerContainerClient.ByIdAsync(this.Id);
    }

    public async Task StopAsync()
    {
      await this.Stop();
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.disposed)
      {
        return;
      }

      var cleanOrStopTask = this.TestcontainersConfiguration.CleanUp ? this.CleanUp() : this.Stop();
      cleanOrStopTask.GetAwaiter().GetResult();

      this.disposed = true;
    }

    private async Task Create()
    {
      if (!this.HasId)
      {
        this.id = await this.TestcontainersClient.RunAsync(this.TestcontainersConfiguration);
      }
    }

    private async Task Start()
    {
      if (this.HasId)
      {
        using (var cts = new CancellationTokenSource())
        {
          var attachConsumerTask = this.TestcontainersClient.AttachAsync(this.Id, this.TestcontainersConfiguration.OutputConsumer, cts.Token);

          var startTask = this.TestcontainersClient.StartAsync(this.Id, cts.Token);

          var waitTask = WaitStrategy.WaitUntil(() => this.TestcontainersConfiguration.WaitStrategy.Until(this.TestcontainersConfiguration.Endpoint, this.Id), ct: cts.Token);

          var handleDockerExceptionTask = startTask.ContinueWith(task =>
          {
            task.Exception?.Handle(exception =>
            {
              cts.Cancel();
              return false;
            });
          });

          await Task.WhenAll(attachConsumerTask, startTask, waitTask, handleDockerExceptionTask);
        }
      }
    }

    private async Task Stop()
    {
      if (this.HasId)
      {
        await this.TestcontainersClient.StopAsync(this.Id);

        this.container = null;
      }
    }

    private async Task CleanUp()
    {
      if (this.HasId)
      {
        await this.TestcontainersClient.RemoveAsync(this.Id);

        this.container = null;
        this.id = null;
      }
    }
  }
}
