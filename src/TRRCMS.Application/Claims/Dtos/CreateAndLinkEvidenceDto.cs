namespace TRRCMS.Application.Claims.Dtos;

/// <summary>
/// DTO for creating a new evidence record and linking it to the claim's source relation.
/// Used within UpdateClaimCommand.NewEvidence collection.
/// </summary>
public class CreateAndLinkEvidenceDto
{
    /// <summary>
    /// Evidence type enum value (e.g., OwnershipDeed=2, RentalContract=3).
    /// </summary>
    public int EvidenceType { get; set; }

    public string Description { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string? FileHash { get; set; }

    /// <summary>
    /// Reason for linking this evidence to the relation.
    /// </summary>
    public string? LinkReason { get; set; }

    // Optional document metadata
    public DateTime? DocumentIssuedDate { get; set; }
    public DateTime? DocumentExpiryDate { get; set; }
    public string? IssuingAuthority { get; set; }
    public string? DocumentReferenceNumber { get; set; }
}
