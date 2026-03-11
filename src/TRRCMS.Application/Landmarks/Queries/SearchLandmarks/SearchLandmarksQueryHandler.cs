using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Landmarks.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Landmarks.Queries.SearchLandmarks;

public class SearchLandmarksQueryHandler : IRequestHandler<SearchLandmarksQuery, List<LandmarkDto>>
{
    private readonly ILandmarkRepository _landmarkRepository;
    private readonly IMapper _mapper;

    public SearchLandmarksQueryHandler(
        ILandmarkRepository landmarkRepository,
        IMapper mapper)
    {
        _landmarkRepository = landmarkRepository;
        _mapper = mapper;
    }

    public async Task<List<LandmarkDto>> Handle(SearchLandmarksQuery request, CancellationToken cancellationToken)
    {
        LandmarkType? typeFilter = request.Type.HasValue ? (LandmarkType)request.Type.Value : null;

        var landmarks = await _landmarkRepository.SearchByNameAsync(
            request.Query,
            typeFilter,
            request.MaxResults,
            cancellationToken);

        return _mapper.Map<List<LandmarkDto>>(landmarks);
    }
}
