using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Evidence entity - represents documents, photos, and other evidence
/// </summary>
public class Evidence : BaseAuditableEntity
{
    /// <summary>
    /// Evidence type (نوع الدليل)
    /// </summary>
    public EvidenceType EvidenceType { get; private set; }
    public string Description { get; private set; }

    // File information
    public string OriginalFileName { get; private set; }
    public string FilePath { get; private set; }
    public long FileSizeBytes { get; private set; }
    public string MimeType { get; private set; }
    public string? FileHash { get; private set; }

    // Metadata
    public DateTime? DocumentIssuedDate { get; private set; }
    public DateTime? DocumentExpiryDate { get; private set; }
    public string? IssuingAuthority { get; private set; }
    public string? DocumentReferenceNumber { get; private set; }
    public string? Notes { get; private set; }

    // Versioning
    public int VersionNumber { get; private set; }
    public Guid? PreviousVersionId { get; private set; }
    public bool IsCurrentVersion { get; private set; }

    // Relationships
    public Guid? PersonId { get; private set; }
    public Guid? PersonPropertyRelationId { get; private set; }
    public Guid? ClaimId { get; private set; }

    // Navigation properties
    public virtual Person? Person { get; private set; }
    public virtual PersonPropertyRelation? PersonPropertyRelation { get; private set; }
    public virtual Claim? Claim { get; private set; }
    public virtual Evidence? PreviousVersion { get; private set; }

    private Evidence() : base()
    {
        EvidenceType = EvidenceType.Other;
        Description = string.Empty;
        OriginalFileName = string.Empty;
        FilePath = string.Empty;
        MimeType = string.Empty;
        VersionNumber = 1;
        IsCurrentVersion = true;
    }

    public static Evidence Create(
        EvidenceType evidenceType,
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

    public void LinkToPerson(Guid personId, Guid modifiedByUserId)
    {
        PersonId = personId;
        MarkAsModified(modifiedByUserId);
    }

    public void LinkToRelation(Guid relationId, Guid modifiedByUserId)
    {
        PersonPropertyRelationId = relationId;
        MarkAsModified(modifiedByUserId);
    }

    public void LinkToClaim(Guid claimId, Guid modifiedByUserId)
    {
        ClaimId = claimId;
        MarkAsModified(modifiedByUserId);
    }

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

    public Evidence CreateNewVersion(
        string newFilePath,
        long newFileSizeBytes,
        string? newFileHash,
        Guid createdByUserId)
    {
        IsCurrentVersion = false;
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

    public bool IsExpired() => DocumentExpiryDate.HasValue && DocumentExpiryDate.Value < DateTime.UtcNow;
}
