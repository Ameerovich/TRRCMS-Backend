using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Surveys.Commands.CreateHouseholdInSurvey;

/// <summary>
/// Command to create a household in the context of a survey (canonical v1.9 shape).
/// </summary>
public class CreateHouseholdInSurveyCommand : IRequest<HouseholdDto>
{
    /// <summary>Survey ID this household is being created for (required)</summary>
    public Guid SurveyId { get; set; }

    /// <summary>Property unit ID (if not using survey's linked property unit)</summary>
    public Guid? PropertyUnitId { get; set; }

    /// <summary>Total household size (عدد الأفراد) — required, 1–50</summary>
    public int HouseholdSize { get; set; }

    /// <summary>Notes/observations (ملاحظات)</summary>
    public string? Notes { get; set; }

    /// <summary>Occupancy nature enum code</summary>
    public int? OccupancyNature { get; set; }

    /// <summary>Date the household started occupying this unit (UTC)</summary>
    public DateTime? OccupancyStartDate { get; set; }

    public int? MaleCount { get; set; }
    public int? FemaleCount { get; set; }
    public int? AdultCount { get; set; }
    public int? ChildCount { get; set; }
    public int? ElderlyCount { get; set; }
    public int? DisabledCount { get; set; }
}
