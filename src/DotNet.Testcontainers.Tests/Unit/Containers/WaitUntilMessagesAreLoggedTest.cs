namespace DotNet.Testcontainers.Tests.Unit.Containers
{
  using System;
  using System.IO;
  using System.Text;
  using System.Threading.Tasks;
  using Moq;
  using Testcontainers.Containers.WaitStrategies;
  using Xunit;

  public class WaitUntilMessagesAreLoggedTest
  {
    [Fact]
    public async Task InvalidNullParameter()
    {
      // Given
      var wait = new WaitUntilMessagesAreLogged(null);

      // Then
      await Assert.ThrowsAsync<NullReferenceException>(async () =>
      {
        // When
        await WaitStrategy.WaitUntil(() => wait.Until(string.Empty));
      });
    }

    [Fact]
    public async Task ValidNullStreamParameter()
    {
      // Given
      var wait = new WaitUntilMessagesAreLogged(Stream.Null);

      // When
      // Then
      await WaitStrategy.WaitUntil(() => wait.Until(string.Empty));
    }

    [Fact]
    public async Task MessageFound()
    {
      // Given
      var msg = @"""
                aaaaa
                bbbbb
                test msg
                ccccc
                """;
      var stream = new MemoryStream(Encoding.UTF8.GetBytes(msg));
      var wait = new WaitUntilMessagesAreLogged(stream, "test msg");

      // When
      // Then
      await WaitStrategy.WaitUntil(() => wait.Until(string.Empty));
    }

    [Fact]
    public async Task MessageNotFound()
    {
      // Given
      var msg = @"""
                aaaaa
                bbbbb
                ccccc
                """;
      var stream = new MemoryStream(Encoding.UTF8.GetBytes(msg));
      var wait = new WaitUntilMessagesAreLogged(stream, "test msg");

      // Then
      await Assert.ThrowsAsync<TimeoutException>(async () =>
      {
        // When
        await WaitStrategy.WaitUntil(() => wait.Until(string.Empty), timeout: 100);
      });
    }

    [Fact]
    public async Task UnreadableStream()
    {
      // Given
      var streamMock = new Mock<Stream>();
      streamMock.Setup(stream => stream.CanSeek).Returns(false); // unreadable stream
      var wait = new WaitUntilMessagesAreLogged(streamMock.Object, "test msg");

      // Then
      await Assert.ThrowsAsync<ArgumentException>(async () =>
      {
        // When
        await WaitStrategy.WaitUntil(() => wait.Until(string.Empty), timeout: 100);
      });
    }
  }
}
