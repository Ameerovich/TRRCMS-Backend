using MediatR;
using TRRCMS.Application.PersonPropertyRelations.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetRelationsForPropertyUnitInSurvey;

/// <summary>
/// Query to get all person-property relations for a property unit within a survey context.
/// Returns the same PersonPropertyRelationDto used by LinkPersonToPropertyUnit response.
/// </summary>
public class GetRelationsForPropertyUnitInSurveyQuery : IRequest<List<PersonPropertyRelationDto>>
{
    /// <summary>
    /// Survey ID (for authorization — validates ownership)
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Property unit ID to get relations for
    /// </summary>
    public Guid PropertyUnitId { get; set; }
}
