using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Households.Commands.CreateHousehold;

/// <summary>
/// Command to create a new household
/// </summary>
public class CreateHouseholdCommand : IRequest<HouseholdDto>
{
    // ==================== REQUIRED FIELDS ====================

    /// <summary>
    /// Property unit this household occupies (required)
    /// </summary>
    public Guid PropertyUnitId { get; set; }

    /// <summary>
    /// Name of head of household (required)
    /// </summary>
    public string HeadOfHouseholdName { get; set; } = string.Empty;

    /// <summary>
    /// Total household size (required)
    /// </summary>
    public int HouseholdSize { get; set; }

    /// <summary>
    /// User creating this household (required)
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    // ==================== OPTIONAL FIELDS ====================

    /// <summary>
    /// Link to Person entity if head of household is registered as Person
    /// </summary>
    public Guid? HeadOfHouseholdPersonId { get; set; }

    // ==================== GENDER COMPOSITION ====================

    public int? MaleCount { get; set; }
    public int? FemaleCount { get; set; }

    // ==================== AGE COMPOSITION ====================

    public int? InfantCount { get; set; }
    public int? ChildCount { get; set; }
    public int? MinorCount { get; set; }
    public int? AdultCount { get; set; }
    public int? ElderlyCount { get; set; }

    // ==================== VULNERABILITY INDICATORS ====================

    public int? PersonsWithDisabilitiesCount { get; set; }
    public bool? IsFemaleHeaded { get; set; }
    public int? WidowCount { get; set; }
    public int? OrphanCount { get; set; }
    public int? SingleParentCount { get; set; }

    // ==================== ECONOMIC INDICATORS ====================

    public int? EmployedPersonsCount { get; set; }
    public int? UnemployedPersonsCount { get; set; }
    public string? PrimaryIncomeSource { get; set; }
    public decimal? MonthlyIncomeEstimate { get; set; }

    // ==================== DISPLACEMENT & ORIGIN ====================

    public bool? IsDisplaced { get; set; }
    public string? OriginLocation { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public string? DisplacementReason { get; set; }

    // ==================== ADDITIONAL INFORMATION ====================

    public string? Notes { get; set; }
    public string? SpecialNeeds { get; set; }
}
