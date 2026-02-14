using MediatR;
using Microsoft.AspNetCore.Http;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UpdateIdentificationDocument;

/// <summary>
/// Command to update an identification document
/// Mirrors UploadIdentificationDocumentCommand fields (all nullable for partial update)
/// </summary>
public class UpdateIdentificationDocumentCommand : IRequest<EvidenceDto>
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
    /// Person ID to re-link document to (optional - for changing the linked person)
    /// </summary>
    public Guid? PersonId { get; set; }

    /// <summary>
    /// Replacement document file (optional - only if re-uploading)
    /// </summary>
    public IFormFile? File { get; set; }

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
    /// Document reference/ID number
    /// </summary>
    public string? DocumentReferenceNumber { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }
}
