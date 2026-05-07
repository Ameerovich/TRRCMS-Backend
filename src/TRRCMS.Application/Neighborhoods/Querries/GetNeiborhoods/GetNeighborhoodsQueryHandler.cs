using MediatR;
using TRRCMS.Application.Common;
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
    private readonly ICommunityRepository _communityRepository;

    public GetNeighborhoodsQueryHandler(
        INeighborhoodRepository neighborhoodRepository,
        ICommunityRepository communityRepository)
    {
        _neighborhoodRepository = neighborhoodRepository
            ?? throw new ArgumentNullException(nameof(neighborhoodRepository));
        _communityRepository = communityRepository
            ?? throw new ArgumentNullException(nameof(communityRepository));
    }

    public async Task<List<NeighborhoodDto>> Handle(
        GetNeighborhoodsQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize OCHA pCode filters to raw numeric components.
        var (govCode, distCode, subDistCode) = OchaCommandNormalizer.ResolveAdmCodes(
            request.GovernorateCode, request.DistrictCode, request.SubDistrictCode,
            request.GovernoratePCode, request.DistrictPCode, request.SubDistrictPCode);

        var commCode = request.CommunityCode;
        var commPCodeNorm = OchaCommandNormalizer.NormalizeCommunityPCode(request.CommunityPCode);
        string? communityExternalPCodeForOutput = null;
        if (commPCodeNorm != null)
        {
            var matched = await _communityRepository.GetByExternalPCodeAsync(
                commPCodeNorm,
                string.IsNullOrWhiteSpace(govCode) ? null : govCode,
                string.IsNullOrWhiteSpace(distCode) ? null : distCode,
                string.IsNullOrWhiteSpace(subDistCode) ? null : subDistCode,
                cancellationToken);

            // The OCHA C-code is a flat ID — the same numeric Community.Code (e.g. "001")
            // exists under many parent hierarchies. Lock the parent codes from the matched
            // row so we don't accidentally pull every "001" community across the governorate.
            if (matched != null)
            {
                commCode = matched.Code;
                govCode = matched.GovernorateCode;
                distCode = matched.DistrictCode;
                subDistCode = matched.SubDistrictCode;
                communityExternalPCodeForOutput = matched.ExternalPCode;
            }
            else
            {
                // The caller asked for a community OCHA code that doesn't exist — return empty.
                return new List<NeighborhoodDto>();
            }
        }

        var neighborhoods = await _neighborhoodRepository.GetAllAsync(
            governorateCode: string.IsNullOrWhiteSpace(govCode) ? null : govCode,
            districtCode: string.IsNullOrWhiteSpace(distCode) ? null : distCode,
            subDistrictCode: string.IsNullOrWhiteSpace(subDistCode) ? null : subDistCode,
            communityCode: string.IsNullOrWhiteSpace(commCode) ? null : commCode,
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
            IsActive = n.IsActive,
            // OCHA P-Codes (additive). Community pCode is synthetic at this layer; the
            // controller / a downstream enrichment step can override with the real
            // Community.ExternalPCode if needed.
            GovernoratePCode = OchaPCodeConverter.ToGovPCode(n.GovernorateCode),
            DistrictPCode = OchaPCodeConverter.ToDistrictPCode(n.GovernorateCode, n.DistrictCode),
            SubDistrictPCode = OchaPCodeConverter.ToSubDistrictPCode(n.GovernorateCode, n.DistrictCode, n.SubDistrictCode),
            CommunityPCode = OchaPCodeConverter.ToCommunityPCode(communityExternalPCodeForOutput, n.CommunityCode),
            PCode = OchaPCodeConverter.ToNeighborhoodPCode(n.NeighborhoodCode)
        }).ToList();
    }
}
