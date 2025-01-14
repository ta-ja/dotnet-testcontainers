namespace DotNet.Testcontainers.Containers
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;

  public interface IDockerContainer : IDisposable
  {
    /// <summary>Gets the Testcontainer id.</summary>
    /// <value>Returns the Docker container id if present or an empty string instead.</value>
    string Id { get; }

    /// <summary>Gets the Testcontainer name.</summary>
    /// <value>Returns the Docker container name if present or an empty string instead.</value>
    string Name { get; }

    /// <summary>Gets the Testcontainer ip address.</summary>
    /// <value>Returns the Docker container ip address if present or an empty string instead.</value>
    string IpAddress { get; }

    /// <summary>Gets the Testcontainer mac address.</summary>
    /// <value>Returns the Docker container mac address if present or an empty string instead.</value>
    string MacAddress { get; }

    /// <summary>
    /// Gets the public host port associated with the private container port.
    /// </summary>
    /// <param name="privatePort">Private container port.</param>
    /// <returns>Returns the public host port associated with the private container port.</returns>
    ushort GetMappedPublicPort(int privatePort);

    /// <summary>
    /// Gets the public host port associated with the private container port.
    /// </summary>
    /// <param name="privatePort">Private container port.</param>
    /// <returns>Returns the public host port associated with the private container port.</returns>
    ushort GetMappedPublicPort(string privatePort);

    /// <summary>
    /// Gets the Testcontainer exit code.
    /// </summary>
    /// <returns>Returns the Docker container exit code.</returns>
    Task<long> GetExitCode();

    /// <summary>
    /// Starts the Testcontainer. If the image does not exist, it will be downloaded automatically. Non-existing containers are created at first start.
    /// </summary>
    /// <returns>A task that represents the asynchronous start operation of a Testcontainer.</returns>
    Task StartAsync();

    /// <summary>
    /// Stops the Testcontainer and removes the container automatically.
    /// </summary>
    /// <returns>A task that represents the asynchronous stop operation of a Testcontainer.</returns>
    Task StopAsync();

    /// <summary>
    /// Executes a command in the running Testcontainer.
    /// </summary>
    /// <param name="command">Shell command.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task that completes when the shell command has been executed.</returns>
    Task<long> ExecAsync(IList<string> command, CancellationToken ct = default);
  }
}
