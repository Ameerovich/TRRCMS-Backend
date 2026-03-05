namespace TRRCMS.Application.Buildings.Dtos;

/// <summary>
/// Building Document Data Transfer Object.
/// Represents a photo or PDF document describing a building.
/// </summary>
public class BuildingDocumentDto
{
    /// <summary>Database ID (GUID)</summary>
    public Guid Id { get; set; }

    /// <summary>Document type (0 = Photo, 1 = PDF)</summary>
    public int DocumentType { get; set; }

    /// <summary>Optional description</summary>
    public string? Description { get; set; }

    /// <summary>Original file name</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Server-side file path</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>File size in bytes</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>MIME type (e.g., "image/jpeg", "application/pdf")</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>SHA-256 hash for deduplication</summary>
    public string? FileHash { get; set; }

    /// <summary>Additional notes</summary>
    public string? Notes { get; set; }

    /// <summary>Created timestamp</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Last modified timestamp</summary>
    public DateTime? LastModifiedAtUtc { get; set; }
}
