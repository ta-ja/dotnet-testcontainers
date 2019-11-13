namespace DotNet.Testcontainers.Client
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  internal abstract class AbstractContainerImageClient<T> : DockerApiClient
    where T : class
  {
    protected AbstractContainerImageClient(Uri endpoint) : base(endpoint)
    {
    }

    public abstract Task<IReadOnlyCollection<T>> GetAllAsync();

    public abstract Task<T> ByIdAsync(string id);

    public abstract Task<T> ByNameAsync(string name);

    public abstract Task<T> ByPropertyAsync(string property, string value);

    public async Task<bool> ExistsWithIdAsync(string id)
    {
      return !(await this.ByIdAsync(id) is null);
    }

    public async Task<bool> ExistsWithNameAsync(string name)
    {
      return !(await this.ByNameAsync(name) is null);
    }
  }
}
