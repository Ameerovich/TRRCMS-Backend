using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Household entity - family/group occupying a property unit
/// Includes demographic composition breakdown
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
    /// Household size (total number of members)
    /// </summary>
    public int HouseholdSize { get; private set; }

    /// <summary>
    /// Head of household name
    /// </summary>
    public string HeadOfHouseholdName { get; private set; }

    /// <summary>
    /// Foreign key to Person who is head of household (if registered)
    /// </summary>
    public Guid? HeadOfHouseholdPersonId { get; private set; }

    // ==================== GENDER COMPOSITION ====================

    /// <summary>
    /// Number of male members
    /// </summary>
    public int MaleCount { get; private set; }

    /// <summary>
    /// Number of female members
    /// </summary>
    public int FemaleCount { get; private set; }

    // ==================== AGE COMPOSITION ====================

    /// <summary>
    /// Number of infants (under 2 years)
    /// </summary>
    public int InfantCount { get; private set; }

    /// <summary>
    /// Number of children (2-12 years)
    /// </summary>
    public int ChildCount { get; private set; }

    /// <summary>
    /// Number of minors/adolescents (13-17 years)
    /// </summary>
    public int MinorCount { get; private set; }

    /// <summary>
    /// Number of adults (18-64 years)
    /// </summary>
    public int AdultCount { get; private set; }

    /// <summary>
    /// Number of elderly (65+ years)
    /// </summary>
    public int ElderlyCount { get; private set; }

    // ==================== VULNERABILITY INDICATORS ====================

    /// <summary>
    /// Number of persons with disabilities
    /// </summary>
    public int PersonsWithDisabilitiesCount { get; private set; }

    /// <summary>
    /// Number of female-headed households (1 if female-headed, 0 otherwise)
    /// </summary>
    public bool IsFemaleHeaded { get; private set; }

    /// <summary>
    /// Number of widows in household
    /// </summary>
    public int WidowCount { get; private set; }

    /// <summary>
    /// Number of orphans in household
    /// </summary>
    public int OrphanCount { get; private set; }

    /// <summary>
    /// Number of single parents
    /// </summary>
    public int SingleParentCount { get; private set; }

    // ==================== ECONOMIC INDICATORS ====================

    /// <summary>
    /// Number of employed persons
    /// </summary>
    public int EmployedPersonsCount { get; private set; }

    /// <summary>
    /// Number of unemployed persons (of working age)
    /// </summary>
    public int UnemployedPersonsCount { get; private set; }

    /// <summary>
    /// Primary income source
    /// </summary>
    public string? PrimaryIncomeSource { get; private set; }

    /// <summary>
    /// Estimated monthly household income (in local currency)
    /// </summary>
    public decimal? MonthlyIncomeEstimate { get; private set; }

    // ==================== DISPLACEMENT & ORIGIN ====================

    /// <summary>
    /// Indicates if household is displaced
    /// </summary>
    public bool IsDisplaced { get; private set; }

    /// <summary>
    /// Origin location if displaced
    /// </summary>
    public string? OriginLocation { get; private set; }

    /// <summary>
    /// Date when household arrived at current location
    /// </summary>
    public DateTime? ArrivalDate { get; private set; }

    /// <summary>
    /// Displacement reason
    /// </summary>
    public string? DisplacementReason { get; private set; }

    // ==================== ADDITIONAL INFORMATION ====================

    /// <summary>
    /// Household notes
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Special needs or circumstances
    /// </summary>
    public string? SpecialNeeds { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Property unit this household occupies
    /// </summary>
    public virtual PropertyUnit PropertyUnit { get; private set; } = null!;

    /// <summary>
    /// Head of household person (if registered as Person entity)
    /// </summary>
    public virtual Person? HeadOfHouseholdPerson { get; private set; }

    /// <summary>
    /// Individual persons in this household
    /// </summary>
    public virtual ICollection<Person> Members { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Household() : base()
    {
        HeadOfHouseholdName = string.Empty;
        Members = new List<Person>();
        HouseholdSize = 0;
        MaleCount = 0;
        FemaleCount = 0;
        InfantCount = 0;
        ChildCount = 0;
        MinorCount = 0;
        AdultCount = 0;
        ElderlyCount = 0;
        PersonsWithDisabilitiesCount = 0;
        IsFemaleHeaded = false;
        WidowCount = 0;
        OrphanCount = 0;
        SingleParentCount = 0;
        EmployedPersonsCount = 0;
        UnemployedPersonsCount = 0;
        IsDisplaced = false;
    }

    /// <summary>
    /// Create new household
    /// </summary>
    public static Household Create(
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
    /// Update household size
    /// </summary>
    public void UpdateSize(int newSize, Guid modifiedByUserId)
    {
        HouseholdSize = newSize;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update gender composition
    /// </summary>
    public void UpdateGenderComposition(int maleCount, int femaleCount, Guid modifiedByUserId)
    {
        MaleCount = maleCount;
        FemaleCount = femaleCount;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update age composition
    /// </summary>
    public void UpdateAgeComposition(
        int infantCount,
        int childCount,
        int minorCount,
        int adultCount,
        int elderlyCount,
        Guid modifiedByUserId)
    {
        InfantCount = infantCount;
        ChildCount = childCount;
        MinorCount = minorCount;
        AdultCount = adultCount;
        ElderlyCount = elderlyCount;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update vulnerability indicators
    /// </summary>
    public void UpdateVulnerabilityIndicators(
        int personsWithDisabilitiesCount,
        bool isFemaleHeaded,
        int widowCount,
        int orphanCount,
        int singleParentCount,
        Guid modifiedByUserId)
    {
        PersonsWithDisabilitiesCount = personsWithDisabilitiesCount;
        IsFemaleHeaded = isFemaleHeaded;
        WidowCount = widowCount;
        OrphanCount = orphanCount;
        SingleParentCount = singleParentCount;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update economic indicators
    /// </summary>
    public void UpdateEconomicIndicators(
        int employedCount,
        int unemployedCount,
        string? primaryIncomeSource,
        decimal? monthlyIncomeEstimate,
        Guid modifiedByUserId)
    {
        EmployedPersonsCount = employedCount;
        UnemployedPersonsCount = unemployedCount;
        PrimaryIncomeSource = primaryIncomeSource;
        MonthlyIncomeEstimate = monthlyIncomeEstimate;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update displacement information
    /// </summary>
    public void UpdateDisplacementInfo(
        bool isDisplaced,
        string? originLocation,
        DateTime? arrivalDate,
        string? displacementReason,
        Guid modifiedByUserId)
    {
        IsDisplaced = isDisplaced;
        OriginLocation = originLocation;
        ArrivalDate = arrivalDate;
        DisplacementReason = displacementReason;
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
    /// Update special needs
    /// </summary>
    public void UpdateSpecialNeeds(string? specialNeeds, Guid modifiedByUserId)
    {
        SpecialNeeds = specialNeeds;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Add notes
    /// </summary>
    public void AddNotes(string notes, Guid modifiedByUserId)
    {
        Notes = string.IsNullOrWhiteSpace(Notes)
            ? notes
            : $"{Notes}\n{notes}";
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Calculate dependency ratio
    /// </summary>
    public decimal CalculateDependencyRatio()
    {
        var dependents = InfantCount + ChildCount + MinorCount + ElderlyCount;
        var workingAge = AdultCount;

        if (workingAge == 0)
            return 0;

        return (decimal)dependents / workingAge;
    }

    /// <summary>
    /// Check if household is vulnerable
    /// Based on multiple vulnerability indicators
    /// </summary>
    public bool IsVulnerable()
    {
        return IsFemaleHeaded
            || PersonsWithDisabilitiesCount > 0
            || WidowCount > 0
            || OrphanCount > 0
            || SingleParentCount > 0
            || IsDisplaced
            || (ElderlyCount > 0 && AdultCount == 0);
    }
}