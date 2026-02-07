using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for PropertyUnit records from .uhc packages.
/// Mirrors the <see cref="PropertyUnit"/> production entity in an isolated staging area.
/// Records are validated before commit to production (FSD FR-D-4).
/// 
/// Key staging-specific field:
/// - <see cref="OriginalBuildingId"/>: UUID of the parent building in the .uhc package
///   (not a FK to production Buildings — resolved during commit via staging cross-references).
/// 
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingPropertyUnit : BaseStagingEntity
{
    // ==================== RELATIONSHIPS (original UUIDs from .uhc) ====================

    /// <summary>
    /// Original Building UUID from .uhc — not a FK to production Buildings.
    /// Used for intra-batch referential integrity validation.
    /// </summary>
    public Guid OriginalBuildingId { get; private set; }

    // ==================== UNIT IDENTIFICATION ====================

    /// <summary>Unit identifier within the building.</summary>
    public string UnitIdentifier { get; private set; }

    // ==================== UNIT ATTRIBUTES ====================

    /// <summary>Property unit type classification.</summary>
    public PropertyUnitType UnitType { get; private set; }

    /// <summary>Property unit status — physical condition and occupancy.</summary>
    public PropertyUnitStatus Status { get; private set; }

    /// <summary>Floor number (0=Ground, 1=First, -1=Basement) — from command, optional.</summary>
    public int? FloorNumber { get; private set; }

    /// <summary>Number of rooms (عدد الغرف) — from command, optional.</summary>
    public int? NumberOfRooms { get; private set; }

    /// <summary>Occupancy status description.</summary>
    public string? OccupancyStatus { get; private set; }

    /// <summary>Damage level assessment.</summary>
    public DamageLevel? DamageLevel { get; private set; }

    /// <summary>Measured area in square meters.</summary>
    public decimal? AreaSquareMeters { get; private set; }

    /// <summary>Estimated area in square meters (when measurement not possible).</summary>
    public decimal? EstimatedAreaSqm { get; private set; }

    /// <summary>Position of unit on its floor (e.g. "Left", "Right", "Center").</summary>
    public string? PositionOnFloor { get; private set; }

    // ==================== OCCUPANCY ====================

    /// <summary>Type of occupancy arrangement.</summary>
    public OccupancyType? OccupancyType { get; private set; }

    /// <summary>Nature/basis of the occupancy.</summary>
    public OccupancyNature? OccupancyNature { get; private set; }

    // ==================== UTILITIES ====================

    /// <summary>Notes about available utilities (electricity, water, etc.).</summary>
    public string? UtilitiesNotes { get; private set; }

    // ==================== DESCRIPTION ====================

    /// <summary>General description of the property unit.</summary>
    public string? Description { get; private set; }

    /// <summary>Special features or notable characteristics.</summary>
    public string? SpecialFeatures { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>EF Core constructor.</summary>
    private StagingPropertyUnit() : base()
    {
        UnitIdentifier = string.Empty;
    }

    // ==================== FACTORY METHOD ====================

    /// <summary>
    /// Create a new StagingPropertyUnit record from .uhc package data.
    /// </summary>
    public static StagingPropertyUnit Create(
        Guid importPackageId,
        Guid originalEntityId,
        Guid originalBuildingId,
        string unitIdentifier,
        PropertyUnitType unitType,
        PropertyUnitStatus status,
        // --- optional: from command ---
        int? floorNumber = null,
        int? numberOfRooms = null,
        decimal? areaSquareMeters = null,
        string? description = null,
        // --- optional: future expansion ---
        string? occupancyStatus = null,
        DamageLevel? damageLevel = null,
        decimal? estimatedAreaSqm = null,
        string? positionOnFloor = null,
        OccupancyType? occupancyType = null,
        OccupancyNature? occupancyNature = null,
        string? utilitiesNotes = null,
        string? specialFeatures = null)
    {
        var entity = new StagingPropertyUnit
        {
            OriginalBuildingId = originalBuildingId,
            UnitIdentifier = unitIdentifier,
            UnitType = unitType,
            Status = status,
            FloorNumber = floorNumber,
            NumberOfRooms = numberOfRooms,
            AreaSquareMeters = areaSquareMeters,
            Description = description,
            OccupancyStatus = occupancyStatus,
            DamageLevel = damageLevel,
            EstimatedAreaSqm = estimatedAreaSqm,
            PositionOnFloor = positionOnFloor,
            OccupancyType = occupancyType,
            OccupancyNature = occupancyNature,
            UtilitiesNotes = utilitiesNotes,
            SpecialFeatures = specialFeatures
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
