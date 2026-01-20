using MediatR;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.Surveys.Commands.CreatePropertyUnitInSurvey;

/// <summary>
/// Command to create a new property unit in the context of a field survey
/// Corresponds to UC-001 Stage 2: Property Unit Selection - Create New Unit
/// </summary>
public class CreatePropertyUnitInSurveyCommand : IRequest<PropertyUnitDto>
{
    /// <summary>
    /// Survey ID this property unit is being created for
    /// </summary>
    public Guid SurveyId { get; set; }

    // ==================== PROPERTY UNIT DETAILS ====================

    /// <summary>
    /// Unit identifier (e.g., "1A", "Ground-Left", "الطابق الأول-يمين")
    /// </summary>
    public string UnitIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Floor number (0 = Ground, 1 = First, -1 = Basement)
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Position on floor (e.g., "Left", "Right", "Center", "يمين", "يسار")
    /// </summary>
    public string? PositionOnFloor { get; set; }

    /// <summary>
    /// Unit type (Apartment, Shop, Office, Storage, etc.)
    /// </summary>
    public string UnitType { get; set; } = string.Empty;

    /// <summary>
    /// Occupancy status (Occupied, Vacant, UnderConstruction, etc.)
    /// </summary>
    public string? OccupancyStatus { get; set; }

    /// <summary>
    /// Number of rooms (bedrooms, living rooms, etc.)
    /// </summary>
    public int? NumberOfRooms { get; set; }

    /// <summary>
    /// Estimated area in square meters
    /// </summary>
    public decimal? EstimatedAreaSqm { get; set; }

    /// <summary>
    /// Unit description and notes
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Has electricity connection
    /// </summary>
    public bool? HasElectricity { get; set; }

    /// <summary>
    /// Has water connection
    /// </summary>
    public bool? HasWater { get; set; }

    /// <summary>
    /// Has sewage connection
    /// </summary>
    public bool? HasSewage { get; set; }

    /// <summary>
    /// Additional utilities notes
    /// </summary>
    public string? UtilitiesNotes { get; set; }
}