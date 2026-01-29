using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UpdateHouseholdInSurvey;

/// <summary>
/// Command to update a household in the context of a field survey
/// All fields optional - only provided fields will be updated
/// </summary>
public class UpdateHouseholdInSurveyCommand : IRequest<HouseholdDto>
{
    /// <summary>
    /// Survey ID for authorization check (required)
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Household ID to update (required)
    /// </summary>
    public Guid HouseholdId { get; set; }

    // ==================== BASIC INFORMATION ====================

    /// <summary>
    /// Head of household name (رب الأسرة/العميل)
    /// </summary>
    public string? HeadOfHouseholdName { get; set; }

    /// <summary>
    /// Total household size (عدد الأفراد)
    /// </summary>
    public int? HouseholdSize { get; set; }

    /// <summary>
    /// Notes/observations (ملاحظات)
    /// </summary>
    public string? Notes { get; set; }

    // ==================== ADULTS COMPOSITION ====================

    /// <summary>
    /// Number of adult males (عدد البالغين الذكور)
    /// </summary>
    public int? MaleCount { get; set; }

    /// <summary>
    /// Number of adult females (عدد البالغين الإناث)
    /// </summary>
    public int? FemaleCount { get; set; }

    // ==================== CHILDREN COMPOSITION ====================

    /// <summary>
    /// Number of male children under 18 (عدد الأطفال الذكور - أقل من 18)
    /// </summary>
    public int? MaleChildCount { get; set; }

    /// <summary>
    /// Number of female children under 18 (عدد الأطفال الإناث - أقل من 18)
    /// </summary>
    public int? FemaleChildCount { get; set; }

    // ==================== ELDERLY COMPOSITION ====================

    /// <summary>
    /// Number of male elderly over 65 (عدد كبار السن الذكور - أكثر من 65)
    /// </summary>
    public int? MaleElderlyCount { get; set; }

    /// <summary>
    /// Number of female elderly over 65 (عدد كبار السن الإناث - أكثر من 65)
    /// </summary>
    public int? FemaleElderlyCount { get; set; }

    // ==================== DISABLED COMPOSITION ====================

    /// <summary>
    /// Number of male persons with disabilities (عدد المعاقين الذكور)
    /// </summary>
    public int? MaleDisabledCount { get; set; }

    /// <summary>
    /// Number of female persons with disabilities (عدد المعاقين الإناث)
    /// </summary>
    public int? FemaleDisabledCount { get; set; }
}
