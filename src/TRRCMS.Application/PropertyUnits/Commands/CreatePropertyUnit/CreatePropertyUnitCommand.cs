using MediatR;

namespace TRRCMS.Application.PropertyUnits.Commands.CreatePropertyUnit;

/// <summary>
/// Command to create a new property unit
/// </summary>
public class CreatePropertyUnitCommand : IRequest<Guid>
{
    // ==================== IDENTIFIERS ====================

    public Guid BuildingId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;

    // ==================== UNIT ATTRIBUTES ====================

    public int? FloorNumber { get; set; }
    public int UnitType { get; set; }

    // ==================== OPTIONAL UNIT DETAILS ====================

    public decimal? AreaSquareMeters { get; set; }
    public int? NumberOfRooms { get; set; }
    public int? NumberOfBathrooms { get; set; }
    public bool? HasBalcony { get; set; }

    // ==================== OCCUPANCY INFORMATION ====================

    public int? OccupancyType { get; set; }
    public int? OccupancyNature { get; set; }
    public int? NumberOfHouseholds { get; set; }
    public int? TotalOccupants { get; set; }

    // ==================== ADDITIONAL INFORMATION ====================

    public string? Description { get; set; }
    public string? SpecialFeatures { get; set; }
}