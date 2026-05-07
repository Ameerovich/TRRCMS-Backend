using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.CreateBuilding;

/// <summary>
/// Command to create a new building
/// Matches the frontend form: إضافة بناء جديد
/// </summary>
public record CreateBuildingCommand : IRequest<BuildingDto>
{
    /// <summary>
    /// Governorate code (محافظة) - 2 digits
    /// </summary>
    /// <example>01</example>
    public string GovernorateCode { get; init; } = string.Empty;

    /// <summary>
    /// District code (مدينة) - 2 digits
    /// </summary>
    /// <example>01</example>
    public string DistrictCode { get; init; } = string.Empty;

    /// <summary>
    /// Sub-district code (بلدة) - 2 digits
    /// </summary>
    /// <example>01</example>
    public string SubDistrictCode { get; init; } = string.Empty;

    /// <summary>
    /// Community code (قرية) - 3 digits
    /// </summary>
    /// <example>003</example>
    public string CommunityCode { get; init; } = string.Empty;

    /// <summary>
    /// Neighborhood code (حي) - 3 digits
    /// </summary>
    /// <example>002</example>
    public string NeighborhoodCode { get; init; } = string.Empty;

    /// <summary>
    /// Building number within neighborhood (رقم البناء) - 5 digits
    /// </summary>
    /// <example>00001</example>
    public string BuildingNumber { get; init; } = string.Empty;

    // ────────────── OCHA P-Codes (optional — accepted alongside raw codes) ──────────────
    /// <summary>OCHA governorate P-Code, e.g. "SY02".</summary>
    public string? GovernoratePCode { get; init; }
    /// <summary>OCHA district P-Code, e.g. "SY0200".</summary>
    public string? DistrictPCode { get; init; }
    /// <summary>OCHA sub-district P-Code, e.g. "SY020000".</summary>
    public string? SubDistrictPCode { get; init; }
    /// <summary>OCHA community P-Code, e.g. "C1007" (resolved via Community.ExternalPCode lookup).</summary>
    public string? CommunityPCode { get; init; }
    /// <summary>OCHA neighborhood P-Code, e.g. "N0160".</summary>
    public string? NeighborhoodPCode { get; init; }


    /// <summary>
    /// Building type (نوع البناء)
    /// 1=Residential(سكني), 2=Commercial(تجاري), 3=MixedUse(مختلط), 4=Industrial(صناعي)
    /// </summary>
    /// <example>1</example>
    public BuildingType BuildingType { get; init; }

    /// <summary>
    /// Building status (حالة البناء)
    /// </summary>
    /// <example>1</example>
    public BuildingStatus BuildingStatus { get; init; }

    /// <summary>
    /// Number of property units (عدد الوحدات)
    /// </summary>
    /// <example>10</example>
    public int NumberOfPropertyUnits { get; init; }

    /// <summary>
    /// Number of apartments (عدد المقاسم)
    /// </summary>
    /// <example>8</example>
    public int NumberOfApartments { get; init; }

    /// <summary>
    /// Number of shops (عدد المحلات)
    /// </summary>
    /// <example>2</example>
    public int NumberOfShops { get; init; }


    /// <summary>
    /// GPS latitude coordinate (optional)
    /// </summary>
    /// <example>36.2021</example>
    public decimal? Latitude { get; init; }

    /// <summary>
    /// GPS longitude coordinate (optional)
    /// </summary>
    /// <example>37.1343</example>
    public decimal? Longitude { get; init; }

    /// <summary>
    /// Building polygon geometry in WKT format (optional)
    /// </summary>
    public string? BuildingGeometryWkt { get; init; }


    /// <summary>
    /// General notes (الوصف العام) - optional
    /// </summary>
    /// <example>بناء سكني مؤلف من 5 طوابق</example>
    public string? Notes { get; init; }
}