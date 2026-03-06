using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingDocument;

/// <summary>
/// Handler for GetBuildingDocumentQuery.
/// </summary>
public class GetBuildingDocumentQueryHandler : IRequestHandler<GetBuildingDocumentQuery, BuildingDocumentDto?>
{
    private readonly IBuildingDocumentRepository _repository;
    private readonly IMapper _mapper;

    public GetBuildingDocumentQueryHandler(
        IBuildingDocumentRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<BuildingDocumentDto?> Handle(GetBuildingDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return document == null ? null : _mapper.Map<BuildingDocumentDto>(document);
    }
}
