namespace TRRCMS.Application.Common;

/// <summary>
/// Converts between TRRCMS internal numeric admin codes and UN-OCHA P-Codes.
/// OCHA convention (Aleppo example):
///   Governorate  → SY02
///   District     → SY0200
///   SubDistrict  → SY020000
///   Community    → C1007       (flat global ID, stored on Community.ExternalPCode)
///   Neighborhood → N0160       (N0 + 3-digit numeric)
///
/// Internal storage stays purely numeric (2+2+2+3+3 + 5 = 17-digit BuildingId).
/// This helper formats outbound DTOs and parses inbound requests; it never reads
/// from the database — community pCode lookup must be done by the caller using
/// ICommunityRepository when the inbound C-code is unknown.
/// </summary>
public static class OchaPCodeConverter
{
    public const string CountryPrefix = "SY";
    public const string NeighborhoodPrefix = "N";
    public const string CommunityPrefix = "C";

    private const int GovLen = 2;
    private const int DistLen = 2;
    private const int SubDistLen = 2;
    private const int NeighborhoodLen = 3;

    // ── Outbound formatting ─────────────────────────────────────

    public static string ToGovPCode(string? governorateCode)
        => string.IsNullOrWhiteSpace(governorateCode)
            ? string.Empty
            : CountryPrefix + governorateCode.Trim().PadLeft(GovLen, '0');

    public static string ToDistrictPCode(string? governorateCode, string? districtCode)
        => (string.IsNullOrWhiteSpace(governorateCode) || string.IsNullOrWhiteSpace(districtCode))
            ? string.Empty
            : CountryPrefix
              + governorateCode!.Trim().PadLeft(GovLen, '0')
              + districtCode!.Trim().PadLeft(DistLen, '0');

    public static string ToSubDistrictPCode(string? governorateCode, string? districtCode, string? subDistrictCode)
        => (string.IsNullOrWhiteSpace(governorateCode)
                || string.IsNullOrWhiteSpace(districtCode)
                || string.IsNullOrWhiteSpace(subDistrictCode))
            ? string.Empty
            : CountryPrefix
              + governorateCode!.Trim().PadLeft(GovLen, '0')
              + districtCode!.Trim().PadLeft(DistLen, '0')
              + subDistrictCode!.Trim().PadLeft(SubDistLen, '0');

    /// <summary>
    /// Returns the stored OCHA community pCode when present; otherwise a synthetic
    /// "C{Code}" fallback so callers always get a non-empty pCode for legacy rows
    /// imported before ExternalPCode was introduced.
    /// </summary>
    public static string ToCommunityPCode(string? externalPCode, string? communityCode)
    {
        if (!string.IsNullOrWhiteSpace(externalPCode))
            return externalPCode!.Trim();
        if (string.IsNullOrWhiteSpace(communityCode))
            return string.Empty;
        return CommunityPrefix + communityCode!.Trim();
    }

    public static string ToNeighborhoodPCode(string? neighborhoodCode)
        => string.IsNullOrWhiteSpace(neighborhoodCode)
            ? string.Empty
            : NeighborhoodPrefix + "0" + neighborhoodCode!.Trim().PadLeft(NeighborhoodLen, '0');

    // ── Inbound parsing ─────────────────────────────────────────

    /// <summary>
    /// Parses an OCHA admin pCode into its components. Accepts SY02, SY0200, or SY020000
    /// (case-insensitive, whitespace tolerated). District/subDistrict are null when the
    /// input only encodes a shallower level.
    /// </summary>
    public static bool TryParseAdmPCode(
        string? pCode,
        out string governorateCode,
        out string? districtCode,
        out string? subDistrictCode)
    {
        governorateCode = string.Empty;
        districtCode = null;
        subDistrictCode = null;

        if (string.IsNullOrWhiteSpace(pCode))
            return false;

        var s = pCode!.Trim();
        if (!s.StartsWith(CountryPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var rest = s.Substring(CountryPrefix.Length);
        if (rest.Length == 0 || !rest.All(char.IsDigit))
            return false;

        switch (rest.Length)
        {
            case GovLen:
                governorateCode = rest;
                return true;
            case GovLen + DistLen:
                governorateCode = rest.Substring(0, GovLen);
                districtCode = rest.Substring(GovLen, DistLen);
                return true;
            case GovLen + DistLen + SubDistLen:
                governorateCode = rest.Substring(0, GovLen);
                districtCode = rest.Substring(GovLen, DistLen);
                subDistrictCode = rest.Substring(GovLen + DistLen, SubDistLen);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Strict parse for the full SY020000 form. Returns false unless all three components are present.
    /// </summary>
    public static bool TryParseSubDistrictPCode(
        string? pCode,
        out string governorateCode,
        out string districtCode,
        out string subDistrictCode)
    {
        districtCode = string.Empty;
        subDistrictCode = string.Empty;

        if (!TryParseAdmPCode(pCode, out governorateCode, out var d, out var s))
            return false;

        if (d == null || s == null)
            return false;

        districtCode = d;
        subDistrictCode = s;
        return true;
    }

    /// <summary>
    /// Parses an OCHA neighborhood pCode into a 3-digit code.
    /// Accepts N0160, N160, 0160, or 160 (case-insensitive). When the digit string is
    /// longer than 3 the last 3 digits win; shorter inputs are zero-padded.
    /// </summary>
    public static bool TryParseNeighborhoodPCode(string? pCode, out string neighborhoodCode)
    {
        neighborhoodCode = string.Empty;
        if (string.IsNullOrWhiteSpace(pCode))
            return false;

        var s = pCode!.Trim();
        if (s.StartsWith(NeighborhoodPrefix, StringComparison.OrdinalIgnoreCase))
            s = s.Substring(NeighborhoodPrefix.Length);

        if (s.Length == 0 || !s.All(char.IsDigit))
            return false;

        neighborhoodCode = s.Length >= NeighborhoodLen
            ? s.Substring(s.Length - NeighborhoodLen)
            : s.PadLeft(NeighborhoodLen, '0');
        return true;
    }

    /// <summary>
    /// Normalizes a community pCode (e.g. "c1007 " → "C1007"). Community codes are
    /// flat IDs and are NOT decomposed into hierarchy components.
    /// </summary>
    public static bool TryNormalizeCommunityPCode(string? pCode, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(pCode))
            return false;

        var s = pCode!.Trim().ToUpperInvariant();
        if (!s.StartsWith(CommunityPrefix, StringComparison.Ordinal))
            return false;
        if (s.Length < 2 || !s.Substring(1).All(char.IsDigit))
            return false;

        normalized = s;
        return true;
    }

    // ── Lightweight format probes (for validators / fallback dispatch) ──

    public static bool LooksLikeAdmPCode(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value!.Trim().StartsWith(CountryPrefix, StringComparison.OrdinalIgnoreCase);

    public static bool LooksLikeNeighborhoodPCode(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value!.Trim().StartsWith(NeighborhoodPrefix, StringComparison.OrdinalIgnoreCase);

    public static bool LooksLikeCommunityPCode(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value!.Trim().StartsWith(CommunityPrefix, StringComparison.OrdinalIgnoreCase);
}
