using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Sync.DTOs;

/// <summary>
/// Compact representation of a property unit included in a sync download payload.
/// Contains only the attributes needed by the Flutter tablet application to
/// display the unit in the field survey form and pre-populate known values.
///
/// Part of <see cref="SyncBuildingDto.PropertyUnits"/> within the sync payload.
/// FSD: FR-D-5 (Sync Package Contents).
/// </summary>
public sealed record SyncPropertyUnitDto
{
    /// <summary>
    /// Surrogate ID of the <c>PropertyUnit</c> entity.
    /// Used by the tablet to correlate collected data with the server record.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Human-readable unit identifier within the building (e.g., "Apt 1", "Shop 2").
    /// </summary>
    public string UnitIdentifier { get; init; } = string.Empty;

    /// <summary>
    /// Floor number where the unit is located; null if the information is unknown.
    /// </summary>
    public int? FloorNumber { get; init; }

    /// <summary>
    /// Position of the unit on the floor (e.g., "Left", "Right", "يمين", "يسار").
    /// </summary>
    public string? PositionOnFloor { get; init; }

    /// <summary>
    /// Type of the property unit (Apartment, Shop, Office, etc.).
    /// Stored as an integer in the database; transmitted as integer to the tablet.
    /// </summary>
    public PropertyUnitType UnitType { get; init; }

    /// <summary>
    /// Current occupancy / use status of the unit.
    /// </summary>
    public PropertyUnitStatus Status { get; init; }

    /// <summary>
    /// Area of the unit in square metres; null when not yet measured.
    /// </summary>
    public decimal? AreaSquareMeters { get; init; }

    /// <summary>
    /// Damage level recorded for this unit (None, Minor, Moderate, Severe, Destroyed).
    /// Null when damage assessment has not been performed.
    /// </summary>
    public DamageLevel? DamageLevel { get; init; }
}
