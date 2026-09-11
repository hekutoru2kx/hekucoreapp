using Hekucoreapp.Application.Interfaces;

namespace Hekucoreapp.Infrastructure.Content;

// This core has no tenants, so every blob lands under the constant "global" partition. The
// multi-tenant cores' equivalent resolves tenantId -> Tenant.StoragePrefix instead — that's the
// only DI difference in the blob layer between the single- and multi-tenant variants.
public class GlobalContentPartitionResolver : IContentPartitionResolver
{
    public Task<string> ResolveAsync(int? tenantId) => Task.FromResult("global");
}
