using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Buildings.Queries.SearchBuildings;

/// <summary>
/// Handler for SearchBuildingsQuery
/// </summary>
public class SearchBuildingsQueryHandler : IRequestHandler<SearchBuildingsQuery, SearchBuildingsResponse>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICommunityRepository _communityRepository;
    private readonly IMapper _mapper;

    public SearchBuildingsQueryHandler(
        IBuildingRepository buildingRepository,
        ICommunityRepository communityRepository,
        IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _communityRepository = communityRepository;
        _mapper = mapper;
    }

    public async Task<SearchBuildingsResponse> Handle(
        SearchBuildingsQuery request,
        CancellationToken cancellationToken)
    {
        // Validate pagination
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = PagedQuery.ClampPageSize(request.PageSize);

        // Normalize OCHA pCode filters → raw numeric codes.
        var (govCode, distCode, subDistCode) = OchaCommandNormalizer.ResolveAdmCodes(
            request.GovernorateCode, request.DistrictCode, request.SubDistrictCode,
            request.GovernoratePCode, request.DistrictPCode, request.SubDistrictPCode);
        var neighCode = OchaCommandNormalizer.ResolveNeighborhoodCode(
            request.NeighborhoodCode, request.NeighborhoodPCode);

        var commCode = request.CommunityCode;
        var commPCodeNorm = OchaCommandNormalizer.NormalizeCommunityPCode(request.CommunityPCode);
        if (commPCodeNorm != null)
        {
            var matched = await _communityRepository.GetByExternalPCodeAsync(
                commPCodeNorm,
                string.IsNullOrWhiteSpace(govCode) ? null : govCode,
                string.IsNullOrWhiteSpace(distCode) ? null : distCode,
                string.IsNullOrWhiteSpace(subDistCode) ? null : subDistCode,
                cancellationToken);

            // OCHA C-codes are flat IDs; the same Community.Code can repeat under different
            // parents. Pin the parent hierarchy to the matched community so the filter is exact.
            if (matched != null)
            {
                commCode = matched.Code;
                govCode = matched.GovernorateCode;
                distCode = matched.DistrictCode;
                subDistCode = matched.SubDistrictCode;
            }
            else
            {
                // No community matches the supplied OCHA pCode → no buildings can match.
                return new SearchBuildingsResponse
                {
                    Buildings = new List<BuildingDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }
        }

        // Get filtered and paginated buildings
        var (buildings, totalCount) = await _buildingRepository.SearchBuildingsAsync(
            governorateCode: string.IsNullOrWhiteSpace(govCode) ? null : govCode,
            districtCode: string.IsNullOrWhiteSpace(distCode) ? null : distCode,
            subDistrictCode: string.IsNullOrWhiteSpace(subDistCode) ? null : subDistCode,
            communityCode: string.IsNullOrWhiteSpace(commCode) ? null : commCode,
            neighborhoodCode: string.IsNullOrWhiteSpace(neighCode) ? null : neighCode,
            buildingId: request.BuildingId,
            buildingNumber: request.BuildingNumber,
            latitude: null,         // Not used for search
            longitude: null,        // Not used for search
            radiusMeters: null,     // Not used in current frontend
            status: request.Status,
            buildingType: request.BuildingType,
            page: page,
            pageSize: pageSize,
            sortBy: request.SortBy,
            sortDescending: request.SortDescending,
            cancellationToken: cancellationToken);

        // Map to DTOs (AutoMapper sets synthetic CommunityPCode = "C" + Code by default).
        var buildingDtos = _mapper.Map<List<BuildingDto>>(buildings);

        // Enrich CommunityPCode with the real Community.ExternalPCode where available.
        // Batch-load distinct communities referenced by this page of results.
        var distinctCommunityKeys = buildings
            .Select(b => (b.GovernorateCode, b.DistrictCode, b.SubDistrictCode, b.CommunityCode))
            .Distinct()
            .ToList();
        var pCodeByKey = new Dictionary<(string, string, string, string), string?>();
        foreach (var key in distinctCommunityKeys)
        {
            var c = await _communityRepository.GetByCodeAsync(
                key.GovernorateCode, key.DistrictCode, key.SubDistrictCode, key.CommunityCode,
                cancellationToken);
            pCodeByKey[key] = c?.ExternalPCode;
        }
        for (var i = 0; i < buildings.Count; i++)
        {
            var b = buildings[i];
            var key = (b.GovernorateCode, b.DistrictCode, b.SubDistrictCode, b.CommunityCode);
            if (pCodeByKey.TryGetValue(key, out var ext) && !string.IsNullOrWhiteSpace(ext))
            {
                buildingDtos[i].CommunityPCode = ext;
            }
        }

        return new SearchBuildingsResponse
        {
            Buildings = buildingDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}