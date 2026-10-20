using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UpdateHouseholdInSurvey;

/// <summary>
/// Command to update a household in the context of a survey — partial update (canonical v1.9 shape).
/// </summary>
public class UpdateHouseholdInSurveyCommand : IRequest<HouseholdDto>
{
    /// <summary>Survey ID for authorization check (required)</summary>
    public Guid SurveyId { get; set; }

    /// <summary>Household ID to update (required)</summary>
    public Guid HouseholdId { get; set; }

    /// <summary>Property unit ID to move this household to (must belong to the survey's building)</summary>
    public Guid? PropertyUnitId { get; set; }

    public int? HouseholdSize { get; set; }
    public string? Notes { get; set; }
    public int? OccupancyNature { get; set; }
    public DateTime? OccupancyStartDate { get; set; }

    /// <summary>The unit is empty (المقسم فارغ); null = keep current. When true, every provided count must be 0</summary>
    public bool? IsEmptyProperty { get; set; }

    /// <summary>Why the unit is empty; null/blank keeps the stored justification while the unit stays empty</summary>
    public string? EmptyPropertyJustification { get; set; }

    public int? MaleCount { get; set; }
    public int? FemaleCount { get; set; }
    public int? AdultCount { get; set; }
    public int? ChildCount { get; set; }
    public int? ElderlyCount { get; set; }
    public int? DisabledCount { get; set; }
}
