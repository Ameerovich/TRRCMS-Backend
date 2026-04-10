using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for Household records from .uhc packages.
/// Mirrors the <see cref="Household"/> production entity (canonical v1.9 shape).
/// Upper-bound validation rules (enforced in the import pipeline):
/// - (MaleCount ?? 0) + (FemaleCount ?? 0) ≤ HouseholdSize
/// - (AdultCount ?? 0) + (ChildCount ?? 0) + (ElderlyCount ?? 0) ≤ HouseholdSize
/// - (DisabledCount ?? 0) ≤ HouseholdSize
/// </summary>
public class StagingHousehold : BaseStagingEntity
{
    /// <summary>Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits.</summary>
    public Guid OriginalPropertyUnitId { get; private set; }

    /// <summary>Total number of persons in the household.</summary>
    public int HouseholdSize { get; private set; }

    /// <summary>Total males across all ages.</summary>
    public int? MaleCount { get; private set; }

    /// <summary>Total females across all ages.</summary>
    public int? FemaleCount { get; private set; }

    /// <summary>Number of adults.</summary>
    public int? AdultCount { get; private set; }

    /// <summary>Number of children.</summary>
    public int? ChildCount { get; private set; }

    /// <summary>Number of elderly.</summary>
    public int? ElderlyCount { get; private set; }

    /// <summary>Number of persons with disabilities.</summary>
    public int? DisabledCount { get; private set; }

    /// <summary>Occupancy nature (LegalFormal, Informal, Customary, etc.).</summary>
    public OccupancyNature? OccupancyNature { get; private set; }

    /// <summary>Date the household started occupying this unit (UTC).</summary>
    public DateTime? OccupancyStartDate { get; private set; }

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
        int? maleCount = null,
        int? femaleCount = null,
        int? adultCount = null,
        int? childCount = null,
        int? elderlyCount = null,
        int? disabledCount = null,
        OccupancyNature? occupancyNature = null,
        DateTime? occupancyStartDate = null,
        string? notes = null)
    {
        var entity = new StagingHousehold
        {
            OriginalPropertyUnitId = originalPropertyUnitId,
            HouseholdSize = householdSize,
            MaleCount = maleCount,
            FemaleCount = femaleCount,
            AdultCount = adultCount,
            ChildCount = childCount,
            ElderlyCount = elderlyCount,
            DisabledCount = disabledCount,
            OccupancyNature = occupancyNature,
            OccupancyStartDate = occupancyStartDate,
            Notes = notes
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
