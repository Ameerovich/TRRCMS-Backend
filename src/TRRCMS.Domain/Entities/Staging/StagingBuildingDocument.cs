using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for BuildingDocument records from .uhc packages.
/// Mirrors the <see cref="BuildingDocument"/> production entity in an isolated staging area.
/// Subject to attachment deduplication by SHA-256 hash.
///
/// Parent entity link stored as original UUID from .uhc (not production FK):
/// - <see cref="OriginalBuildingId"/>: building this document belongs to
///
/// </summary>
public class StagingBuildingDocument : BaseStagingEntity
{
    /// <summary>Original Building UUID from .uhc — not a FK to production Buildings.</summary>
    public Guid OriginalBuildingId { get; private set; }
    /// <summary>Optional description of the document.</summary>
    public string? Description { get; private set; }

    /// <summary>Original file name as it appeared on the tablet.</summary>
    public string OriginalFileName { get; private set; }

    /// <summary>File path within .uhc container or staging storage.</summary>
    public string FilePath { get; private set; }

    /// <summary>File size in bytes.</summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>MIME type of the file (e.g. "image/jpeg", "application/pdf").</summary>
    public string MimeType { get; private set; }

    /// <summary>
    /// SHA-256 hash of the file content for deduplication during commit.
    /// Indexed for fast lookups against existing documents in production.
    /// </summary>
    public string? FileHash { get; private set; }

    /// <summary>Additional notes.</summary>
    public string? Notes { get; private set; }
    /// <summary>EF Core constructor</summary>
    private StagingBuildingDocument()
    {
        OriginalFileName = string.Empty;
        FilePath = string.Empty;
        MimeType = string.Empty;
    }
    /// <summary>
    /// Create a staging building document from .uhc import data.
    /// </summary>
    public static StagingBuildingDocument Create(
        Guid importPackageId,
        Guid originalEntityId,
        Guid originalBuildingId,
        string originalFileName,
        string filePath,
        long fileSizeBytes,
        string mimeType,
        string? description = null,
        string? fileHash = null,
        string? notes = null)
    {
        var entity = new StagingBuildingDocument
        {
            OriginalBuildingId = originalBuildingId,
            Description = description,
            OriginalFileName = originalFileName,
            FilePath = filePath,
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType,
            FileHash = fileHash,
            Notes = notes
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
