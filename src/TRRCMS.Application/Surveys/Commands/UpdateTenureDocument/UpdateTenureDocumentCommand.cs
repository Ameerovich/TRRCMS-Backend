using MediatR;
using Microsoft.AspNetCore.Http;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UpdateTenureDocument;

/// <summary>
/// Command to update a tenure/ownership document
/// Mirrors UploadTenureDocumentCommand fields (all nullable for partial update)
/// </summary>
public class UpdateTenureDocumentCommand : IRequest<EvidenceDto>
{
    /// <summary>
    /// Survey ID for authorization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Evidence ID to update (from route)
    /// </summary>
    public Guid EvidenceId { get; set; }

    /// <summary>
    /// Person-Property relation ID to re-link document to (optional)
    /// </summary>
    public Guid? PersonPropertyRelationId { get; set; }

    /// <summary>
    /// Replacement document file (optional - only if re-uploading)
    /// </summary>
    public IFormFile? File { get; set; }

    /// <summary>
    /// Evidence type - OwnershipDeed=2, RentalContract=3, InheritanceDocument=8, etc.
    /// </summary>
    public int? EvidenceType { get; set; }

    /// <summary>
    /// Document description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Document issue date
    /// </summary>
    public DateTime? DocumentIssuedDate { get; set; }

    /// <summary>
    /// Document expiry date
    /// </summary>
    public DateTime? DocumentExpiryDate { get; set; }

    /// <summary>
    /// Issuing authority
    /// </summary>
    public string? IssuingAuthority { get; set; }

    /// <summary>
    /// Document reference/registration number
    /// </summary>
    public string? DocumentReferenceNumber { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }
}
