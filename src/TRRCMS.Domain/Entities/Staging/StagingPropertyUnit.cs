using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for PropertyUnit records from .uhc packages.
/// Mirrors the <see cref="PropertyUnit"/> production entity in an isolated staging area.
/// Records are validated before commit to production.
/// 
/// Key staging-specific field:
/// - <see cref="OriginalBuildingId"/>: UUID of the parent building in the .uhc package
///   (not a FK to production Buildings — resolved during commit via staging cross-references).
/// </summary>
public class StagingPropertyUnit : BaseStagingEntity
{
    /// <summary>
    /// Original Building UUID from .uhc — not a FK to production Buildings.
    /// Used for intra-batch referential integrity validation.
    /// </summary>
    public Guid OriginalBuildingId { get; private set; }
    /// <summary>Unit identifier within the building.</summary>
    public string UnitIdentifier { get; private set; }
    /// <summary>Property unit type classification.</summary>
    public PropertyUnitType UnitType { get; private set; }

    /// <summary>Property unit status — physical condition and occupancy.</summary>
    public PropertyUnitStatus Status { get; private set; }

    /// <summary>Floor number (0=Ground, 1=First, -1=Basement) — from command, optional.</summary>
    public int? FloorNumber { get; private set; }

    /// <summary>Number of rooms (عدد الغرف) — from command, optional.</summary>
    public int? NumberOfRooms { get; private set; }

    /// <summary>Measured area in square meters.</summary>
    public decimal? AreaSquareMeters { get; private set; }
    /// <summary>General description of the property unit.</summary>
    public string? Description { get; private set; }
    /// <summary>EF Core constructor.</summary>
    private StagingPropertyUnit() : base()
    {
        UnitIdentifier = string.Empty;
    }
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
        string? description = null)
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
            Description = description
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
