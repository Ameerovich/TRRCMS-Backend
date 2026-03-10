using MediatR;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Surveys.Commands.DeleteHouseholdInSurvey;

/// <summary>
/// Command to delete a household within a survey context (cascade soft delete).
/// حذف الأسرة مع جميع البيانات المرتبطة بها
/// </summary>
public class DeleteHouseholdInSurveyCommand : IRequest<DeleteResultDto>
{
    /// <summary>
    /// Survey ID for authorization check
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Household ID to delete
    /// </summary>
    public Guid HouseholdId { get; set; }
}
