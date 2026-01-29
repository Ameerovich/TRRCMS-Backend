using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Household entity - family/group occupying a property unit
/// Includes demographic composition breakdown by gender
/// </summary>
public class Household : BaseAuditableEntity
{
    // ==================== IDENTIFIERS ====================

    /// <summary>
    /// Foreign key to property unit
    /// </summary>
    public Guid PropertyUnitId { get; private set; }

    // ==================== HOUSEHOLD BASIC INFO ====================

    /// <summary>
    /// Household size (total number of members) - عدد الأفراد
    /// </summary>
    public int HouseholdSize { get; private set; }

    /// <summary>
    /// Head of household name - رب الأسرة/العميل
    /// </summary>
    public string HeadOfHouseholdName { get; private set; }

    /// <summary>
    /// Foreign key to Person who is head of household (if registered)
    /// </summary>
    public Guid? HeadOfHouseholdPersonId { get; private set; }

    // ==================== ADULTS BY GENDER ====================

    /// <summary>
    /// Number of adult males - عدد البالغين الذكور
    /// </summary>
    public int MaleCount { get; private set; }

    /// <summary>
    /// Number of adult females - عدد البالغين الإناث
    /// </summary>
    public int FemaleCount { get; private set; }

    // ==================== CHILDREN BY GENDER (NEW) ====================

    /// <summary>
    /// Number of male children under 18 - عدد الأطفال الذكور (أقل من 18)
    /// </summary>
    public int MaleChildCount { get; private set; }

    /// <summary>
    /// Number of female children under 18 - عدد الأطفال الإناث (أقل من 18)
    /// </summary>
    public int FemaleChildCount { get; private set; }

    // ==================== ELDERLY BY GENDER (NEW) ====================

    /// <summary>
    /// Number of male elderly over 65 - عدد كبار السن الذكور (أكثر من 65)
    /// </summary>
    public int MaleElderlyCount { get; private set; }

    /// <summary>
    /// Number of female elderly over 65 - عدد كبار السن الإناث (أكثر من 65)
    /// </summary>
    public int FemaleElderlyCount { get; private set; }

    // ==================== DISABLED BY GENDER (NEW) ====================

    /// <summary>
    /// Number of male persons with disabilities - عدد المعاقين الذكور
    /// </summary>
    public int MaleDisabledCount { get; private set; }

    /// <summary>
    /// Number of female persons with disabilities - عدد المعاقين الإناث
    /// </summary>
    public int FemaleDisabledCount { get; private set; }

    // ==================== NOTES ====================

    /// <summary>
    /// Household notes - ملاحظات
    /// </summary>
    public string? Notes { get; private set; }

    // ==================== LEGACY FIELDS (kept for future expansion) ====================

