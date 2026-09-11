namespace Hekucoreapp.Infrastructure.Content;

// This core has no tenants, so every blob lands under one constant partition. Named after the
// app itself (not the generic "global") because the family's blob storage account is shared
// across all four repos (hekucoreapp/hekutenantcoreapp/gestamind/ludemia) — a literal "global"
// here would collide/commingle with the same literal in ludemia (and with the multi-tenant
// cores' own "no resolvable tenant" fallback), making it impossible to tell which blob belongs
// to which app. Blob names still end in a random GUID, so this was never a data-loss risk, only
// an organizational one — same principle Tenant.StoragePrefix exists for one level down.
public class GlobalContentPartitionResolver : IContentPartitionResolver
{
    public Task<string> ResolveAsync(CancellationToken ct = default) => Task.FromResult("hekucoreapp");
}
