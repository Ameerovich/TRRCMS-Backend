using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Households.Commands.CreateHousehold;

/// <summary>
/// Command to create a new household
/// Matches frontend form: تسجيل الأسرة
/// </summary>
public class CreateHouseholdCommand : IRequest<HouseholdDto>
{
    /// <summary>
    /// Property unit ID this household belongs to (required)
    /// </summary>
    public Guid PropertyUnitId { get; set; }

    // ==================== BASIC INFORMATION ====================

    /// <summary>
    /// Head of household name (رب الأسرة/العميل) - required
    /// </summary>
    public string HeadOfHouseholdName { get; set; } = string.Empty;

    /// <summary>
    /// Total household size (عدد الأفراد) - required
    /// </summary>
    public int HouseholdSize { get; set; }

    /// <summary>
    /// Notes/observations (ملاحظات)
    /// </summary>
    public string? Notes { get; set; }

    // ==================== ADULTS COMPOSITION ====================

    /// <summary>
    /// Number of adult males (عدد البالغين الذكور)
    /// </summary>
    public int MaleCount { get; set; }

    /// <summary>
    /// Number of adult females (عدد البالغين الإناث)
    /// </summary>
    public int FemaleCount { get; set; }

    // ==================== CHILDREN COMPOSITION ====================

    /// <summary>
    /// Number of male children under 18 (عدد الأطفال الذكور - أقل من 18)
    /// </summary>
    public int MaleChildCount { get; set; }

    /// <summary>
    /// Number of female children under 18 (عدد الأطفال الإناث - أقل من 18)
    /// </summary>
    public int FemaleChildCount { get; set; }

    // ==================== ELDERLY COMPOSITION ====================

    /// <summary>
    /// Number of male elderly over 65 (عدد كبار السن الذكور - أكثر من 65)
    /// </summary>
    public int MaleElderlyCount { get; set; }

    /// <summary>
    /// Number of female elderly over 65 (عدد كبار السن الإناث - أكثر من 65)
    /// </summary>
    public int FemaleElderlyCount { get; set; }

    // ==================== DISABLED COMPOSITION ====================

    /// <summary>
    /// Number of male persons with disabilities (عدد المعاقين الذكور)
    /// </summary>
    public int MaleDisabledCount { get; set; }

    /// <summary>
    /// Number of female persons with disabilities (عدد المعاقين الإناث)
    /// </summary>
    public int FemaleDisabledCount { get; set; }
}
