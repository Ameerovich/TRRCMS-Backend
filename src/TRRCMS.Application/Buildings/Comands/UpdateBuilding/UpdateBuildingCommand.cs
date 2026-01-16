using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuilding;

/// <summary>
/// Update building attributes, status, and details
/// UC-000: Manage Building Data
/// Note: Administrative codes (governorate, district, etc.) cannot be changed after creation
/// </summary>
public class UpdateBuildingCommand : IRequest<BuildingDto>
{
    /// <summary>
    /// Building ID (GUID) - required
    /// </summary>
    public Guid BuildingId { get; set; }

    // Status & Condition
    public BuildingStatus? Status { get; set; }
    public DamageLevel? DamageLevel { get; set; }

    // Building Type
    public BuildingType? BuildingType { get; set; }

    // Unit Counts
    public int? NumberOfApartments { get; set; }
    public int? NumberOfShops { get; set; }

    // Building Details
    public int? NumberOfFloors { get; set; }
    public int? YearOfConstruction { get; set; }

    // Location Details
    public string? Address { get; set; }
    public string? Landmark { get; set; }
    public string? Notes { get; set; }

    // Coordinates
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    // Audit
    /// <summary>
    /// Reason for modification (required for audit trail)
    /// Minimum 10 characters
    /// </summary>
    public string ReasonForModification { get; set; } = string.Empty;
}