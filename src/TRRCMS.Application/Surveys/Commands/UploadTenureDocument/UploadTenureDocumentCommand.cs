using MediatR;
using Microsoft.AspNetCore.Http;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UploadTenureDocument;

/// <summary>
/// Command to upload tenure/ownership document
/// Links to PersonPropertyRelation for ownership/tenancy proof
/// </summary>
public class UploadTenureDocumentCommand : IRequest<EvidenceDto>
{
    /// <summary>
    /// Survey ID for authorization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Person-Property relation ID to link document to
    /// </summary>
    public Guid PersonPropertyRelationId { get; set; }

    /// <summary>
    /// Document file to upload (deed, rental contract, etc.)
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Document description (e.g., "Property Deed", "Rental Contract", "Inheritance Document")
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Document issue date (optional)
    /// </summary>
    public DateTime? DocumentIssuedDate { get; set; }

    /// <summary>
    /// Document expiry date (optional, for temporary documents)
    /// </summary>
    public DateTime? DocumentExpiryDate { get; set; }

    /// <summary>
    /// Issuing authority (e.g., "Real Estate Registry", "Municipality", "Court")
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