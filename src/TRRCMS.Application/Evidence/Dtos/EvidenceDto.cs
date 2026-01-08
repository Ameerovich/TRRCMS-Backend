namespace TRRCMS.Application.Evidences.Dtos;

/// <summary>
/// Data transfer object for Evidence entity
/// </summary>
public class EvidenceDto
{
    // ==================== IDENTIFIERS ====================

    public Guid Id { get; set; }

    // ==================== EVIDENCE CLASSIFICATION ====================

    /// <summary>
    /// Evidence type (controlled vocabulary)
    /// </summary>
    public string EvidenceType { get; set; } = string.Empty;

    /// <summary>
    /// Document or evidence description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    // ==================== FILE INFORMATION ====================

    /// <summary>
    /// Original filename as uploaded
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// File path in storage system
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// MIME type (e.g., image/jpeg, application/pdf)
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the file for integrity verification
    /// </summary>
    public string? FileHash { get; set; }

    // ==================== METADATA ====================

    /// <summary>
    /// Date when the document was issued (if applicable)
    /// </summary>
    public DateTime? DocumentIssuedDate { get; set; }

    /// <summary>
    /// Date when the document expires (if applicable)
    /// </summary>
    public DateTime? DocumentExpiryDate { get; set; }

    /// <summary>
    /// Issuing authority or organization
    /// </summary>
    public string? IssuingAuthority { get; set; }

    /// <summary>
    /// Document reference number (if any)
    /// </summary>
    public string? DocumentReferenceNumber { get; set; }

    /// <summary>
    /// Additional notes about this evidence
    /// </summary>
    public string? Notes { get; set; }

    // ==================== VERSIONING ====================

    /// <summary>
    /// Version number for document versioning
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Reference to previous version (if this is an updated version)
    /// </summary>
    public Guid? PreviousVersionId { get; set; }

    /// <summary>
    /// Indicates if this is the current/latest version
    /// </summary>
    public bool IsCurrentVersion { get; set; }

    // ==================== RELATIONSHIPS ====================

    /// <summary>
    /// Foreign key to Person (if evidence is linked to a person)
    /// </summary>
    public Guid? PersonId { get; set; }

    /// <summary>
    /// Foreign key to PersonPropertyRelation (if evidence supports a relation)
    /// </summary>
    public Guid? PersonPropertyRelationId { get; set; }

    /// <summary>
    /// Foreign key to Claim (if evidence supports a claim)
    /// </summary>
    public Guid? ClaimId { get; set; }

    // ==================== AUDIT FIELDS ====================

    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }

    // ==================== COMPUTED PROPERTIES ====================

    /// <summary>
    /// Indicates if the document is expired (computed from DocumentExpiryDate)
    /// </summary>
    public bool IsExpired { get; set; }
}
