using MediatR;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Queries.SearchBuildings;

/// <summary>
/// Search buildings with filter criteria
/// Supports administrative hierarchy and attribute filters
/// </summary>
public class SearchBuildingsQuery : IRequest<SearchBuildingsResponse>
{
    /// <summary>
    /// Filter by governorate code (محافظة)
    /// </summary>
    public string? GovernorateCode { get; set; }

    /// <summary>
    /// Filter by district code (مدينة)
    /// </summary>
    public string? DistrictCode { get; set; }

    /// <summary>
    /// Filter by sub-district code (بلدة)
    /// </summary>
    public string? SubDistrictCode { get; set; }

    /// <summary>
    /// Filter by community code (قرية)
    /// </summary>
    public string? CommunityCode { get; set; }

    /// <summary>
    /// Filter by neighborhood code (حي)
    /// </summary>
    public string? NeighborhoodCode { get; set; }

    // ──────────── OCHA pCode filter aliases (optional) ────────────
    /// <summary>Filter by OCHA governorate P-Code, e.g. "SY02".</summary>
    public string? GovernoratePCode { get; set; }
    /// <summary>Filter by OCHA district P-Code, e.g. "SY0200".</summary>
    public string? DistrictPCode { get; set; }
    /// <summary>Filter by OCHA sub-district P-Code, e.g. "SY020000".</summary>
    public string? SubDistrictPCode { get; set; }
    /// <summary>Filter by OCHA community P-Code, e.g. "C1007".</summary>
    public string? CommunityPCode { get; set; }
    /// <summary>Filter by OCHA neighborhood P-Code, e.g. "N0160".</summary>
    public string? NeighborhoodPCode { get; set; }


    /// <summary>
    /// Search by building ID (رمز البناء) - partial match
    /// </summary>
    public string? BuildingId { get; set; }

    /// <summary>
    /// Search by building number (رقم البناء)
    /// </summary>
    public string? BuildingNumber { get; set; }


    /// <summary>
    /// Filter by building status (حالة البناء)
    /// </summary>
    public BuildingStatus? Status { get; set; }

    /// <summary>
    /// Filter by building type (نوع البناء)
    /// </summary>
    public BuildingType? BuildingType { get; set; }


    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Items per page (default: 20, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 20;


    /// <summary>
    /// Sort field: "buildingId", "createdDate", "status", "buildingType"
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort descending (default: false)
    /// </summary>
    public bool SortDescending { get; set; } = false;
}