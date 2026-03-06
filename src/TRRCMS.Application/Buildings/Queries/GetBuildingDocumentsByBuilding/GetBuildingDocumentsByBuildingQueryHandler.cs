using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingDocumentsByBuilding;

/// <summary>
/// Handler for GetBuildingDocumentsByBuildingQuery.
/// </summary>
public class GetBuildingDocumentsByBuildingQueryHandler : IRequestHandler<GetBuildingDocumentsByBuildingQuery, List<BuildingDocumentDto>>
{
    private readonly IBuildingDocumentRepository _repository;
    private readonly IMapper _mapper;

    public GetBuildingDocumentsByBuildingQueryHandler(
        IBuildingDocumentRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<BuildingDocumentDto>> Handle(GetBuildingDocumentsByBuildingQuery request, CancellationToken cancellationToken)
    {
        var documents = await _repository.GetByBuildingIdAsync(request.BuildingId, cancellationToken);
        return _mapper.Map<List<BuildingDocumentDto>>(documents);
    }
}
