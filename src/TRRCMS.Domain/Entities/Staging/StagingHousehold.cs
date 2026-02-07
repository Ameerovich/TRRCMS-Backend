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

    // ==================== AGE CATEGORIES ====================

    /// <summary>Number of infants (0-1 years).</summary>
    public int InfantCount { get; private set; }

    /// <summary>Number of children (2-5 years).</summary>
    public int ChildCount { get; private set; }

    /// <summary>Number of minors (6-17 years).</summary>
    public int MinorCount { get; private set; }

    /// <summary>Number of adults (18-59 years).</summary>
    public int AdultCount { get; private set; }

    /// <summary>Number of elderly (60+ years).</summary>
    public int ElderlyCount { get; private set; }

    /// <summary>Number of persons with disabilities.</summary>
    public int PersonsWithDisabilitiesCount { get; private set; }

    // ==================== VULNERABILITY INDICATORS ====================

    /// <summary>Whether the household is headed by a female.</summary>
    public bool IsFemaleHeaded { get; private set; }

    /// <summary>Number of widows in the household.</summary>
    public int WidowCount { get; private set; }

    /// <summary>Number of orphans in the household.</summary>
    public int OrphanCount { get; private set; }

    /// <summary>Number of single parents.</summary>
    public int SingleParentCount { get; private set; }

    // ==================== ECONOMIC STATUS ====================

    /// <summary>Number of employed persons.</summary>
    public int EmployedPersonsCount { get; private set; }

    /// <summary>Number of unemployed persons.</summary>
    public int UnemployedPersonsCount { get; private set; }

    /// <summary>Primary source of income.</summary>
    public string? PrimaryIncomeSource { get; private set; }

    /// <summary>Estimated monthly income.</summary>
    public decimal? MonthlyIncomeEstimate { get; private set; }

    // ==================== DISPLACEMENT ====================

    /// <summary>Whether the household is displaced.</summary>
    public bool IsDisplaced { get; private set; }

    /// <summary>Original location before displacement.</summary>
    public string? OriginLocation { get; private set; }

    /// <summary>Reason for displacement.</summary>
    public string? DisplacementReason { get; private set; }

    // ==================== ADDITIONAL ====================

    /// <summary>Special needs description.</summary>
    public string? SpecialNeeds { get; private set; }

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
        int infantCount = 0,
        int childCount = 0,
        int minorCount = 0,
        int adultCount = 0,
        int elderlyCount = 0,
        int personsWithDisabilitiesCount = 0,
        bool isFemaleHeaded = false,
        int widowCount = 0,
        int orphanCount = 0,
        int singleParentCount = 0,
        int employedPersonsCount = 0,
        int unemployedPersonsCount = 0,
        string? primaryIncomeSource = null,
        decimal? monthlyIncomeEstimate = null,
        bool isDisplaced = false,
        string? originLocation = null,
        string? displacementReason = null,
        string? specialNeeds = null,
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
            InfantCount = infantCount,
            ChildCount = childCount,
            MinorCount = minorCount,
            AdultCount = adultCount,
            ElderlyCount = elderlyCount,
            PersonsWithDisabilitiesCount = personsWithDisabilitiesCount,
            IsFemaleHeaded = isFemaleHeaded,
            WidowCount = widowCount,
            OrphanCount = orphanCount,
            SingleParentCount = singleParentCount,
            EmployedPersonsCount = employedPersonsCount,
            UnemployedPersonsCount = unemployedPersonsCount,
            PrimaryIncomeSource = primaryIncomeSource,
            MonthlyIncomeEstimate = monthlyIncomeEstimate,
            IsDisplaced = isDisplaced,
            OriginLocation = originLocation,
            DisplacementReason = displacementReason,
            SpecialNeeds = specialNeeds,
            Notes = notes
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
