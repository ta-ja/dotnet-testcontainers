namespace DotNet.Testcontainers.Client
{
  using System;
  using System.Runtime.InteropServices;

  internal static class DockerApiEndpoint
  {
#pragma warning disable S1075

    public static Uri LocalEndpoint { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Uri("npipe://./pipe/docker_engine") : new Uri("unix:/var/run/docker.sock");

#pragma warning restore S1075
  }
}
