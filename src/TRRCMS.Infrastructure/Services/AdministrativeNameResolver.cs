using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Resolves administrative hierarchy codes to Arabic names by querying
/// the Governorate → District → SubDistrict → Community → Neighborhood tables.
/// </summary>
public class AdministrativeNameResolver : IAdministrativeNameResolver
{
    private readonly IGovernorateRepository _governorates;
    private readonly IDistrictRepository _districts;
    private readonly ISubDistrictRepository _subDistricts;
    private readonly ICommunityRepository _communities;
    private readonly INeighborhoodRepository _neighborhoods;

    public AdministrativeNameResolver(
        IGovernorateRepository governorates,
        IDistrictRepository districts,
        ISubDistrictRepository subDistricts,
        ICommunityRepository communities,
        INeighborhoodRepository neighborhoods)
    {
        _governorates = governorates;
        _districts = districts;
        _subDistricts = subDistricts;
        _communities = communities;
        _neighborhoods = neighborhoods;
    }

    public async Task<AdministrativeNames> ResolveAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        string neighborhoodCode,
        CancellationToken cancellationToken = default)
    {
        // Run sequentially — all repositories share the same scoped DbContext
        var gov = await _governorates.GetByCodeAsync(governorateCode, cancellationToken);
        var dist = await _districts.GetByCodeAsync(governorateCode, districtCode, cancellationToken);
        var subDist = await _subDistricts.GetByCodeAsync(governorateCode, districtCode, subDistrictCode, cancellationToken);
        var comm = await _communities.GetByCodeAsync(governorateCode, districtCode, subDistrictCode, communityCode, cancellationToken);
        var neigh = await _neighborhoods.GetByCodeAsync(governorateCode, districtCode, subDistrictCode, communityCode, neighborhoodCode, cancellationToken);

        return new AdministrativeNames(
            GovernorateName: gov?.NameArabic ?? string.Empty,
            DistrictName: dist?.NameArabic ?? string.Empty,
            SubDistrictName: subDist?.NameArabic ?? string.Empty,
            CommunityName: comm?.NameArabic ?? string.Empty,
            NeighborhoodName: neigh?.NameArabic ?? string.Empty);
    }
}
