namespace Hekucoreapp.Application.Interfaces;

// Abstraction over where uploaded bytes physically live. Deliberately has no notion of a
// "partition"/tenant in this signature — ContentService (Application layer) has no clean way to
// resolve "the current tenant" itself (only Infrastructure can, via the DbContext), so each
// implementation resolves its own storage partition internally via IContentPartitionResolver
// (a constant "global" here and in ludemia; the caller's Tenant.StoragePrefix in the
// multi-tenant cores) instead of taking one as a parameter.
public interface IContentStorage
{
    Task<StoredBlobRef> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);

    Task<Stream> OpenReadAsync(string container, string blobName, CancellationToken ct = default);

    Task DeleteAsync(string container, string blobName, CancellationToken ct = default);
}

public record StoredBlobRef(string Container, string BlobName);
