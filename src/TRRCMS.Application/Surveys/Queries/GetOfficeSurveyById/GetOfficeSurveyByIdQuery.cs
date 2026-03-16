using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetOfficeSurveyById;

/// <summary>
/// Query to get a specific office survey by ID with full details
/// Gets a specific office survey by ID with full details
/// </summary>
public class GetOfficeSurveyByIdQuery : IRequest<OfficeSurveyDetailDto>
{
    /// <summary>
    /// Survey ID to retrieve
    /// </summary>
    public Guid SurveyId { get; set; }
}
