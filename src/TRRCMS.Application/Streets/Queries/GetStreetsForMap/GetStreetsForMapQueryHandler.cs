using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Streets.Dtos;

namespace TRRCMS.Application.Streets.Queries.GetStreetsForMap;

public class GetStreetsForMapQueryHandler : IRequestHandler<GetStreetsForMapQuery, List<StreetMapDto>>
{
    private readonly IStreetRepository _streetRepository;
    private readonly IMapper _mapper;

    public GetStreetsForMapQueryHandler(
        IStreetRepository streetRepository,
        IMapper mapper)
    {
        _streetRepository = streetRepository;
        _mapper = mapper;
    }

    public async Task<List<StreetMapDto>> Handle(GetStreetsForMapQuery request, CancellationToken cancellationToken)
    {
        var streets = await _streetRepository.GetInBoundingBoxAsync(
            request.NorthEastLat, request.NorthEastLng,
            request.SouthWestLat, request.SouthWestLng,
            cancellationToken);

        return _mapper.Map<List<StreetMapDto>>(streets);
    }
}
