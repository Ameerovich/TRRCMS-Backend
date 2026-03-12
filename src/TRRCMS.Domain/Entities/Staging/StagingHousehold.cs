using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for Household records from .uhc packages.
/// Mirrors the <see cref="Household"/> production entity in an isolated staging area.
/// Subject to household structure validation (FR-D-4 Level 4):
/// - MaleCount + FemaleCount must be consistent with HouseholdSize
/// - Head of household must exist in StagingPersons
/// - Gender composition totals must be internally consistent
///
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingHousehold : BaseStagingEntity
{
    // ==================== RELATIONSHIPS (original UUIDs from .uhc) ====================

    /// <summary>
    /// Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits.
    /// </summary>
    public Guid OriginalPropertyUnitId { get; private set; }

    /// <summary>
    /// Original head-of-household Person UUID from .uhc.
    /// Used for cross-entity validation (head must exist in StagingPersons).
    /// </summary>
    public Guid? OriginalHeadOfHouseholdPersonId { get; private set; }

    // ==================== HOUSEHOLD CORE ====================

    /// <summary>Full name of the head of household.</summary>
    public string HeadOfHouseholdName { get; private set; }

    /// <summary>Total number of persons in the household.</summary>
    public int HouseholdSize { get; private set; }

    // ==================== GENDER COMPOSITION ====================

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

    // ==================== COMPUTED TOTALS ====================

    /// <summary>Total number of children (MaleChildCount + FemaleChildCount).</summary>
    public int ChildCount { get; private set; }

    /// <summary>Total number of elderly (MaleElderlyCount + FemaleElderlyCount).</summary>
    public int ElderlyCount { get; private set; }

    /// <summary>Total number of persons with disabilities (MaleDisabledCount + FemaleDisabledCount).</summary>
    public int PersonsWithDisabilitiesCount { get; private set; }

    // ==================== ADDITIONAL ====================

    /// <summary>Additional notes.</summary>
    public string? Notes { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>EF Core constructor.</summary>
    private StagingHousehold() : base()
    {
        HeadOfHouseholdName = string.Empty;
    }

    // ==================== FACTORY METHOD ====================

    /// <summary>
    /// Create a new StagingHousehold record from .uhc package data.
    /// </summary>
    public static StagingHousehold Create(
        Guid importPackageId,
        Guid originalEntityId,
        Guid originalPropertyUnitId,
        string headOfHouseholdName,
        int householdSize,
        Guid? originalHeadOfHouseholdPersonId = null,
        int maleCount = 0,
        int femaleCount = 0,
        int maleChildCount = 0,
        int femaleChildCount = 0,
        int maleElderlyCount = 0,
        int femaleElderlyCount = 0,
        int maleDisabledCount = 0,
        int femaleDisabledCount = 0,
        string? notes = null)
    {
        var entity = new StagingHousehold
        {
            OriginalPropertyUnitId = originalPropertyUnitId,
            OriginalHeadOfHouseholdPersonId = originalHeadOfHouseholdPersonId,
            HeadOfHouseholdName = headOfHouseholdName,
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
            Notes = notes
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
