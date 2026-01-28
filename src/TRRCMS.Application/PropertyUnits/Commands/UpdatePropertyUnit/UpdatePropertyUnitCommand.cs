using MediatR;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Commands.UpdatePropertyUnit;

/// <summary>
/// Command to update a property unit
/// Simplified to match frontend form fields
/// All fields optional - only provided fields will be updated
/// </summary>
public class UpdatePropertyUnitCommand : IRequest<PropertyUnitDto>
{
    /// <summary>
    /// Property unit ID to update (required)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Floor number (رقم الطابق)
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Property unit type (نوع الوحدة)
    /// Values: 1=Apartment, 2=Shop, 3=Office, 4=Warehouse, 5=Other
    /// </summary>
    public int? UnitType { get; set; }

    /// <summary>
    /// Property unit status (حالة الوحدة)
    /// Values: 1=Occupied, 2=Vacant, 3=Damaged, 4=UnderRenovation, 5=Uninhabitable, 6=Locked, 99=Unknown
    /// </summary>
    public int? Status { get; set; }

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
