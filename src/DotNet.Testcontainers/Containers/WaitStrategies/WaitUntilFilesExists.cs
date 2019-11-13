namespace DotNet.Testcontainers.Containers.WaitStrategies
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;

  internal class WaitUntilFilesExists : WaitUntilContainerIsRunning
  {
    private readonly string[] files;

    public WaitUntilFilesExists(params string[] files)
    {
      this.files = files;
    }

    public override async Task<bool> Until(Uri endpoint, string id)
    {
      await WaitStrategy.WaitUntil(() => base.Until(endpoint, id));
      return this.files.All(File.Exists);
    }
  }
}