    public int InfantCount { get; private set; }
    public int ChildCount { get; private set; }
    public int MinorCount { get; private set; }
    public int AdultCount { get; private set; }
    public int ElderlyCount { get; private set; }
    public int PersonsWithDisabilitiesCount { get; private set; }
    public bool IsFemaleHeaded { get; private set; }
    public int WidowCount { get; private set; }
    public int OrphanCount { get; private set; }
    public int SingleParentCount { get; private set; }
    public int EmployedPersonsCount { get; private set; }
    public int UnemployedPersonsCount { get; private set; }
    public string? PrimaryIncomeSource { get; private set; }
    public decimal? MonthlyIncomeEstimate { get; private set; }
    public bool IsDisplaced { get; private set; }
    public string? OriginLocation { get; private set; }
    public DateTime? ArrivalDate { get; private set; }
    public string? DisplacementReason { get; private set; }
    public string? SpecialNeeds { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    public virtual PropertyUnit PropertyUnit { get; private set; } = null!;
    public virtual Person? HeadOfHouseholdPerson { get; private set; }
    public virtual ICollection<Person> Members { get; private set; }

    // ==================== CONSTRUCTORS ====================

    private Household() : base()
    {
        HeadOfHouseholdName = string.Empty;
        Members = new List<Person>();
    }

    /// <summary>
    /// Create new household with full composition (for frontend form)
    /// </summary>
    public static Household Create(
        Guid propertyUnitId,
        string headOfHouseholdName,
        int householdSize,
        int maleCount,
        int femaleCount,
        int maleChildCount,
        int femaleChildCount,
        int maleElderlyCount,
        int femaleElderlyCount,
        int maleDisabledCount,
        int femaleDisabledCount,
        string? notes,
        Guid createdByUserId)
    {
        var household = new Household
        {
            PropertyUnitId = propertyUnitId,
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
            Notes = notes,
            // Auto-calculate legacy totals
            ChildCount = maleChildCount + femaleChildCount,
            ElderlyCount = maleElderlyCount + femaleElderlyCount,
            PersonsWithDisabilitiesCount = maleDisabledCount + femaleDisabledCount
        };

        household.MarkAsCreated(createdByUserId);
        return household;
    }

    /// <summary>
    /// Simple create (legacy compatibility)
    /// </summary>
    public static Household CreateSimple(
        Guid propertyUnitId,
        string headOfHouseholdName,
        int householdSize,
        Guid createdByUserId)
    {
        var household = new Household
        {
            PropertyUnitId = propertyUnitId,
            HeadOfHouseholdName = headOfHouseholdName,
            HouseholdSize = householdSize
        };

        household.MarkAsCreated(createdByUserId);
        return household;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Update basic info
    /// </summary>
    public void UpdateBasicInfo(
        string headOfHouseholdName,
        int householdSize,
        string? notes,
        Guid modifiedByUserId)
    {
        HeadOfHouseholdName = headOfHouseholdName;
        HouseholdSize = householdSize;
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update full composition (for frontend form)
    /// </summary>
    public void UpdateComposition(
        int maleCount,
        int femaleCount,
        int maleChildCount,
        int femaleChildCount,
        int maleElderlyCount,
        int femaleElderlyCount,
        int maleDisabledCount,
        int femaleDisabledCount,
        Guid modifiedByUserId)
    {
        MaleCount = maleCount;
        FemaleCount = femaleCount;
        MaleChildCount = maleChildCount;
        FemaleChildCount = femaleChildCount;
        MaleElderlyCount = maleElderlyCount;
        FemaleElderlyCount = femaleElderlyCount;
        MaleDisabledCount = maleDisabledCount;
        FemaleDisabledCount = femaleDisabledCount;

        // Update legacy totals
        ChildCount = maleChildCount + femaleChildCount;
        ElderlyCount = maleElderlyCount + femaleElderlyCount;
        PersonsWithDisabilitiesCount = maleDisabledCount + femaleDisabledCount;

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Link head of household to Person entity
    /// </summary>
    public void LinkHeadOfHousehold(Guid personId, Guid modifiedByUserId)
    {
        HeadOfHouseholdPersonId = personId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update notes
    /// </summary>
    public void UpdateNotes(string? notes, Guid modifiedByUserId)
    {
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    // ==================== LEGACY METHODS (kept for expansion) ====================

    public void UpdateSize(int newSize, Guid modifiedByUserId)
    {
        HouseholdSize = newSize;
        MarkAsModified(modifiedByUserId);
    }

    public void UpdateGenderComposition(int maleCount, int femaleCount, Guid modifiedByUserId)
    {
        MaleCount = maleCount;
        FemaleCount = femaleCount;
        MarkAsModified(modifiedByUserId);
    }

    public void UpdateAgeComposition(
        int infantCount, int childCount, int minorCount,
        int adultCount, int elderlyCount, Guid modifiedByUserId)
    {
        InfantCount = infantCount;
        ChildCount = childCount;
        MinorCount = minorCount;
        AdultCount = adultCount;
        ElderlyCount = elderlyCount;
        MarkAsModified(modifiedByUserId);
    }

    public void UpdateVulnerabilityIndicators(
        int personsWithDisabilitiesCount, bool isFemaleHeaded,
        int widowCount, int orphanCount, int singleParentCount,
        Guid modifiedByUserId)
    {
        PersonsWithDisabilitiesCount = personsWithDisabilitiesCount;
        IsFemaleHeaded = isFemaleHeaded;
        WidowCount = widowCount;
        OrphanCount = orphanCount;
        SingleParentCount = singleParentCount;
        MarkAsModified(modifiedByUserId);
    }

    public void UpdateEconomicIndicators(
        int employedCount, int unemployedCount,
        string? primaryIncomeSource, decimal? monthlyIncomeEstimate,
        Guid modifiedByUserId)
    {
        EmployedPersonsCount = employedCount;
        UnemployedPersonsCount = unemployedCount;
        PrimaryIncomeSource = primaryIncomeSource;
        MonthlyIncomeEstimate = monthlyIncomeEstimate;
        MarkAsModified(modifiedByUserId);
    }

    public void UpdateDisplacementInfo(
        bool isDisplaced, string? originLocation,
        DateTime? arrivalDate, string? displacementReason,
        Guid modifiedByUserId)
    {
        IsDisplaced = isDisplaced;
        OriginLocation = originLocation;
        ArrivalDate = arrivalDate;
        DisplacementReason = displacementReason;
        MarkAsModified(modifiedByUserId);
    }

    public void UpdateSpecialNeeds(string? specialNeeds, Guid modifiedByUserId)
    {
        SpecialNeeds = specialNeeds;
        MarkAsModified(modifiedByUserId);
    }

    public bool IsVulnerable()
    {
        return IsFemaleHeaded
            || PersonsWithDisabilitiesCount > 0
            || WidowCount > 0
            || OrphanCount > 0
            || SingleParentCount > 0
            || IsDisplaced;
    }
}