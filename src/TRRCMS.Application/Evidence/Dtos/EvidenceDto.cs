namespace TRRCMS.Application.Evidences.Dtos;

/// <summary>
/// DTO for Evidence entity
/// </summary>
public class EvidenceDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// نوع الدليل - Returned as integer: IdentificationDocument=1, OwnershipDeed=2, RentalContract=3, etc.
    /// </summary>
    public int EvidenceType { get; set; }
    public string Description { get; set; } = string.Empty;

    // File information
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

    // Versioning
    public int VersionNumber { get; set; }
    public Guid? PreviousVersionId { get; set; }
    public bool IsCurrentVersion { get; set; }

    // Relationships
    public Guid? PersonId { get; set; }
    public Guid? ClaimId { get; set; }

    /// <summary>
    /// Many-to-many links to PersonPropertyRelations via EvidenceRelation join entity
    /// </summary>
    public List<EvidenceRelationDto>? EvidenceRelations { get; set; }

    // Audit fields
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }

    // Computed
    public bool IsExpired { get; set; }
}
