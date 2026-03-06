using MediatR;
using TRRCMS.Application.Buildings.Dtos;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingDocument;

/// <summary>
/// Query to get a building document by its ID.
/// </summary>
public record GetBuildingDocumentQuery(Guid Id) : IRequest<BuildingDocumentDto?>;
