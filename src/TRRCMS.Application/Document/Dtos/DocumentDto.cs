namespace TRRCMS.Application.Documents.Dtos;

/// <summary>
/// Data transfer object for Document entity
/// </summary>
public class DocumentDto
{
    // ==================== IDENTIFIERS ====================

    public Guid Id { get; set; }

    // ==================== DOCUMENT CLASSIFICATION ====================

    /// <summary>
    /// Document type (enum converted to string)
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Document number/reference
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Document title or description
    /// </summary>
    public string? DocumentTitle { get; set; }

    // ==================== ISSUANCE INFORMATION ====================

    /// <summary>
    /// Date when document was issued
    /// </summary>
    public DateTime? IssueDate { get; set; }

    /// <summary>
    /// Date when document expires
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Issuing authority/organization
    /// </summary>
    public string? IssuingAuthority { get; set; }

    /// <summary>
    /// Place where document was issued
    /// </summary>
    public string? IssuingPlace { get; set; }

    // ==================== VERIFICATION STATUS ====================

    /// <summary>
    /// Indicates if document has been verified
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Verification status (enum converted to string)
    /// </summary>
    public string VerificationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Date when document was verified
    /// </summary>
    public DateTime? VerificationDate { get; set; }

    /// <summary>
    /// User who verified the document
    /// </summary>
    public Guid? VerifiedByUserId { get; set; }

    /// <summary>
    /// Verification notes or comments
    /// </summary>
    public string? VerificationNotes { get; set; }

    // ==================== DOCUMENT CONTENT ====================

    /// <summary>
    /// Foreign key to Evidence entity (the actual file/scan)
    /// </summary>
    public Guid? EvidenceId { get; set; }

    /// <summary>
    /// SHA-256 hash of the document
    /// </summary>
    public string? DocumentHash { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    // ==================== DOCUMENT RELATIONSHIPS ====================

    /// <summary>
    /// Foreign key to Person
    /// </summary>
    public Guid? PersonId { get; set; }

    /// <summary>
    /// Foreign key to PropertyUnit
    /// </summary>
    public Guid? PropertyUnitId { get; set; }

    /// <summary>
    /// Foreign key to PersonPropertyRelation
    /// </summary>
    public Guid? PersonPropertyRelationId { get; set; }

    /// <summary>
    /// Foreign key to Claim
    /// </summary>
    public Guid? ClaimId { get; set; }

    // ==================== LEGAL VALIDITY ====================

    /// <summary>
    /// Indicates if document is legally valid
    /// </summary>
    public bool IsLegallyValid { get; set; }

    /// <summary>
    /// Legal validity notes
    /// </summary>
    public string? LegalValidityNotes { get; set; }

    /// <summary>
    /// Indicates if document is original or a copy
    /// </summary>
    public bool IsOriginal { get; set; }

    /// <summary>
    /// If copy, reference to original document
    /// </summary>
    public Guid? OriginalDocumentId { get; set; }

    // ==================== NOTARIZATION ====================

    /// <summary>
    /// Indicates if document is notarized
    /// </summary>
    public bool IsNotarized { get; set; }

    /// <summary>
    /// Notary office name/number
    /// </summary>
    public string? NotaryOffice { get; set; }

    /// <summary>
    /// Notarization date
    /// </summary>
    public DateTime? NotarizationDate { get; set; }

    /// <summary>
    /// Notarization number
    /// </summary>
    public string? NotarizationNumber { get; set; }

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
    /// Indicates if the document is expired (computed from ExpiryDate)
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// Indicates if the document is expiring soon (within 30 days)
    /// </summary>
    public bool IsExpiringSoon { get; set; }
}
