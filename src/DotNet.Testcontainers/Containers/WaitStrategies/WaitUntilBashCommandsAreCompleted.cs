﻿namespace DotNet.Testcontainers.Containers.WaitStrategies
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using DotNet.Testcontainers.Client;

  internal class WaitUntilBashCommandsAreCompleted : WaitUntilContainerIsRunning
  {
    private readonly string[] bashCommands;

    public WaitUntilBashCommandsAreCompleted(params string[] bashCommands)
    {
      this.bashCommands = bashCommands;
    }

    public override async Task<bool> Until(Uri endpoint, string id)
    {
      await WaitStrategy.WaitUntil(() => base.Until(endpoint, id));

      var exitCodes = await Task.WhenAll(this.bashCommands.Select(command => new TestcontainersClient(endpoint).ExecAsync(id, new[] { "/bin/bash", "-c", command })).ToList());

      return exitCodes.All(exitCode => 0L.Equals(exitCode));
    }
  }
}
