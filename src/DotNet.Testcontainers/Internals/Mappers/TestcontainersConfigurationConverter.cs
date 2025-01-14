namespace DotNet.Testcontainers.Internals.Mappers
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using Docker.DotNet.Models;
  using DotNet.Testcontainers.Containers.Configurations;

  internal class TestcontainersConfigurationConverter
  {
    public TestcontainersConfigurationConverter(TestcontainersConfiguration config)
    {
      this.Config = config;
    }

    public IList<string> Entrypoint => new ToList().Convert(this.Config.Container.Entrypoint);

    public IList<string> Command => new ToList().Convert(this.Config.Container.Command);

    public IList<string> Environments => new ToMappedList().Convert(this.Config.Container.Environments);

    public IDictionary<string, string> Labels => new ToDictionary().Convert(this.Config.Container.Labels);

    public IDictionary<string, EmptyStruct> ExposedPorts => new ToExposedPorts().Convert(this.Config.Container.ExposedPorts);

    public IDictionary<string, IList<PortBinding>> PortBindings => new ToPortBindings().Convert(this.Config.Host.PortBindings);

    public IList<Mount> Mounts => new ToMounts().Convert(this.Config.Host.Mounts);

    private TestcontainersConfiguration Config { get; }

    private class ToList : CollectionConverter<IList<string>>
    {
      public override IList<string> Convert(IReadOnlyCollection<string> source)
      {
        return source?.ToList();
      }
    }

    private class ToDictionary : DictionaryConverter<IDictionary<string, string>>
    {
      public override IDictionary<string, string> Convert(IReadOnlyDictionary<string, string> source)
      {
        return source?.ToDictionary(item => item.Key, item => item.Value);
      }
    }

    private class ToMappedList : DictionaryConverter<IList<string>>
    {
      public override IList<string> Convert(IReadOnlyDictionary<string, string> source)
      {
        return source?.Select(item => $"{item.Key}={item.Value}").ToList();
      }
    }

    private class ToExposedPorts : DictionaryConverter<IDictionary<string, EmptyStruct>>
    {
      public ToExposedPorts() : base(nameof(ToExposedPorts))
      {
      }

      public override IDictionary<string, EmptyStruct> Convert(IReadOnlyDictionary<string, string> source)
      {
        return source?.ToDictionary(exposedPort => $"{exposedPort.Key}/tcp", exposedPort => default(EmptyStruct));
      }
    }

    private class ToPortBindings : DictionaryConverter<IDictionary<string, IList<PortBinding>>>
    {
      public ToPortBindings() : base(nameof(ToPortBindings))
      {
      }

      public override IDictionary<string, IList<PortBinding>> Convert(IReadOnlyDictionary<string, string> source)
      {
        return source?.ToDictionary(binding => $"{binding.Value}/tcp", binding => new List<PortBinding> { new PortBinding { HostPort = binding.Key } } as IList<PortBinding>);
      }
    }

    private class ToMounts : DictionaryConverter<IList<Mount>>
    {
      public ToMounts() : base(nameof(ToMounts))
      {
      }

      public override IList<Mount> Convert(IReadOnlyDictionary<string, string> source)
      {
        return source?.Select(mount => new Mount { Source = Path.GetFullPath(mount.Key), Target = mount.Value, Type = "bind" }).ToList();
      }
    }
  }
}
