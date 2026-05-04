using MediatR;
using TRRCMS.Application.Common;
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
    private readonly ICommunityRepository _communityRepository;

    public GetNeighborhoodByCodeQueryHandler(
        INeighborhoodRepository neighborhoodRepository,
        ICommunityRepository communityRepository)
    {
        _neighborhoodRepository = neighborhoodRepository
            ?? throw new ArgumentNullException(nameof(neighborhoodRepository));
        _communityRepository = communityRepository
            ?? throw new ArgumentNullException(nameof(communityRepository));
    }

    public async Task<NeighborhoodDto?> Handle(
        GetNeighborhoodByCodeQuery request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Neighborhood? neighborhood;

        // Normalize OCHA pCodes (when provided) to raw numeric codes.
        var (govCode, distCode, subDistCode) = OchaCommandNormalizer.ResolveAdmCodes(
            request.GovernorateCode, request.DistrictCode, request.SubDistrictCode,
            request.GovernoratePCode, request.DistrictPCode, request.SubDistrictPCode);
        var neighCode = OchaCommandNormalizer.ResolveNeighborhoodCode(
            request.NeighborhoodCode, request.NeighborhoodPCode);

        var commCode = request.CommunityCode ?? string.Empty;
        var commPCodeNorm = OchaCommandNormalizer.NormalizeCommunityPCode(request.CommunityPCode);
        if (commPCodeNorm != null)
        {
            var matched = await _communityRepository.GetByExternalPCodeAsync(
                commPCodeNorm, govCode, distCode, subDistCode, cancellationToken);
            if (matched != null)
            {
                // Pin the parent hierarchy from the matched row — OCHA C-codes are flat IDs.
                commCode = matched.Code;
                govCode = matched.GovernorateCode;
                distCode = matched.DistrictCode;
                subDistCode = matched.SubDistrictCode;
            }
            else
            {
                return null;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.FullCode))
        {
            neighborhood = await _neighborhoodRepository.GetByFullCodeAsync(
                request.FullCode, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(govCode) &&
                 !string.IsNullOrWhiteSpace(distCode) &&
                 !string.IsNullOrWhiteSpace(subDistCode) &&
                 !string.IsNullOrWhiteSpace(commCode) &&
                 !string.IsNullOrWhiteSpace(neighCode))
        {
            neighborhood = await _neighborhoodRepository.GetByCodeAsync(
                govCode, distCode, subDistCode, commCode, neighCode,
                cancellationToken);
        }
        else
        {
            return null;
        }

        if (neighborhood == null)
            return null;

        // Resolve community ExternalPCode (real OCHA C-code) for the response.
        var communityRow = await _communityRepository.GetByCodeAsync(
            neighborhood.GovernorateCode, neighborhood.DistrictCode, neighborhood.SubDistrictCode,
            neighborhood.CommunityCode, cancellationToken);
        var communityExternalPCode = communityRow?.ExternalPCode;

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
            IsActive = neighborhood.IsActive,
            GovernoratePCode = OchaPCodeConverter.ToGovPCode(neighborhood.GovernorateCode),
            DistrictPCode = OchaPCodeConverter.ToDistrictPCode(neighborhood.GovernorateCode, neighborhood.DistrictCode),
            SubDistrictPCode = OchaPCodeConverter.ToSubDistrictPCode(neighborhood.GovernorateCode, neighborhood.DistrictCode, neighborhood.SubDistrictCode),
            CommunityPCode = OchaPCodeConverter.ToCommunityPCode(communityExternalPCode, neighborhood.CommunityCode),
            PCode = OchaPCodeConverter.ToNeighborhoodPCode(neighborhood.NeighborhoodCode)
        };
    }
}
