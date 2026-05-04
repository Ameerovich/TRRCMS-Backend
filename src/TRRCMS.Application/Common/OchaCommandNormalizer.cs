namespace TRRCMS.Application.Common;

/// <summary>
/// Helpers for normalizing inbound admin codes when a request may carry either
/// raw numeric components (legacy) or OCHA P-Codes (the Excel convention).
///
/// All Resolve* methods follow the same precedence:
///   1. If a pCode is provided, parse it via <see cref="OchaPCodeConverter"/>
///      and prefer the parsed values.
///   2. Otherwise fall back to the raw numeric components.
///   3. Empty inputs round-trip as empty strings — validators decide whether
///      they are required.
/// </summary>
public static class OchaCommandNormalizer
{
    /// <summary>
    /// Resolve governorate / district / sub-district. Each pCode is independent —
    /// callers may supply governoratePCode + raw districtCode etc.
    /// </summary>
    public static (string Governorate, string District, string SubDistrict) ResolveAdmCodes(
        string? governorateCode,
        string? districtCode,
        string? subDistrictCode,
        string? governoratePCode = null,
        string? districtPCode = null,
        string? subDistrictPCode = null)
    {
        var gov = governorateCode ?? string.Empty;
        var dist = districtCode ?? string.Empty;
        var sub = subDistrictCode ?? string.Empty;

        // Sub-district pCode (SY020000) populates all three when present.
        if (!string.IsNullOrWhiteSpace(subDistrictPCode)
            && OchaPCodeConverter.TryParseSubDistrictPCode(subDistrictPCode, out var g, out var d, out var s))
        {
            gov = g;
            dist = d;
            sub = s;
        }
        // District pCode (SY0200) populates gov + dist.
        else if (!string.IsNullOrWhiteSpace(districtPCode)
                 && OchaPCodeConverter.TryParseAdmPCode(districtPCode, out var g2, out var d2, out _)
                 && d2 != null)
        {
            gov = g2;
            dist = d2;
        }
        // Governorate pCode (SY02) populates gov only.
        else if (!string.IsNullOrWhiteSpace(governoratePCode)
                 && OchaPCodeConverter.TryParseAdmPCode(governoratePCode, out var g3, out _, out _))
        {
            gov = g3;
        }

        return (gov, dist, sub);
    }

    /// <summary>
    /// Resolve neighborhood code: prefer parsed pCode (N0xxx), fall back to raw 3-digit code.
    /// </summary>
    public static string ResolveNeighborhoodCode(string? neighborhoodCode, string? neighborhoodPCode)
    {
        if (!string.IsNullOrWhiteSpace(neighborhoodPCode)
            && OchaPCodeConverter.TryParseNeighborhoodPCode(neighborhoodPCode, out var parsed))
        {
            return parsed;
        }
        return neighborhoodCode ?? string.Empty;
    }

    /// <summary>
    /// Normalize a community OCHA pCode to its canonical form (e.g. "C1007").
    /// Returns null when neither value parses; callers can then fall back to a 3-digit lookup.
    /// </summary>
    public static string? NormalizeCommunityPCode(string? communityPCode)
    {
        if (!string.IsNullOrWhiteSpace(communityPCode)
            && OchaPCodeConverter.TryNormalizeCommunityPCode(communityPCode, out var normalized))
        {
            return normalized;
        }
        return null;
    }
}
