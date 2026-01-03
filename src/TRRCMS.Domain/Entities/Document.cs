using System.Xml.Linq;
using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Document entity - represents official documents separate from evidence files
/// This entity tracks document metadata while Evidence entity handles physical files
/// </summary>
public class Document : BaseAuditableEntity
{
    // ==================== DOCUMENT CLASSIFICATION ====================

    /// <summary>
    /// Document type from controlled vocabulary (نوع الوثيقة)
    /// Example: TabuGreen, RentalContract, NationalIdCard, etc.
    /// </summary>
    public DocumentType DocumentType { get; private set; }

    /// <summary>
    /// Document number/reference (رقم الوثيقة)
    /// Example: Tabu number, ID number, contract number
    /// </summary>
    public string? DocumentNumber { get; private set; }

    /// <summary>
    /// Document title or description in Arabic
    /// </summary>
    public string? DocumentTitle { get; private set; }

    // ==================== ISSUANCE INFORMATION ====================

    /// <summary>
    /// Date when document was issued (تاريخ الإصدار)
    /// </summary>
    public DateTime? IssueDate { get; private set; }

    /// <summary>
    /// Date when document expires (if applicable)
    /// </summary>
    public DateTime? ExpiryDate { get; private set; }

    /// <summary>
    /// Issuing authority/organization (الجهة المصدرة)
    /// Example: "Ministry of Interior", "Aleppo Municipality", etc.
    /// </summary>
    public string? IssuingAuthority { get; private set; }

    /// <summary>
    /// Place where document was issued (مكان الإصدار)
    /// </summary>
    public string? IssuingPlace { get; private set; }

    // ==================== VERIFICATION STATUS ====================

    /// <summary>
    /// Indicates if document has been verified (موثق)
    /// </summary>
    public bool IsVerified { get; private set; }

    /// <summary>
    /// Verification status (Pending, Verified, Rejected, etc.)
    /// </summary>
    public VerificationStatus VerificationStatus { get; private set; }

    /// <summary>
    /// Date when document was verified
    /// </summary>
    public DateTime? VerificationDate { get; private set; }

    /// <summary>
    /// User who verified the document
    /// </summary>
    public Guid? VerifiedByUserId { get; private set; }

    /// <summary>
    /// Verification notes or comments
    /// </summary>
    public string? VerificationNotes { get; private set; }

    // ==================== DOCUMENT CONTENT ====================

    /// <summary>
    /// Foreign key to Evidence entity (the actual file/scan)
    /// Links to the physical document scan or photo
    /// </summary>
    public Guid? EvidenceId { get; private set; }

    /// <summary>
    /// SHA-256 hash of the document for integrity verification
    /// </summary>
    public string? DocumentHash { get; private set; }

    /// <summary>
    /// Additional notes about this document
    /// </summary>
    public string? Notes { get; private set; }

    // ==================== DOCUMENT RELATIONSHIPS ====================

    /// <summary>
    /// Foreign key to Person (if document belongs to a person)
    /// Example: ID card, birth certificate
    /// </summary>
    public Guid? PersonId { get; private set; }

    /// <summary>
    /// Foreign key to PropertyUnit (if document relates to property)
    /// Example: Tabu deed, rental contract
    /// </summary>
    public Guid? PropertyUnitId { get; private set; }

    /// <summary>
    /// Foreign key to PersonPropertyRelation (if document proves a relation)
    /// </summary>
    public Guid? PersonPropertyRelationId { get; private set; }

    /// <summary>
    /// Foreign key to Claim (if document supports a claim)
    /// </summary>
    public Guid? ClaimId { get; private set; }

    // ==================== LEGAL VALIDITY ====================

    /// <summary>
    /// Indicates if document is legally valid
    /// </summary>
    public bool IsLegallyValid { get; private set; }

    /// <summary>
    /// Legal validity notes (why valid or invalid)
    /// </summary>
    public string? LegalValidityNotes { get; private set; }

    /// <summary>
    /// Indicates if document is original or a copy
    /// </summary>
    public bool IsOriginal { get; private set; }

    /// <summary>
    /// If copy, reference to original document (if in system)
    /// </summary>
    public Guid? OriginalDocumentId { get; private set; }

    // ==================== NOTARIZATION ====================

    /// <summary>
    /// Indicates if document is notarized (موثق عند كاتب العدل)
    /// </summary>
    public bool IsNotarized { get; private set; }

    /// <summary>
    /// Notary office name/number
    /// </summary>
    public string? NotaryOffice { get; private set; }

    /// <summary>
    /// Notarization date
    /// </summary>
    public DateTime? NotarizationDate { get; private set; }

