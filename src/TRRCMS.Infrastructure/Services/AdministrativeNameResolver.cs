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
        // Run all lookups concurrently — they are independent
        var govTask = _governorates.GetByCodeAsync(governorateCode, cancellationToken);
        var distTask = _districts.GetByCodeAsync(governorateCode, districtCode, cancellationToken);
        var subDistTask = _subDistricts.GetByCodeAsync(governorateCode, districtCode, subDistrictCode, cancellationToken);
        var commTask = _communities.GetByCodeAsync(governorateCode, districtCode, subDistrictCode, communityCode, cancellationToken);
        var neighTask = _neighborhoods.GetByCodeAsync(governorateCode, districtCode, subDistrictCode, communityCode, neighborhoodCode, cancellationToken);

        await Task.WhenAll(govTask, distTask, subDistTask, commTask, neighTask);

        return new AdministrativeNames(
            GovernorateName: govTask.Result?.NameArabic ?? string.Empty,
            DistrictName: distTask.Result?.NameArabic ?? string.Empty,
            SubDistrictName: subDistTask.Result?.NameArabic ?? string.Empty,
            CommunityName: commTask.Result?.NameArabic ?? string.Empty,
            NeighborhoodName: neighTask.Result?.NameArabic ?? string.Empty);
    }
}
