using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Neighborhoods.Dtos;

namespace TRRCMS.Application.Neighborhoods.Queries.GetNeighborhoods;

/// <summary>
/// Handler for GetNeighborhoodsQuery.
/// Returns all active neighborhoods mapped to DTOs.
/// </summary>
public class GetNeighborhoodsQueryHandler
    : IRequestHandler<GetNeighborhoodsQuery, List<NeighborhoodDto>>
{
    private readonly INeighborhoodRepository _neighborhoodRepository;

    public GetNeighborhoodsQueryHandler(INeighborhoodRepository neighborhoodRepository)
    {
        _neighborhoodRepository = neighborhoodRepository
            ?? throw new ArgumentNullException(nameof(neighborhoodRepository));
    }

    public async Task<List<NeighborhoodDto>> Handle(
        GetNeighborhoodsQuery request,
        CancellationToken cancellationToken)
    {
        var neighborhoods = await _neighborhoodRepository.GetAllAsync(
            governorateCode: request.GovernorateCode,
            districtCode: request.DistrictCode,
            subDistrictCode: request.SubDistrictCode,
            communityCode: request.CommunityCode,
            cancellationToken: cancellationToken);

        return neighborhoods.Select(n => new NeighborhoodDto
        {
            Id = n.Id,
            GovernorateCode = n.GovernorateCode,
            DistrictCode = n.DistrictCode,
            SubDistrictCode = n.SubDistrictCode,
            CommunityCode = n.CommunityCode,
            NeighborhoodCode = n.NeighborhoodCode,
            FullCode = n.FullCode,
            NameArabic = n.NameArabic,
            NameEnglish = n.NameEnglish,
            CenterLatitude = n.CenterLatitude,
            CenterLongitude = n.CenterLongitude,
            BoundaryWkt = n.BoundaryWkt,
            AreaSquareKm = n.AreaSquareKm,
            ZoomLevel = n.ZoomLevel,
            IsActive = n.IsActive
        }).ToList();
    }
}
