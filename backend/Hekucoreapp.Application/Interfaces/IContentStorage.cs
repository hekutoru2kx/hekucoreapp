namespace Hekucoreapp.Application.Interfaces;

// Abstraction over where uploaded bytes physically live. `partition` is the constant "global"
// in this single-tenant core and a tenant's storage slug in the multi-tenant cores (see
// IContentPartitionResolver) — kept in the signature so it's identical across the family.
public interface IContentStorage
{
    Task<StoredBlobRef> SaveAsync(string partition, Stream content, string fileName, string contentType, CancellationToken ct = default);

    Task<Stream> OpenReadAsync(string container, string blobName, CancellationToken ct = default);

    Task DeleteAsync(string container, string blobName, CancellationToken ct = default);
}

public record StoredBlobRef(string Container, string BlobName);
