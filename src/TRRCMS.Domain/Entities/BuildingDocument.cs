using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Building document entity — a photo or PDF that describes a building (وثيقة البناء).
/// Similar to Evidence but specialized for buildings.
/// Populated only by field survey via .uhc import pipeline.
/// File type is determined by MimeType (e.g., image/jpeg, application/pdf).
/// </summary>
public class BuildingDocument : BaseAuditableEntity
{
    /// <summary>
    /// The building this document belongs to (required FK).
    /// </summary>
    public Guid BuildingId { get; private set; }

    /// <summary>
    /// Optional description of the document
    /// </summary>
    public string? Description { get; private set; }

    // ==================== FILE INFORMATION ====================

    /// <summary>
    /// Original file name as uploaded
    /// </summary>
    public string OriginalFileName { get; private set; }

    /// <summary>
    /// Server-side file path (storage location)
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>
    /// MIME type (e.g., "image/jpeg", "application/pdf")
    /// </summary>
    public string MimeType { get; private set; }

    /// <summary>
    /// SHA-256 hash for deduplication
    /// </summary>
    public string? FileHash { get; private set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private BuildingDocument() : base()
    {
        OriginalFileName = string.Empty;
        FilePath = string.Empty;
        MimeType = string.Empty;
    }

    // ==================== FACTORY METHOD ====================

    /// <summary>
    /// Create a new building document (Factory Method — DDD pattern)
    /// </summary>
    public static BuildingDocument Create(
        Guid buildingId,
        string? description,
        string originalFileName,
        string filePath,
        long fileSizeBytes,
        string mimeType,
        string? fileHash,
        Guid createdByUserId)
    {
        var document = new BuildingDocument
        {
            BuildingId = buildingId,
            Description = description,
            OriginalFileName = originalFileName,
            FilePath = filePath,
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType,
            FileHash = fileHash
        };
        document.MarkAsCreated(createdByUserId);
        return document;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Update document description and notes
    /// </summary>
    public void UpdateDescription(string? description, string? notes, Guid modifiedByUserId)
    {
        Description = description;
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }
}
