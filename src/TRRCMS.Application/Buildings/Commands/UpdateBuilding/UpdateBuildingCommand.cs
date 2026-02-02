using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuilding;

/// <summary>
/// Update building attributes and details
/// UC-000: Manage Building Data
/// Note: Administrative codes (governorate, district, etc.) cannot be changed after creation
/// </summary>
public class UpdateBuildingCommand : IRequest<BuildingDto>
{
    /// <summary>
    /// Building ID (GUID) - set from route parameter
    /// </summary>
    public Guid BuildingId { get; set; }

    // ==================== BUILDING ATTRIBUTES ====================

    /// <summary>
    /// Building type (نوع البناء)
    /// 1=Residential, 2=Commercial, 3=MixedUse, 4=Industrial
    /// </summary>
    public BuildingType? BuildingType { get; set; }

    /// <summary>
    /// Building status (حالة البناء)
    /// </summary>
    public BuildingStatus? BuildingStatus { get; set; }

    /// <summary>
    /// Number of property units (عدد الوحدات)
    /// </summary>
    public int? NumberOfPropertyUnits { get; set; }

    /// <summary>
    /// Number of apartments (عدد المقاسم)
    /// </summary>
    public int? NumberOfApartments { get; set; }

    /// <summary>
    /// Number of shops (عدد المحلات)
    /// </summary>
    public int? NumberOfShops { get; set; }

    // ==================== LOCATION ====================

    /// <summary>
    /// GPS latitude coordinate
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// GPS longitude coordinate
    /// </summary>
    public decimal? Longitude { get; set; }

    // ==================== DESCRIPTIONS ====================

    /// <summary>
    /// Location description (وصف الموقع)
    /// </summary>
    public string? LocationDescription { get; set; }

    /// <summary>
    /// General notes (الوصف العام)
    /// </summary>
    public string? Notes { get; set; }
}