using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.RevertSurveyToDraft;

/// <summary>
/// Handler for RevertSurveyToDraftCommand.
/// Transitions a Finalized survey back to Draft so it can be edited and re-processed.
/// </summary>
public class RevertSurveyToDraftCommandHandler : IRequestHandler<RevertSurveyToDraftCommand, Unit>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public RevertSurveyToDraftCommandHandler(
        ISurveyRepository surveyRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<Unit> Handle(
        RevertSurveyToDraftCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        var oldStatus = survey.Status;

        // Domain method handles status validation (only Finalized → Draft)
        survey.RevertToDraft(currentUserId);

        await _surveyRepository.UpdateAsync(survey, cancellationToken);
        await _surveyRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.StatusChange,
            actionDescription: $"Reverted survey {survey.ReferenceCode} to Draft. Reason: {request.Reason}",
            entityType: "Survey",
            entityId: survey.Id,
            entityIdentifier: survey.ReferenceCode,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new { Status = oldStatus.ToString() }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                Status = "Draft",
                RevertedAt = DateTime.UtcNow,
                RevertedBy = currentUserId,
                Reason = request.Reason
            }),
            changedFields: "Status",
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
