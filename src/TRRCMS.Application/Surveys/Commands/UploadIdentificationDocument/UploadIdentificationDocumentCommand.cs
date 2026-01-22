using MediatR;
using Microsoft.AspNetCore.Http;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UploadIdentificationDocument;

/// <summary>
/// Command to upload identification document for a person
/// Links document directly to Person entity
/// </summary>
public class UploadIdentificationDocumentCommand : IRequest<EvidenceDto>
{
    /// <summary>
    /// Survey ID for authorization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Person ID to link document to
    /// </summary>
    public Guid PersonId { get; set; }

    /// <summary>
    /// Document file to upload (ID card, passport, etc.)
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Document description (e.g., "National ID Card", "Passport", "Birth Certificate")
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Document issue date (optional)
    /// </summary>
    public DateTime? DocumentIssuedDate { get; set; }

    /// <summary>
    /// Document expiry date (optional)
    /// </summary>
    public DateTime? DocumentExpiryDate { get; set; }

    /// <summary>
    /// Issuing authority (e.g., "Ministry of Interior", "Passport Office")
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