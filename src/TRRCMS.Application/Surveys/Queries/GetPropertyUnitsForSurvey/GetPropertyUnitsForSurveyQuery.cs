using MediatR;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetPropertyUnitsForSurvey;

/// <summary>
/// Query to get all property units for a survey's building
/// Corresponds to UC-001 Stage 2: Property Unit Selection - View Available Units
/// </summary>
public class GetPropertyUnitsForSurveyQuery : IRequest<List<PropertyUnitDto>>
{
    /// <summary>
    /// Survey ID to get property units for
    /// </summary>
    public Guid SurveyId { get; set; }
}