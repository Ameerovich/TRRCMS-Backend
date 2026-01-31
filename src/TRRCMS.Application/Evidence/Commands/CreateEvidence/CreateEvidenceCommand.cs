using MediatR;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Evidences.Commands.CreateEvidence;

/// <summary>
/// Command to create evidence (document/photo)
/// </summary>
public class CreateEvidenceCommand : IRequest<EvidenceDto>
{
    /// <summary>
    /// نوع الدليل - IdentificationDocument=1, OwnershipDeed=2, RentalContract=3, etc.
    /// </summary>
    public EvidenceType EvidenceType { get; set; }

    public string Description { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public string? FileHash { get; set; }

    // Optional links
    public Guid? PersonId { get; set; }
    public Guid? PersonPropertyRelationId { get; set; }
    public Guid? ClaimId { get; set; }

    // Optional metadata
    public DateTime? DocumentIssuedDate { get; set; }
    public DateTime? DocumentExpiryDate { get; set; }
    public string? IssuingAuthority { get; set; }
    public string? DocumentReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
