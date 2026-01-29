namespace TRRCMS.Application.Households.Dtos;

/// <summary>
/// Simplified Household DTO - matches frontend form fields
/// تسجيل الأسرة - تسجيل تفاصيل الإشغال
/// </summary>
public class HouseholdDto
{
    // ==================== IDENTIFIERS ====================

    /// <summary>
    /// Unique identifier (GUID)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Parent property unit ID
    /// </summary>
    public Guid PropertyUnitId { get; set; }

    /// <summary>
    /// Property unit identifier (for display)
    /// </summary>
    public string? PropertyUnitIdentifier { get; set; }

    // ==================== BASIC INFORMATION ====================

    /// <summary>
    /// Head of household name (رب الأسرة/العميل - اسم الشخص)
    /// </summary>
    public string HeadOfHouseholdName { get; set; } = string.Empty;

    /// <summary>
    /// Link to Person entity if head is registered
    /// </summary>
    public Guid? HeadOfHouseholdPersonId { get; set; }

    /// <summary>
    /// Total household size (عدد الأفراد)
    /// </summary>
    public int HouseholdSize { get; set; }

    /// <summary>
    /// Notes/observations (ادخل ملاحظاتك)
    /// </summary>
    public string? Notes { get; set; }

    // ==================== ADULTS COMPOSITION (تكوين الأسرة - البالغين) ====================

    /// <summary>
    /// Number of adult males (عدد البالغين الذكور)
    /// </summary>
    public int MaleCount { get; set; }

    /// <summary>
    /// Number of adult females (عدد البالغين الإناث)
    /// </summary>
    public int FemaleCount { get; set; }

    // ==================== CHILDREN COMPOSITION (تكوين الأسرة - الأطفال) ====================

    /// <summary>
    /// Number of male children under 18 (عدد الأطفال الذكور - أقل من 18)
    /// </summary>
    public int MaleChildCount { get; set; }

    /// <summary>
    /// Number of female children under 18 (عدد الأطفال الإناث - أقل من 18)
    /// </summary>
    public int FemaleChildCount { get; set; }

    // ==================== ELDERLY COMPOSITION (تكوين الأسرة - كبار السن) ====================

    /// <summary>
    /// Number of male elderly over 65 (عدد كبار السن الذكور - أكثر من 65)
    /// </summary>
    public int MaleElderlyCount { get; set; }

    /// <summary>
    /// Number of female elderly over 65 (عدد كبار السن الإناث - أكثر من 65)
    /// </summary>
    public int FemaleElderlyCount { get; set; }

    // ==================== DISABLED COMPOSITION (تكوين الأسرة - المعاقين) ====================

    /// <summary>
    /// Number of male persons with disabilities (عدد المعاقين الذكور)
    /// </summary>
    public int MaleDisabledCount { get; set; }

    /// <summary>
    /// Number of female persons with disabilities (عدد المعاقين الإناث)
    /// </summary>
    public int FemaleDisabledCount { get; set; }

    // ==================== AUDIT FIELDS ====================

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Created by user ID
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime? LastModifiedAtUtc { get; set; }

    /// <summary>
    /// Last modified by user ID
    /// </summary>
    public Guid? LastModifiedBy { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Deletion timestamp
    /// </summary>
    public DateTime? DeletedAtUtc { get; set; }

    /// <summary>
    /// Deleted by user ID
    /// </summary>
    public Guid? DeletedBy { get; set; }
}
