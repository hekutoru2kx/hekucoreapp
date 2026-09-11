namespace Hekucoreapp.Infrastructure.Content;

// This core has no tenants, so every blob lands under the constant "global" partition.
public class GlobalContentPartitionResolver : IContentPartitionResolver
{
    public Task<string> ResolveAsync(CancellationToken ct = default) => Task.FromResult("global");
}
