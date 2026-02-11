using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Neighborhoods.Dtos;

namespace TRRCMS.Application.Neighborhoods.Queries.GetNeighborhoodByCode;

/// <summary>
/// Handler for GetNeighborhoodByCodeQuery.
/// Supports lookup by full code or individual hierarchy codes.
/// </summary>
public class GetNeighborhoodByCodeQueryHandler
    : IRequestHandler<GetNeighborhoodByCodeQuery, NeighborhoodDto?>
{
    private readonly INeighborhoodRepository _neighborhoodRepository;

    public GetNeighborhoodByCodeQueryHandler(INeighborhoodRepository neighborhoodRepository)
    {
        _neighborhoodRepository = neighborhoodRepository
            ?? throw new ArgumentNullException(nameof(neighborhoodRepository));
    }

    public async Task<NeighborhoodDto?> Handle(
        GetNeighborhoodByCodeQuery request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Neighborhood? neighborhood;

        if (!string.IsNullOrWhiteSpace(request.FullCode))
        {
            neighborhood = await _neighborhoodRepository.GetByFullCodeAsync(
                request.FullCode, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.GovernorateCode) &&
                 !string.IsNullOrWhiteSpace(request.DistrictCode) &&
                 !string.IsNullOrWhiteSpace(request.SubDistrictCode) &&
                 !string.IsNullOrWhiteSpace(request.CommunityCode) &&
                 !string.IsNullOrWhiteSpace(request.NeighborhoodCode))
        {
            neighborhood = await _neighborhoodRepository.GetByCodeAsync(
                request.GovernorateCode,
                request.DistrictCode,
                request.SubDistrictCode,
                request.CommunityCode,
                request.NeighborhoodCode,
                cancellationToken);
        }
        else
        {
            return null;
        }

        if (neighborhood == null)
            return null;

        return new NeighborhoodDto
        {
            Id = neighborhood.Id,
            GovernorateCode = neighborhood.GovernorateCode,
            DistrictCode = neighborhood.DistrictCode,
            SubDistrictCode = neighborhood.SubDistrictCode,
            CommunityCode = neighborhood.CommunityCode,
            NeighborhoodCode = neighborhood.NeighborhoodCode,
            FullCode = neighborhood.FullCode,
            NameArabic = neighborhood.NameArabic,
            NameEnglish = neighborhood.NameEnglish,
            CenterLatitude = neighborhood.CenterLatitude,
            CenterLongitude = neighborhood.CenterLongitude,
            BoundaryWkt = neighborhood.BoundaryWkt,
            AreaSquareKm = neighborhood.AreaSquareKm,
            ZoomLevel = neighborhood.ZoomLevel,
            IsActive = neighborhood.IsActive
        };
    }
}
