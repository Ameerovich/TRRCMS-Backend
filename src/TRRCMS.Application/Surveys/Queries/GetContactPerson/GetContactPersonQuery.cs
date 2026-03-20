using MediatR;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetContactPerson;

/// <summary>
/// Get the contact person for a survey.
/// </summary>
public class GetContactPersonQuery : IRequest<PersonDto>
{
    public Guid SurveyId { get; set; }
}
