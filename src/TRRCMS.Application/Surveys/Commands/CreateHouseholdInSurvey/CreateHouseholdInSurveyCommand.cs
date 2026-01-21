using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Surveys.Commands.CreateHouseholdInSurvey;

/// <summary>
/// Command to create a household in the context of a field survey
/// Corresponds to UC-001 Stage 3: Household Registration
/// </summary>
public class CreateHouseholdInSurveyCommand : IRequest<HouseholdDto>
{
    /// <summary>
    /// Survey ID this household is being created for
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Property unit ID this household occupies
    /// </summary>
    public Guid PropertyUnitId { get; set; }

    // ==================== BASIC INFORMATION ====================

    /// <summary>
    /// Name of the head of household
    /// </summary>
    public string HeadOfHouseholdName { get; set; } = string.Empty;

    /// <summary>
    /// Total household size (number of members)
    /// </summary>
    public int HouseholdSize { get; set; }

    // ==================== GENDER COMPOSITION ====================

    /// <summary>
    /// Number of male members
    /// </summary>
    public int? MaleCount { get; set; }

    /// <summary>
    /// Number of female members
    /// </summary>
    public int? FemaleCount { get; set; }

    // ==================== AGE COMPOSITION ====================

    /// <summary>
    /// Number of infants (under 2 years)
    /// </summary>
    public int? InfantCount { get; set; }

    /// <summary>
    /// Number of children (2-12 years)
    /// </summary>
    public int? ChildCount { get; set; }

    /// <summary>
    /// Number of minors/adolescents (13-17 years)
    /// </summary>
    public int? MinorCount { get; set; }

    /// <summary>
    /// Number of adults (18-64 years)
    /// </summary>
    public int? AdultCount { get; set; }

    /// <summary>
    /// Number of elderly (65+ years)
    /// </summary>
    public int? ElderlyCount { get; set; }

    // ==================== VULNERABILITY INDICATORS ====================

    /// <summary>
    /// Number of persons with disabilities
    /// </summary>
    public int? PersonsWithDisabilitiesCount { get; set; }

    /// <summary>
    /// Is this a female-headed household?
    /// </summary>
    public bool? IsFemaleHeaded { get; set; }

    /// <summary>
    /// Number of widows in household
    /// </summary>
    public int? WidowCount { get; set; }

    /// <summary>
    /// Number of orphans in household
    /// </summary>
    public int? OrphanCount { get; set; }

    /// <summary>
    /// Number of single parents
    /// </summary>
    public int? SingleParentCount { get; set; }

    // ==================== ECONOMIC INDICATORS ====================

    /// <summary>
    /// Number of employed persons
    /// </summary>
    public int? EmployedPersonsCount { get; set; }

    /// <summary>
    /// Number of unemployed persons (of working age)
    /// </summary>
    public int? UnemployedPersonsCount { get; set; }

    /// <summary>
    /// Primary income source
    /// </summary>
    public string? PrimaryIncomeSource { get; set; }

    /// <summary>
    /// Estimated monthly household income
    /// </summary>
    public decimal? MonthlyIncomeEstimate { get; set; }

    // ==================== DISPLACEMENT & ORIGIN ====================

    /// <summary>
    /// Is this household displaced?
    /// </summary>
    public bool? IsDisplaced { get; set; }

    /// <summary>
    /// Origin location if displaced
    /// </summary>
    public string? OriginLocation { get; set; }

    /// <summary>
    /// Date when household arrived at current location
    /// </summary>
    public DateTime? ArrivalDate { get; set; }

    /// <summary>
    /// Displacement reason
    /// </summary>
    public string? DisplacementReason { get; set; }

    // ==================== ADDITIONAL INFORMATION ====================

    /// <summary>
    /// Household notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Special needs or circumstances
    /// </summary>
    public string? SpecialNeeds { get; set; }
}