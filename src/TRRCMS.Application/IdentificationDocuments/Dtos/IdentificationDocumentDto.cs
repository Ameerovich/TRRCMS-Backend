namespace TRRCMS.Application.IdentificationDocuments.Dtos;

/// <summary>
/// Identification Document DTO
/// </summary>
public class IdentificationDocumentDto
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public int DocumentType { get; set; }
    public string Description { get; set; } = string.Empty;

    // File info
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string? FileHash { get; set; }

    // Metadata
    public DateTime? DocumentIssuedDate { get; set; }
    public DateTime? DocumentExpiryDate { get; set; }
    public string? IssuingAuthority { get; set; }
    public string? DocumentReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsExpired { get; set; }

    // Audit
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}
