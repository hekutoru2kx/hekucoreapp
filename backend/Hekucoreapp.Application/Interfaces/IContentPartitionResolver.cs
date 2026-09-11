namespace Hekucoreapp.Application.Interfaces;

// The single seam that differs between this single-tenant core and the multi-tenant cores in
// the blob layer: there, tenantId maps to Tenant.StoragePrefix; here there are no tenants, so
// it always resolves to "global" (see the design doc's blob-partitioning section).
public interface IContentPartitionResolver
{
    Task<string> ResolveAsync(int? tenantId);
}
