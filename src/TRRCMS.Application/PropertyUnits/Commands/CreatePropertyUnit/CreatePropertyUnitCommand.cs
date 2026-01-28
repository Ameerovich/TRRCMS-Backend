using MediatR;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Commands.CreatePropertyUnit;

/// <summary>
/// Command to create a new property unit
/// Simplified to match frontend form fields
/// </summary>
public class CreatePropertyUnitCommand : IRequest<PropertyUnitDto>
{
    /// <summary>
    /// Parent building ID (required)
    /// </summary>
    public Guid BuildingId { get; set; }

    /// <summary>
    /// Unit identifier within building (رقم الوحدة) - required
    /// Example: "1", "1A", "Ground-Left"
    /// </summary>
    public string UnitIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Floor number (رقم الطابق)
    /// 0 = Ground, 1 = First, -1 = Basement
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Property unit type (نوع الوحدة) - required
    /// Values: 1=Apartment, 2=Shop, 3=Office, 4=Warehouse, 5=Other
    /// </summary>
    public int UnitType { get; set; }

    /// <summary>
    /// Property unit status (حالة الوحدة) - required
    /// Values: 1=Occupied, 2=Vacant, 3=Damaged, 4=UnderRenovation, 5=Uninhabitable, 6=Locked, 99=Unknown
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Area in square meters (مساحة القسم)
    /// </summary>
    public decimal? AreaSquareMeters { get; set; }

    /// <summary>
    /// Number of rooms (عدد الغرف)
    /// </summary>
    public int? NumberOfRooms { get; set; }

    /// <summary>
    /// Unit description and notes (وصف مفصل)
    /// </summary>
    public string? Description { get; set; }
}