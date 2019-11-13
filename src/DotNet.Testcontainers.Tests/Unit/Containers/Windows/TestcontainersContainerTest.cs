namespace DotNet.Testcontainers.Tests.Unit.Containers.Windows
{
  using System.Threading.Tasks;
  using Client;
  using DotNet.Testcontainers.Containers.Builders;
  using DotNet.Testcontainers.Containers.Modules;
  using Xunit;

  public static class TestcontainersContainerTest
  {
    public class With
    {
      [IgnoreOnLinuxEngine]
      public async Task IsWindowsEngineEnabled()
      {
        Assert.True(await new TestcontainersClient().GetIsWindowsEngineEnabled());
      }

      [IgnoreOnLinuxEngine]
      public async Task Disposable()
      {
        // Given
        var testcontainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
          .WithImage("mcr.microsoft.com/windows/nanoserver:1809");

        // When
        // Then
        using (var testcontainer = testcontainersBuilder.Build())
        {
          await testcontainer.StartAsync();
        }
      }
    }
  }
}
