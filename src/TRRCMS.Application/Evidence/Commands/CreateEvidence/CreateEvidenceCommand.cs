using MediatR;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Evidences.Commands.CreateEvidence;

/// <summary>
/// Command to create a new evidence (document/photo/file)
/// </summary>
public class CreateEvidenceCommand : IRequest<EvidenceDto>
{
    // ==================== REQUIRED FIELDS ====================

    /// <summary>
    /// Evidence type (required) - controlled vocabulary
    /// Example: "ID Document", "Property Deed", "Contract", "Photograph", etc.
    /// </summary>
    public string EvidenceType { get; set; } = string.Empty;

    /// <summary>
    /// Document or evidence description (required)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Original filename as uploaded (required)
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// File path in storage system (required)
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes (required)
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// MIME type (required) - e.g., image/jpeg, application/pdf
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// User creating this evidence (required)
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    // ==================== OPTIONAL FIELDS ====================

    /// <summary>
    /// SHA-256 hash of the file for integrity verification (optional)
    /// </summary>
    public string? FileHash { get; set; }

    /// <summary>
    /// Date when the document was issued (optional)
    /// For official documents like IDs, deeds, contracts
    /// </summary>
    public DateTime? DocumentIssuedDate { get; set; }

    /// <summary>
    /// Date when the document expires (optional)
    /// For IDs, permits, temporary documents
    /// </summary>
    public DateTime? DocumentExpiryDate { get; set; }

    /// <summary>
    /// Issuing authority or organization (optional)
    /// </summary>
    public string? IssuingAuthority { get; set; }

    /// <summary>
    /// Document reference number (optional)
    /// </summary>
    public string? DocumentReferenceNumber { get; set; }

    /// <summary>
    /// Additional notes (optional)
    /// </summary>
    public string? Notes { get; set; }

    // ==================== OPTIONAL LINKING ====================

    /// <summary>
    /// Link to Person (optional) - if evidence is for a person's ID
    /// </summary>
    public Guid? PersonId { get; set; }

    /// <summary>
    /// Link to PersonPropertyRelation (optional) - if evidence supports a relation
    /// </summary>
    public Guid? PersonPropertyRelationId { get; set; }

    /// <summary>
    /// Link to Claim (optional) - if evidence supports a claim
    /// </summary>
    public Guid? ClaimId { get; set; }
}
