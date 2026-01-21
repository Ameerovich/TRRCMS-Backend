using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Surveys.Commands.SetHouseholdHead;

/// <summary>
/// Command to set/designate the head of household
/// Links a Person entity as the official head of household
/// </summary>
public class SetHouseholdHeadCommand : IRequest<HouseholdDto>
{
    /// <summary>
    /// Survey ID for authorization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Household ID
    /// </summary>
    public Guid HouseholdId { get; set; }

    /// <summary>
    /// Person ID to set as head of household
    /// </summary>
    public Guid PersonId { get; set; }
}