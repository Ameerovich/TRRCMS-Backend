using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Household entity - family/group occupying a property unit
/// Includes demographic composition breakdown by gender
/// </summary>
public class Household : BaseAuditableEntity
{
    /// <summary>
    /// Foreign key to property unit
    /// </summary>
    public Guid PropertyUnitId { get; private set; }
    /// <summary>
    /// Household size (total number of members) - عدد الأفراد
    /// </summary>
    public int HouseholdSize { get; private set; }

    /// <summary>
    /// Head of household name - رب الأسرة/العميل (nullable for office survey)
    /// </summary>
    public string? HeadOfHouseholdName { get; private set; }

    /// <summary>
    /// Foreign key to Person who is head of household (if registered) - nullable for office survey
    /// </summary>
    public Guid? HeadOfHouseholdPersonId { get; private set; }
    /// <summary>
    /// Occupancy type (نوع الإشغال) - OwnerOccupied, TenantOccupied, FamilyOccupied, etc.
    /// </summary>
    public OccupancyType? OccupancyType { get; private set; }

    /// <summary>
    /// Occupancy nature (طبيعة الإشغال) - LegalFormal, Informal, Customary, etc.
    /// </summary>
    public OccupancyNature? OccupancyNature { get; private set; }
    /// <summary>
    /// Number of adult males - عدد البالغين الذكور
    /// </summary>
    public int MaleCount { get; private set; }

    /// <summary>
    /// Number of adult females - عدد البالغين الإناث
    /// </summary>
    public int FemaleCount { get; private set; }
    /// <summary>
    /// Number of male children under 18 - عدد الأطفال الذكور (أقل من 18)
    /// </summary>
    public int MaleChildCount { get; private set; }

    /// <summary>
    /// Number of female children under 18 - عدد الأطفال الإناث (أقل من 18)
    /// </summary>
    public int FemaleChildCount { get; private set; }
    /// <summary>
    /// Number of male elderly over 65 - عدد كبار السن الذكور (أكثر من 65)
    /// </summary>
    public int MaleElderlyCount { get; private set; }

    /// <summary>
    /// Number of female elderly over 65 - عدد كبار السن الإناث (أكثر من 65)
    /// </summary>
    public int FemaleElderlyCount { get; private set; }
    /// <summary>
    /// Number of male persons with disabilities - عدد المعاقين الذكور
    /// </summary>
    public int MaleDisabledCount { get; private set; }

    /// <summary>
    /// Number of female persons with disabilities - عدد المعاقين الإناث
    /// </summary>
    public int FemaleDisabledCount { get; private set; }
    /// <summary>
    /// Household notes - ملاحظات
    /// </summary>
    public string? Notes { get; private set; }
    public int ChildCount { get; private set; }
    public int ElderlyCount { get; private set; }
    public int PersonsWithDisabilitiesCount { get; private set; }
    public virtual PropertyUnit PropertyUnit { get; private set; } = null!;
    public virtual Person? HeadOfHouseholdPerson { get; private set; }
    public virtual ICollection<Person> Members { get; private set; }
    private Household() : base()
    {
        Members = new List<Person>();
    }

    /// <summary>
    /// Create new household with full composition (for frontend form)
    /// </summary>
    public static Household Create(
        Guid propertyUnitId,
        string? headOfHouseholdName,
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
        OccupancyType? occupancyType,
        OccupancyNature? occupancyNature,
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
            OccupancyType = occupancyType,
            OccupancyNature = occupancyNature,
            // Auto-calculate computed totals
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
        string? headOfHouseholdName,
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
    /// <summary>
    /// Update the property unit this household belongs to
    /// </summary>
    public void UpdatePropertyUnit(Guid propertyUnitId, Guid modifiedByUserId)
    {
        PropertyUnitId = propertyUnitId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update basic info
    /// </summary>
    public void UpdateBasicInfo(
        string? headOfHouseholdName,
        int householdSize,
        string? notes,
        OccupancyType? occupancyType,
        OccupancyNature? occupancyNature,
        Guid modifiedByUserId)
    {
        HeadOfHouseholdName = headOfHouseholdName;
        HouseholdSize = householdSize;
        Notes = notes;
        OccupancyType = occupancyType;
        OccupancyNature = occupancyNature;
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

        // Update computed totals
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

}