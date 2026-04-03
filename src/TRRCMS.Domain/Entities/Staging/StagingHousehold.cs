using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for Household records from .uhc packages.
/// Mirrors the <see cref="Household"/> production entity in an isolated staging area.
/// Subject to household structure validation:
/// - MaleCount + FemaleCount must be consistent with HouseholdSize
/// - Head of household must exist in StagingPersons
/// - Gender composition totals must be internally consistent
///
/// </summary>
public class StagingHousehold : BaseStagingEntity
{
    /// <summary>
    /// Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits.
    /// </summary>
    public Guid OriginalPropertyUnitId { get; private set; }

    /// <summary>Total number of persons in the household.</summary>
    public int HouseholdSize { get; private set; }
    /// <summary>Number of adult males.</summary>
    public int MaleCount { get; private set; }

    /// <summary>Number of adult females.</summary>
    public int FemaleCount { get; private set; }

    /// <summary>Number of male children.</summary>
    public int MaleChildCount { get; private set; }

    /// <summary>Number of female children.</summary>
    public int FemaleChildCount { get; private set; }

    /// <summary>Number of male elderly.</summary>
    public int MaleElderlyCount { get; private set; }

    /// <summary>Number of female elderly.</summary>
    public int FemaleElderlyCount { get; private set; }

    /// <summary>Number of males with disabilities.</summary>
    public int MaleDisabledCount { get; private set; }

    /// <summary>Number of females with disabilities.</summary>
    public int FemaleDisabledCount { get; private set; }
    /// <summary>Total number of children (MaleChildCount + FemaleChildCount).</summary>
    public int ChildCount { get; private set; }

    /// <summary>Total number of elderly (MaleElderlyCount + FemaleElderlyCount).</summary>
    public int ElderlyCount { get; private set; }

    /// <summary>Total number of persons with disabilities (MaleDisabledCount + FemaleDisabledCount).</summary>
    public int PersonsWithDisabilitiesCount { get; private set; }
    /// <summary>Occupancy type (OwnerOccupied, TenantOccupied, etc.).</summary>
    public OccupancyType? OccupancyType { get; private set; }

    /// <summary>Occupancy nature (LegalFormal, Informal, Customary, etc.).</summary>
    public OccupancyNature? OccupancyNature { get; private set; }

    /// <summary>Additional notes.</summary>
    public string? Notes { get; private set; }
    /// <summary>EF Core constructor.</summary>
    private StagingHousehold() : base()
    {
    }
    /// <summary>
    /// Create a new StagingHousehold record from .uhc package data.
    /// </summary>
    public static StagingHousehold Create(
        Guid importPackageId,
        Guid originalEntityId,
        Guid originalPropertyUnitId,
        int householdSize,
        int maleCount = 0,
        int femaleCount = 0,
        int maleChildCount = 0,
        int femaleChildCount = 0,
        int maleElderlyCount = 0,
        int femaleElderlyCount = 0,
        int maleDisabledCount = 0,
        int femaleDisabledCount = 0,
        OccupancyType? occupancyType = null,
        OccupancyNature? occupancyNature = null,
        string? notes = null)
    {
        var entity = new StagingHousehold
        {
            OriginalPropertyUnitId = originalPropertyUnitId,
            HouseholdSize = householdSize,
            MaleCount = maleCount,
            FemaleCount = femaleCount,
            MaleChildCount = maleChildCount,
            FemaleChildCount = femaleChildCount,
            MaleElderlyCount = maleElderlyCount,
            FemaleElderlyCount = femaleElderlyCount,
            MaleDisabledCount = maleDisabledCount,
            FemaleDisabledCount = femaleDisabledCount,
            ChildCount = maleChildCount + femaleChildCount,
            ElderlyCount = maleElderlyCount + femaleElderlyCount,
            PersonsWithDisabilitiesCount = maleDisabledCount + femaleDisabledCount,
            OccupancyType = occupancyType,
            OccupancyNature = occupancyNature,
            Notes = notes
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
