using MediatR;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UpdatePropertyUnitInSurvey;

/// <summary>
/// Command to update a property unit in the context of a field survey
/// Corresponds to UC-001 Stage 2: Property Unit Selection - Update Existing Unit
/// </summary>
public class UpdatePropertyUnitInSurveyCommand : IRequest<PropertyUnitDto>
{
    /// <summary>
    /// Survey ID for authorization check
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Property unit ID to update
    /// </summary>
    public Guid PropertyUnitId { get; set; }

    // ==================== UPDATABLE FIELDS ====================
    // All fields are optional - only provided fields will be updated

    /// <summary>
    /// Floor number (0 = Ground, 1 = First, -1 = Basement)
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Position on floor (e.g., "Left", "Right", "Center", "يمين", "يسار")
    /// </summary>
    public string? PositionOnFloor { get; set; }

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