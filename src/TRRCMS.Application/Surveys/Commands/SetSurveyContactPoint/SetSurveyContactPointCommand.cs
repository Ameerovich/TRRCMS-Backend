using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Surveys.Commands.SetSurveyContactPoint;

/// <summary>
/// Designate an existing person of a draft survey as its Contact Point (جهة الاتصال),
/// replacing the previous one. The person may also be (or later become) a Rights Claimant.
/// </summary>
public class SetSurveyContactPointCommand : IRequest<PersonDto>
{
    public Guid SurveyId { get; set; }
    public Guid PersonId { get; set; }
}
