using MediatR;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UpdatePropertyUnitInSurvey;

/// <summary>
/// Command to update a property unit in the context of a field survey
/// Simplified to match frontend form fields
/// Corresponds to UC-001 Stage 2: Property Unit Selection - Update Existing Unit
/// </summary>
public class UpdatePropertyUnitInSurveyCommand : IRequest<PropertyUnitDto>
{
    /// <summary>
    /// Survey ID for authorization check (required)
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Property unit ID to update (required)
    /// </summary>
    public Guid PropertyUnitId { get; set; }

    // ==================== UPDATABLE FIELDS ====================
    // All fields are optional - only provided fields will be updated

    /// <summary>
    /// Unit identifier within building (رقم الوحدة)
    /// </summary>
    public string? UnitIdentifier { get; set; }

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
