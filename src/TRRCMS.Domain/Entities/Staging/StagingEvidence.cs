using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for Evidence records from .uhc packages.
/// Mirrors the <see cref="Evidence"/> production entity in an isolated staging area.
/// Subject to attachment deduplication by SHA-256 hash (FSD FR-D-9):
/// - <see cref="FileHash"/> is indexed for fast deduplication lookups
/// - During commit, existing evidence with same hash is reused instead of re-storing
/// 
/// Parent entity links are stored as original UUIDs from .uhc (not production FKs):
/// - <see cref="OriginalPersonId"/>: person who owns this evidence
/// - <see cref="OriginalPersonPropertyRelationId"/>: relation this evidence supports
/// - <see cref="OriginalClaimId"/>: claim this evidence belongs to
/// 
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingEvidence : BaseStagingEntity
{
    // ==================== RELATIONSHIPS (original UUIDs from .uhc) ====================

    /// <summary>Original Person UUID from .uhc — not a FK to production Persons.</summary>
    public Guid? OriginalPersonId { get; private set; }

    /// <summary>Original PersonPropertyRelation UUID from .uhc.</summary>
    public Guid? OriginalPersonPropertyRelationId { get; private set; }

    /// <summary>Original Claim UUID from .uhc.</summary>
    public Guid? OriginalClaimId { get; private set; }

    // ==================== EVIDENCE METADATA ====================

    /// <summary>Type of evidence (IdentificationDocument, OwnershipDeed, etc.).</summary>
    public EvidenceType EvidenceType { get; private set; }

    /// <summary>Description of the evidence.</summary>
    public string Description { get; private set; }

    /// <summary>Original file name as it appeared on the tablet.</summary>
    public string OriginalFileName { get; private set; }

    /// <summary>File path within .uhc container or staging storage.</summary>
    public string FilePath { get; private set; }

    /// <summary>File size in bytes.</summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>MIME type of the file (e.g. "image/jpeg", "application/pdf").</summary>
    public string MimeType { get; private set; }

    /// <summary>
    /// SHA-256 hash of the file content for deduplication during commit (FR-D-9).
    /// Indexed for fast lookups against existing evidence in production.
    /// </summary>
    public string? FileHash { get; private set; }

    // ==================== DOCUMENT DETAILS ====================

    /// <summary>Authority that issued the document (e.g. government office).</summary>
    public string? IssuingAuthority { get; private set; }

    /// <summary>Date when document was issued — from command, optional.</summary>
    public DateTime? DocumentIssuedDate { get; private set; }

    /// <summary>Date when document expires — from command, optional.</summary>
    public DateTime? DocumentExpiryDate { get; private set; }

    /// <summary>Reference number on the document.</summary>
    public string? DocumentReferenceNumber { get; private set; }

    /// <summary>Additional notes about this evidence.</summary>
    public string? Notes { get; private set; }

    // ==================== VERSION TRACKING ====================

    /// <summary>Version number of this evidence (supports re-uploads).</summary>
    public int VersionNumber { get; private set; }

    /// <summary>Original previous version UUID from .uhc (for version chains).</summary>
    public Guid? OriginalPreviousVersionId { get; private set; }

    /// <summary>Whether this is the current (latest) version.</summary>
    public bool IsCurrentVersion { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>EF Core constructor.</summary>
    private StagingEvidence() : base()
    {
        Description = string.Empty;
        OriginalFileName = string.Empty;
        FilePath = string.Empty;
        MimeType = string.Empty;
        VersionNumber = 1;
        IsCurrentVersion = true;
    }

    // ==================== FACTORY METHOD ====================

    /// <summary>
    /// Create a new StagingEvidence record from .uhc package data.
    /// </summary>
    public static StagingEvidence Create(
        Guid importPackageId,
        Guid originalEntityId,
        EvidenceType evidenceType,
        string description,
        string originalFileName,
        string filePath,
        long fileSizeBytes,
        string mimeType,
        // --- optional: from command ---
        Guid? originalPersonId = null,
        Guid? originalPersonPropertyRelationId = null,
        Guid? originalClaimId = null,
        string? fileHash = null,
        DateTime? documentIssuedDate = null,
        DateTime? documentExpiryDate = null,
        string? issuingAuthority = null,
        string? documentReferenceNumber = null,
        string? notes = null,
        // --- optional: future expansion ---
        int versionNumber = 1,
        Guid? originalPreviousVersionId = null,
        bool isCurrentVersion = true)
    {
        var entity = new StagingEvidence
        {
            OriginalPersonId = originalPersonId,
            OriginalPersonPropertyRelationId = originalPersonPropertyRelationId,
            OriginalClaimId = originalClaimId,
            EvidenceType = evidenceType,
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
            Notes = notes,
            VersionNumber = versionNumber,
            OriginalPreviousVersionId = originalPreviousVersionId,
            IsCurrentVersion = isCurrentVersion
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
