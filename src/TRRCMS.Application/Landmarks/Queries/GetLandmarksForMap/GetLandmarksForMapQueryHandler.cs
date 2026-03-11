using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Landmarks.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Landmarks.Queries.GetLandmarksForMap;

public class GetLandmarksForMapQueryHandler : IRequestHandler<GetLandmarksForMapQuery, List<LandmarkMapDto>>
{
    private readonly ILandmarkRepository _landmarkRepository;
    private readonly IMapper _mapper;

    public GetLandmarksForMapQueryHandler(
        ILandmarkRepository landmarkRepository,
        IMapper mapper)
    {
        _landmarkRepository = landmarkRepository;
        _mapper = mapper;
    }

    public async Task<List<LandmarkMapDto>> Handle(GetLandmarksForMapQuery request, CancellationToken cancellationToken)
    {
        LandmarkType? typeFilter = request.Type.HasValue ? (LandmarkType)request.Type.Value : null;

        var landmarks = await _landmarkRepository.GetInBoundingBoxAsync(
            request.NorthEastLat, request.NorthEastLng,
            request.SouthWestLat, request.SouthWestLng,
            typeFilter,
            cancellationToken);

        return _mapper.Map<List<LandmarkMapDto>>(landmarks);
    }
}
