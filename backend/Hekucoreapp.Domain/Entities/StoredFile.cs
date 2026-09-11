using Hekucoreapp.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hekucoreapp.Domain.Entities;

// Blob metadata only — the bytes live in blob storage (or App_Data/uploads in LocalDisk dev
// mode), never in Postgres. BlobName already carries the storage partition prefix ("global/..."
// in this single-tenant core — see IContentPartitionResolver; the multi-tenant cores prefix by
// Tenant.StoragePrefix instead).
[Table("stored_files")]
public class StoredFile : AuditableEntity
{
    [Column("id")]
    public int Id { get; set; }

    [Column("container")]
    public string Container { get; set; } = string.Empty;

    [Column("blob_name")]
    public string BlobName { get; set; } = string.Empty;

    [Column("original_file_name")]
    public string OriginalFileName { get; set; } = string.Empty;

    [Column("content_type")]
    public string ContentType { get; set; } = string.Empty;

    [Column("byte_size")]
    public long ByteSize { get; set; }

    // Integrity / dedup, and doubles as the download endpoint's ETag.
    [Column("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    // Images only.
    [Column("width")]
    public int? Width { get; set; }

    [Column("height")]
    public int? Height { get; set; }
}
