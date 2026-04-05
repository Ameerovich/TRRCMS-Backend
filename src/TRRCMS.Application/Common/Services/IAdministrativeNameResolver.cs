namespace TRRCMS.Application.Common.Services;

/// <summary>
/// Resolves administrative hierarchy codes to their Arabic display names.
/// </summary>
public interface IAdministrativeNameResolver
{
    /// <summary>
    /// Look up Governorate, District, SubDistrict, Community, and Neighborhood
    /// names by their hierarchical codes.  Returns empty strings for any level
    /// whose code is not found in the database.
    /// </summary>
    Task<AdministrativeNames> ResolveAsync(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        string neighborhoodCode,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resolved administrative hierarchy names (Arabic).
/// </summary>
public record AdministrativeNames(
    string GovernorateName,
    string DistrictName,
    string SubDistrictName,
    string CommunityName,
    string NeighborhoodName);
