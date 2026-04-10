using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for IdentificationDocument records from .uhc packages.
/// Mirrors the <see cref="IdentificationDocument"/> production entity.
/// </summary>
public class StagingIdentificationDocument : BaseStagingEntity
{
    /// <summary>Original Person UUID from .uhc.</summary>
    public Guid OriginalPersonId { get; private set; }

    /// <summary>Document type (PersonalIdPhoto, FamilyRecord, Photo).</summary>
    public DocumentType? DocumentType { get; private set; }

    /// <summary>Description of the document.</summary>
    public string? Description { get; private set; }

    /// <summary>Original file name as it appeared on the tablet.</summary>
    public string OriginalFileName { get; private set; }

    /// <summary>File path within .uhc container or staging storage.</summary>
    public string FilePath { get; private set; }

    /// <summary>File size in bytes.</summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>MIME type of the file.</summary>
    public string MimeType { get; private set; }

    /// <summary>SHA-256 hash of the file content for deduplication.</summary>
    public string? FileHash { get; private set; }

    /// <summary>Date when document was issued.</summary>
    public DateTime? DocumentIssuedDate { get; private set; }

    /// <summary>Date when document expires.</summary>
    public DateTime? DocumentExpiryDate { get; private set; }

    /// <summary>Authority that issued the document.</summary>
    public string? IssuingAuthority { get; private set; }

    /// <summary>Reference number on the document.</summary>
    public string? DocumentReferenceNumber { get; private set; }

    /// <summary>Additional notes.</summary>
    public string? Notes { get; private set; }

    /// <summary>EF Core constructor.</summary>
    private StagingIdentificationDocument() : base()
    {
        OriginalFileName = string.Empty;
        FilePath = string.Empty;
        MimeType = string.Empty;
    }

    /// <summary>
    /// Create a new StagingIdentificationDocument from .uhc package data.
    /// </summary>
    public static StagingIdentificationDocument Create(
        Guid importPackageId,
        Guid originalEntityId,
        Guid originalPersonId,
        DocumentType? documentType,
        string? description,
        string originalFileName,
        string filePath,
        long fileSizeBytes,
        string mimeType,
        string? fileHash = null,
        DateTime? documentIssuedDate = null,
        DateTime? documentExpiryDate = null,
        string? issuingAuthority = null,
        string? documentReferenceNumber = null,
        string? notes = null)
    {
        var entity = new StagingIdentificationDocument
        {
            OriginalPersonId = originalPersonId,
            DocumentType = documentType,
            Description = description,
            OriginalFileName = originalFileName,
            FilePath = filePath,
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType,
            FileHash = fileHash,
            DocumentIssuedDate = documentIssuedDate,
            DocumentExpiryDate = documentExpiryDate,
            IssuingAuthority = issuingAuthority,
            DocumentReferenceNumber = documentReferenceNumber,
            Notes = notes
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
