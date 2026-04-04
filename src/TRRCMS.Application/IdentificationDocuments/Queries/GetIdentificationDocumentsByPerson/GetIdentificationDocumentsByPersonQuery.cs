using MediatR;
using TRRCMS.Application.IdentificationDocuments.Dtos;

namespace TRRCMS.Application.IdentificationDocuments.Queries.GetIdentificationDocumentsByPerson;

/// <summary>
/// Query to get all identification documents for a specific person.
/// GET /api/v1/persons/{personId}/identification-documents
/// </summary>
public record GetIdentificationDocumentsByPersonQuery(Guid PersonId)
    : IRequest<List<IdentificationDocumentDto>>;
