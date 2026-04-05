using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Identification document entity — personal ID documents linked to a Person.
/// Separate from Evidence (which handles tenure documents linked to PersonPropertyRelation).
/// </summary>
public class IdentificationDocument : BaseAuditableEntity
{
    /// <summary>
    /// Foreign key to the Person this document belongs to
    /// </summary>
    public Guid PersonId { get; private set; }

    /// <summary>
    /// Document type (نوع الوثيقة)
    /// </summary>
    public DocumentType DocumentType { get; private set; }

    /// <summary>
    /// Description of the document
    /// </summary>
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

    // Navigation
    public virtual Person Person { get; private set; } = null!;

    private IdentificationDocument() : base()
    {
        DocumentType = DocumentType.PersonalIdPhoto;
        Description = string.Empty;
        OriginalFileName = string.Empty;
        FilePath = string.Empty;
        MimeType = string.Empty;
    }

    public static IdentificationDocument Create(
        DocumentType documentType,
        string description,
        string originalFileName,
        string filePath,
        long fileSizeBytes,
        string mimeType,
        string? fileHash,
        Guid personId,
        Guid createdByUserId)
    {
        var doc = new IdentificationDocument
        {
            DocumentType = documentType,
            Description = description,
            OriginalFileName = originalFileName,
            FilePath = filePath,
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType,
            FileHash = fileHash,
            PersonId = personId
        };
        doc.MarkAsCreated(createdByUserId);
        return doc;
    }

    public void UpdateFileInfo(
        string filePath,
        string originalFileName,
        long fileSizeBytes,
        string mimeType,
        string? fileHash,
        Guid modifiedByUserId)
    {
        FilePath = filePath;
        OriginalFileName = originalFileName;
        FileSizeBytes = fileSizeBytes;
        MimeType = mimeType;
        FileHash = fileHash;
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

    public void UpdateDocumentType(DocumentType documentType, Guid modifiedByUserId)
    {
        DocumentType = documentType;
        MarkAsModified(modifiedByUserId);
    }

    public void UpdateDescription(string description, Guid modifiedByUserId)
    {
        Description = description;
        MarkAsModified(modifiedByUserId);
    }

    public void LinkToPerson(Guid personId, Guid modifiedByUserId)
    {
        PersonId = personId;
        MarkAsModified(modifiedByUserId);
    }

    public bool IsExpired() => DocumentExpiryDate.HasValue && DocumentExpiryDate.Value < DateTime.UtcNow;
}
