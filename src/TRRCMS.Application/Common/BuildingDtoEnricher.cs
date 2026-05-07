using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Common;

/// <summary>
/// Post-AutoMapper enrichment helpers that fill in fields the static mapper
/// cannot resolve — currently <see cref="BuildingDto.CommunityPCode"/>, which
/// requires an async DB lookup against <c>Community.ExternalPCode</c>.
/// </summary>
public static class BuildingDtoEnricher
{
    /// <summary>
    /// For each DTO, replace the synthetic <c>CommunityPCode</c> ("Cxxx")
    /// with the real OCHA value (<c>Community.ExternalPCode</c>, e.g. "C1007")
    /// when the underlying community row has one. Communities are batch-fetched
    /// once per distinct (gov, dist, subDist, comm) tuple so the enrichment
    /// stays cheap on paged lists.
    /// </summary>
    public static async Task EnrichCommunityPCodesAsync(
        IList<BuildingDto> dtos,
        ICommunityRepository communityRepository,
        CancellationToken cancellationToken)
    {
        if (dtos.Count == 0) return;

        var distinctKeys = dtos
            .Select(d => (d.GovernorateCode, d.DistrictCode, d.SubDistrictCode, d.CommunityCode))
            .Where(k => !string.IsNullOrWhiteSpace(k.GovernorateCode)
                     && !string.IsNullOrWhiteSpace(k.DistrictCode)
                     && !string.IsNullOrWhiteSpace(k.SubDistrictCode)
                     && !string.IsNullOrWhiteSpace(k.CommunityCode))
            .Distinct()
            .ToList();

        var pCodeByKey = new Dictionary<(string, string, string, string), string?>();
        foreach (var key in distinctKeys)
        {
            var c = await communityRepository.GetByCodeAsync(
                key.GovernorateCode, key.DistrictCode, key.SubDistrictCode, key.CommunityCode,
                cancellationToken);
            pCodeByKey[key] = c?.ExternalPCode;
        }

        foreach (var dto in dtos)
        {
            var key = (dto.GovernorateCode, dto.DistrictCode, dto.SubDistrictCode, dto.CommunityCode);
            if (pCodeByKey.TryGetValue(key, out var ext) && !string.IsNullOrWhiteSpace(ext))
            {
                dto.CommunityPCode = ext;
            }
        }
    }

    /// <summary>
    /// Convenience overload for the single-row case.
    /// </summary>
    public static Task EnrichCommunityPCodesAsync(
        BuildingDto dto,
        ICommunityRepository communityRepository,
        CancellationToken cancellationToken)
        => EnrichCommunityPCodesAsync(new[] { dto }, communityRepository, cancellationToken);
}
