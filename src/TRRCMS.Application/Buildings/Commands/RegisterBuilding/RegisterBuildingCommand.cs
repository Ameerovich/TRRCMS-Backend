using MediatR;
using TRRCMS.Application.Buildings.Dtos;

namespace TRRCMS.Application.Buildings.Commands.RegisterBuilding;

/// <summary>
/// Minimal building registration from QGIS plugin.
/// Only administrative codes + polygon geometry required.
/// All other fields default (BuildingType=Residential, Status=Unknown, counts=0).
/// Full building details are provided later via field survey import (.uhc).
/// </summary>
public record RegisterBuildingCommand : IRequest<BuildingDto>
{
    /// <summary>Governorate code (2 digits, محافظة)</summary>
    /// <example>01</example>
    public string GovernorateCode { get; init; } = string.Empty;

    /// <summary>District code (2 digits, مدينة)</summary>
    /// <example>01</example>
    public string DistrictCode { get; init; } = string.Empty;

    /// <summary>Sub-district code (2 digits, بلدة)</summary>
    /// <example>01</example>
    public string SubDistrictCode { get; init; } = string.Empty;

    /// <summary>Community code (3 digits, قرية)</summary>
    /// <example>003</example>
    public string CommunityCode { get; init; } = string.Empty;

    /// <summary>Neighborhood code (3 digits, حي)</summary>
    /// <example>002</example>
    public string NeighborhoodCode { get; init; } = string.Empty;

    /// <summary>Building number (5 digits, رقم البناء)</summary>
    /// <example>00001</example>
    public string BuildingNumber { get; init; } = string.Empty;

    // ────────────── OCHA P-Codes (optional — accepted alongside raw codes) ──────────────
    // Callers may send either the raw codes above (legacy) or OCHA pCodes here. When
    // both are provided, the pCode wins. Community is resolved by ExternalPCode lookup.

    /// <summary>OCHA governorate P-Code, e.g. "SY02" (optional alternative to GovernorateCode).</summary>
    public string? GovernoratePCode { get; init; }

    /// <summary>OCHA district P-Code, e.g. "SY0200" (optional, populates Gov + Dist).</summary>
    public string? DistrictPCode { get; init; }

    /// <summary>OCHA sub-district P-Code, e.g. "SY020000" (optional, populates Gov + Dist + SubDist).</summary>
    public string? SubDistrictPCode { get; init; }

    /// <summary>OCHA community P-Code, e.g. "C1007" (optional — the system looks up Community.ExternalPCode).</summary>
    public string? CommunityPCode { get; init; }

    /// <summary>OCHA neighborhood P-Code, e.g. "N0160" (optional alternative to NeighborhoodCode).</summary>
    public string? NeighborhoodPCode { get; init; }


    /// <summary>
    /// Building polygon geometry in WKT format (from QGIS digitization).
    /// Coordinates in longitude latitude (X Y) order, SRID 4326 (WGS84).
    /// </summary>
    /// <example>POLYGON((37.1340 36.2018, 37.1346 36.2018, 37.1346 36.2024, 37.1340 36.2024, 37.1340 36.2018))</example>
    public string BuildingGeometryWkt { get; init; } = string.Empty;


    /// <summary>General notes (الوصف العام) - optional</summary>
    public string? Notes { get; init; }
}
