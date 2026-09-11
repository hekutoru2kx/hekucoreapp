namespace Hekucoreapp.Infrastructure.Content;

// Internal to the storage layer — ContentService (Application) never sees a partition; each
// IContentStorage implementation resolves its own via this before building a blob path. This
// core has no tenants, so GlobalContentPartitionResolver always returns "global". The
// multi-tenant cores' equivalent resolves the caller's current tenant to its
// Tenant.StoragePrefix instead — that's the only DI difference in the blob layer between the
// single- and multi-tenant variants.
public interface IContentPartitionResolver
{
    Task<string> ResolveAsync(CancellationToken ct = default);
}
