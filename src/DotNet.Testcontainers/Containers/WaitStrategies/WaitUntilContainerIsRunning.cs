namespace DotNet.Testcontainers.Containers.WaitStrategies
{
  using System;
  using System.Threading.Tasks;
  using DotNet.Testcontainers.Client;

  internal class WaitUntilContainerIsRunning : IWaitUntil
  {
    public virtual async Task<bool> Until(Uri endpoint, string id)
    {
      var container = await new TestcontainersClient(endpoint).DockerContainerClient.ByIdAsync(id);
      return !"Created".Equals(container?.Status);
    }
  }
}
