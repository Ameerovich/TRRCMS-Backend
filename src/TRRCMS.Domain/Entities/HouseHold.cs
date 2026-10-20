using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Household entity — family/group occupying a property unit.
/// Canonical field shape (v1.9): ungendered age/disability counts, nullable composition, occupancy start date.
/// </summary>
public class Household : BaseAuditableEntity
{
    /// <summary>Foreign key to property unit.</summary>
    public Guid PropertyUnitId { get; private set; }

    /// <summary>Total household members (عدد الأفراد) — 1–50, or 0 when <see cref="IsEmptyProperty"/>.</summary>
    public int HouseholdSize { get; private set; }

    /// <summary>Total males across all ages (عدد الذكور).</summary>
    public int? MaleCount { get; private set; }

    /// <summary>Total females across all ages (عدد الإناث).</summary>
    public int? FemaleCount { get; private set; }

    /// <summary>Number of adults (عدد البالغين).</summary>
    public int? AdultCount { get; private set; }

    /// <summary>Number of children (عدد الأطفال).</summary>
    public int? ChildCount { get; private set; }

    /// <summary>Number of elderly (عدد كبار السن).</summary>
    public int? ElderlyCount { get; private set; }

    /// <summary>Number of persons with disabilities (عدد ذوي الإعاقة).</summary>
    public int? DisabledCount { get; private set; }

    /// <summary>Occupancy nature (طبيعة الإشغال) — LegalFormal, Informal, Customary, etc.</summary>
    public OccupancyNature? OccupancyNature { get; private set; }

    /// <summary>Date the household started occupying this unit (UTC).</summary>
    public DateTime? OccupancyStartDate { get; private set; }

    /// <summary>Household notes (ملاحظات).</summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// عقار فارغ — the property unit was found empty (no occupants); all counts are 0.
    /// Set by tablet (.uhc) imports and by the desktop survey household endpoints.
    /// </summary>
    public bool IsEmptyProperty { get; private set; }

    /// <summary>
    /// مبرر كون العقار فارغاً — why the unit was found empty (required on the tablet when IsEmptyProperty).
    /// </summary>
    public string? EmptyPropertyJustification { get; private set; }

    public virtual PropertyUnit PropertyUnit { get; private set; } = null!;
    public virtual ICollection<Person> Members { get; private set; }

    private Household() : base()
    {
        Members = new List<Person>();
    }

    /// <summary>
    /// Create a new household with the canonical v1.9 composition.
    /// </summary>
    public static Household Create(
        Guid propertyUnitId,
        int householdSize,
        int? maleCount,
        int? femaleCount,
        int? adultCount,
        int? childCount,
        int? elderlyCount,
        int? disabledCount,
        OccupancyNature? occupancyNature,
        DateTime? occupancyStartDate,
        string? notes,
        Guid createdByUserId,
        bool isEmptyProperty = false,
        string? emptyPropertyJustification = null)
    {
        var household = new Household
        {
            IsEmptyProperty = isEmptyProperty,
            EmptyPropertyJustification = string.IsNullOrWhiteSpace(emptyPropertyJustification)
                ? null
                : emptyPropertyJustification.Trim(),
            PropertyUnitId = propertyUnitId,
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

        household.MarkAsCreated(createdByUserId);
        return household;
    }

    /// <summary>Update the property unit this household belongs to.</summary>
    public void UpdatePropertyUnit(Guid propertyUnitId, Guid modifiedByUserId)
    {
        PropertyUnitId = propertyUnitId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>Update basic info (size, notes, occupancy nature, occupancy start date).</summary>
    public void UpdateBasicInfo(
        int householdSize,
        string? notes,
        OccupancyNature? occupancyNature,
        DateTime? occupancyStartDate,
        Guid modifiedByUserId)
    {
        HouseholdSize = householdSize;
        Notes = notes;
        OccupancyNature = occupancyNature;
        OccupancyStartDate = occupancyStartDate;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>Update full composition (all counts are nullable).</summary>
    public void UpdateComposition(
        int? maleCount,
        int? femaleCount,
        int? adultCount,
        int? childCount,
        int? elderlyCount,
        int? disabledCount,
        Guid modifiedByUserId)
    {
        MaleCount = maleCount;
        FemaleCount = femaleCount;
        AdultCount = adultCount;
        ChildCount = childCount;
        ElderlyCount = elderlyCount;
        DisabledCount = disabledCount;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>Update notes only.</summary>
    public void UpdateNotes(string? notes, Guid modifiedByUserId)
    {
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark the unit as empty (or occupied). Clearing the flag also clears the justification.
    /// </summary>
    public void SetEmptyProperty(bool isEmptyProperty, string? justification, Guid modifiedByUserId)
    {
        IsEmptyProperty = isEmptyProperty;
        EmptyPropertyJustification = isEmptyProperty && !string.IsNullOrWhiteSpace(justification)
            ? justification.Trim()
            : null;
        MarkAsModified(modifiedByUserId);
    }
}
