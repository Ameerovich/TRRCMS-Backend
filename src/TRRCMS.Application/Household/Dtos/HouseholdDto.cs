namespace TRRCMS.Application.Households.Dtos;

/// <summary>
/// Household DTO (canonical v1.9 shape) — ungendered composition counts.
/// </summary>
public class HouseholdDto
{
    // ==================== IDENTIFIERS ====================

    /// <summary>Unique identifier (GUID)</summary>
    public Guid Id { get; set; }

    /// <summary>Parent property unit ID</summary>
    public Guid PropertyUnitId { get; set; }

    /// <summary>Property unit identifier (for display)</summary>
    public string? PropertyUnitIdentifier { get; set; }

    // ==================== BASIC INFORMATION ====================

    /// <summary>Total household size (عدد الأفراد) — required</summary>
    public int HouseholdSize { get; set; }

    /// <summary>Occupancy nature (طبيعة الإشغال) — int enum code</summary>
    public int? OccupancyNature { get; set; }

    /// <summary>Date the household started occupying this unit (UTC)</summary>
    public DateTime? OccupancyStartDate { get; set; }

    /// <summary>Notes/observations (ملاحظات)</summary>
    public string? Notes { get; set; }

    // ==================== COMPOSITION (all ungendered, all optional) ====================

    /// <summary>Total males across all ages (عدد الذكور)</summary>
    public int? MaleCount { get; set; }

    /// <summary>Total females across all ages (عدد الإناث)</summary>
    public int? FemaleCount { get; set; }

    /// <summary>Number of adults (عدد البالغين)</summary>
    public int? AdultCount { get; set; }

    /// <summary>Number of children (عدد الأطفال)</summary>
    public int? ChildCount { get; set; }

    /// <summary>Number of elderly (عدد كبار السن)</summary>
    public int? ElderlyCount { get; set; }

    /// <summary>Number of persons with disabilities (عدد ذوي الإعاقة)</summary>
    public int? DisabledCount { get; set; }

    // ==================== AUDIT FIELDS ====================

    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }
}