    /// <summary>
    /// Notarization number
    /// </summary>
    public string? NotarizationNumber { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Evidence/file associated with this document
    /// </summary>
    public virtual Evidence? Evidence { get; private set; }

    /// <summary>
    /// Person this document belongs to
    /// </summary>
    public virtual Person? Person { get; private set; }

    /// <summary>
    /// Property unit this document relates to
    /// </summary>
    public virtual PropertyUnit? PropertyUnit { get; private set; }

    /// <summary>
    /// Person-property relation this document proves
    /// </summary>
    public virtual PersonPropertyRelation? PersonPropertyRelation { get; private set; }

    /// <summary>
    /// Claim this document supports
    /// </summary>
    public virtual Claim? Claim { get; private set; }

    /// <summary>
    /// Original document (if this is a copy)
    /// </summary>
    public virtual Document? OriginalDocument { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Document() : base()
    {
        VerificationStatus = VerificationStatus.Pending;
        IsVerified = false;
        IsLegallyValid = true; // Assume valid until proven otherwise
        IsOriginal = true; // Assume original unless specified
        IsNotarized = false;
    }

    /// <summary>
    /// Create new document
    /// </summary>
    public static Document Create(
        DocumentType documentType,
        string? documentNumber,
        string? documentTitle,
        DateTime? issueDate,
        string? issuingAuthority,
        Guid createdByUserId)
    {
        var document = new Document
        {
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            DocumentTitle = documentTitle,
            IssueDate = issueDate,
            IssuingAuthority = issuingAuthority,
            VerificationStatus = VerificationStatus.Pending,
            IsVerified = false,
            IsLegallyValid = true,
            IsOriginal = true,
            IsNotarized = false
        };

        document.MarkAsCreated(createdByUserId);

        return document;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Update document details
    /// </summary>
    public void UpdateDocumentDetails(
        string? documentNumber,
        string? documentTitle,
        DateTime? issueDate,
        DateTime? expiryDate,
        string? issuingAuthority,
        string? issuingPlace,
        string? notes,
        Guid modifiedByUserId)
    {
        DocumentNumber = documentNumber;
        DocumentTitle = documentTitle;
        IssueDate = issueDate;
        ExpiryDate = expiryDate;
        IssuingAuthority = issuingAuthority;
        IssuingPlace = issuingPlace;
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Link document to evidence file
    /// </summary>
    public void LinkToEvidence(Guid evidenceId, string? documentHash, Guid modifiedByUserId)
    {
        EvidenceId = evidenceId;
        DocumentHash = documentHash;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Link document to person
    /// </summary>
    public void LinkToPerson(Guid personId, Guid modifiedByUserId)
    {
        PersonId = personId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Link document to property unit
    /// </summary>
    public void LinkToPropertyUnit(Guid propertyUnitId, Guid modifiedByUserId)
    {
        PropertyUnitId = propertyUnitId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Link document to person-property relation
    /// </summary>
    public void LinkToRelation(Guid relationId, Guid modifiedByUserId)
    {
        PersonPropertyRelationId = relationId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Link document to claim
    /// </summary>
    public void LinkToClaim(Guid claimId, Guid modifiedByUserId)
    {
        ClaimId = claimId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Verify document as authentic
    /// </summary>
    public void Verify(Guid verifiedByUserId, string? verificationNotes, Guid modifiedByUserId)
    {
        IsVerified = true;
        VerificationStatus = VerificationStatus.Verified;
        VerificationDate = DateTime.UtcNow;
        VerifiedByUserId = verifiedByUserId;
        VerificationNotes = verificationNotes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Reject document verification
    /// </summary>
    public void Reject(string rejectionReason, Guid modifiedByUserId)
    {
        IsVerified = false;
        VerificationStatus = VerificationStatus.Rejected;
        VerificationDate = DateTime.UtcNow;
        VerificationNotes = rejectionReason;
        IsLegallyValid = false;
        LegalValidityNotes = rejectionReason;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark document as requiring additional information
    /// </summary>
    public void MarkAsRequiringAdditionalInfo(string additionalInfoNeeded, Guid modifiedByUserId)
    {
        VerificationStatus = VerificationStatus.RequiresAdditionalInfo;
        VerificationNotes = additionalInfoNeeded;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update notarization details
    /// </summary>
    public void UpdateNotarizationDetails(
        bool isNotarized,
        string? notaryOffice,
        DateTime? notarizationDate,
        string? notarizationNumber,
        Guid modifiedByUserId)
    {
        IsNotarized = isNotarized;
        NotaryOffice = notaryOffice;
        NotarizationDate = notarizationDate;
        NotarizationNumber = notarizationNumber;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark as copy of another document
    /// </summary>
    public void MarkAsCopy(Guid originalDocumentId, Guid modifiedByUserId)
    {
        IsOriginal = false;
        OriginalDocumentId = originalDocumentId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set legal validity status
    /// </summary>
    public void SetLegalValidity(bool isValid, string? notes, Guid modifiedByUserId)
    {
        IsLegallyValid = isValid;
        LegalValidityNotes = notes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Check if document is expired
    /// </summary>
    public bool IsExpired()
    {
        return ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// Check if document is expiring soon (within 30 days)
    /// </summary>
    public bool IsExpiringSoon()
    {
        return ExpiryDate.HasValue
            && ExpiryDate.Value > DateTime.UtcNow
            && ExpiryDate.Value <= DateTime.UtcNow.AddDays(30);
    }
}