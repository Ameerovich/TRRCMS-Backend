using System.Security.Claims;
using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Evidence entity - represents documents, photos, and other evidence
/// Supporting documentation for claims, relations, and person identification
/// </summary>
public class Evidence : BaseAuditableEntity
{
    // ==================== EVIDENCE CLASSIFICATION ====================

    /// <summary>
    /// Evidence type (controlled vocabulary)
    /// </summary>
    public string EvidenceType { get; private set; }

    /// <summary>
    /// Document or evidence description
    /// </summary>
    public string Description { get; private set; }

    // ==================== FILE INFORMATION ====================

    /// <summary>
    /// Original filename as uploaded
    /// </summary>
    public string OriginalFileName { get; private set; }

    /// <summary>
    /// File path in storage system
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>
    /// MIME type (e.g., image/jpeg, application/pdf)
    /// </summary>
    public string MimeType { get; private set; }

    /// <summary>
    /// SHA-256 hash of the file for integrity verification
    /// </summary>
    public string? FileHash { get; private set; }

    // ==================== METADATA ====================

    /// <summary>
    /// Date when the document was issued (if applicable)
    /// For official documents like IDs, deeds, contracts
    /// </summary>
    public DateTime? DocumentIssuedDate { get; private set; }

    /// <summary>
    /// Date when the document expires (if applicable)
    /// For IDs, permits, temporary documents
    /// </summary>
    public DateTime? DocumentExpiryDate { get; private set; }

    /// <summary>
    /// Issuing authority or organization
    /// Example: "Ministry of Interior", "Municipality", "Private Contract"
    /// </summary>
    public string? IssuingAuthority { get; private set; }

    /// <summary>
    /// Document reference number (if any)
    /// Official document numbers, deed numbers, etc.
    /// </summary>
    public string? DocumentReferenceNumber { get; private set; }

    /// <summary>
    /// Additional notes about this evidence
    /// </summary>
    public string? Notes { get; private set; }

    // ==================== VERSIONING ====================

    /// <summary>
    /// Version number for document versioning
    /// </summary>
    public int VersionNumber { get; private set; }

    /// <summary>
    /// Reference to previous version (if this is an updated version)
    /// </summary>
    public Guid? PreviousVersionId { get; private set; }

    /// <summary>
    /// Indicates if this is the current/latest version
    /// </summary>
    public bool IsCurrentVersion { get; private set; }

    // ==================== RELATIONSHIPS ====================

    /// <summary>
    /// Foreign key to Person (if evidence is linked to a person)
    /// </summary>
    public Guid? PersonId { get; private set; }

    /// <summary>
    /// Foreign key to PersonPropertyRelation (if evidence supports a relation)
    /// </summary>
    public Guid? PersonPropertyRelationId { get; private set; }

    /// <summary>
    /// Foreign key to Claim (if evidence supports a claim)
    /// </summary>
    public Guid? ClaimId { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Person this evidence is linked to (if applicable)
    /// </summary>
    public virtual Person? Person { get; private set; }

    /// <summary>
    /// Person-property relation this evidence supports (if applicable)
    /// </summary>
    public virtual PersonPropertyRelation? PersonPropertyRelation { get; private set; }

    /// <summary>
    /// Claim this evidence supports (if applicable)
    /// </summary>
    public virtual Claim? Claim { get; private set; }

    /// <summary>
    /// Previous version of this document (if versioned)
    /// </summary>
    public virtual Evidence? PreviousVersion { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Evidence() : base()
    {
        EvidenceType = string.Empty;
        Description = string.Empty;
        OriginalFileName = string.Empty;
        FilePath = string.Empty;
        MimeType = string.Empty;
        VersionNumber = 1;
        IsCurrentVersion = true;
    }

    /// <summary>
    /// Create new evidence (document/photo)
    /// </summary>
    public static Evidence Create(
        string evidenceType,
        string description,
        string originalFileName,
        string filePath,
        long fileSizeBytes,
        string mimeType,
        string? fileHash,
        Guid createdByUserId)
    {
        var evidence = new Evidence
        {
            EvidenceType = evidenceType,
            Description = description,
            OriginalFileName = originalFileName,
            FilePath = filePath,
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType,
            FileHash = fileHash,
            VersionNumber = 1,
            IsCurrentVersion = true
        };

        evidence.MarkAsCreated(createdByUserId);

        return evidence;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Link evidence to a person (for ID documents)
    /// </summary>
    public void LinkToPerson(Guid personId, Guid modifiedByUserId)
    {
        PersonId = personId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Link evidence to a person-property relation (for ownership/rental docs)
    /// </summary>
    public void LinkToRelation(Guid relationId, Guid modifiedByUserId)
    {
        PersonPropertyRelationId = relationId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Link evidence to a claim
    /// </summary>
    public void LinkToClaim(Guid claimId, Guid modifiedByUserId)
    {
        ClaimId = claimId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update document metadata
    /// </summary>
    public void UpdateMetadata(
        DateTime? issuedDate,
        DateTime? expiryDate,
        string? issuingAuthority,
        string? referenceNumber,
        string? notes,
        Guid modifiedByUserId)
    {
        DocumentIssuedDate = issuedDate;
        DocumentExpiryDate = expiryDate;
        IssuingAuthority = issuingAuthority;
        DocumentReferenceNumber = referenceNumber;
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Create a new version of this evidence (document replacement via versioning)
    /// </summary>
    public Evidence CreateNewVersion(
        string newFilePath,
        long newFileSizeBytes,
        string? newFileHash,
        Guid createdByUserId)
    {
        // Mark current version as not current
        IsCurrentVersion = false;

        // Create new version
        var newVersion = new Evidence
        {
            EvidenceType = EvidenceType,
            Description = Description,
            OriginalFileName = OriginalFileName,
            FilePath = newFilePath,
            FileSizeBytes = newFileSizeBytes,
            MimeType = MimeType,
            FileHash = newFileHash,
            DocumentIssuedDate = DocumentIssuedDate,
            DocumentExpiryDate = DocumentExpiryDate,
            IssuingAuthority = IssuingAuthority,
            DocumentReferenceNumber = DocumentReferenceNumber,
            Notes = Notes,
            VersionNumber = VersionNumber + 1,
            IsCurrentVersion = true,
            PreviousVersionId = Id,
            PersonId = PersonId,
            PersonPropertyRelationId = PersonPropertyRelationId,
            ClaimId = ClaimId
        };

        newVersion.MarkAsCreated(createdByUserId);

        return newVersion;
    }

    /// <summary>
    /// Check if document is expired
    /// </summary>
    public bool IsExpired()
    {
        return DocumentExpiryDate.HasValue && DocumentExpiryDate.Value < DateTime.UtcNow;
    }
}