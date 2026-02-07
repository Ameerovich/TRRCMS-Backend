using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.FinalizeOfficeSurvey;

/// <summary>
/// Handler for FinalizeOfficeSurveyCommand.
/// Marks an office survey as Finalized
/// </summary>
public class FinalizeOfficeSurveyCommandHandler : IRequestHandler<FinalizeOfficeSurveyCommand, Unit>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public FinalizeOfficeSurveyCommandHandler(
        ISurveyRepository surveyRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<Unit> Handle(
        FinalizeOfficeSurveyCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.Type != SurveyType.Office)
            throw new ValidationException("This endpoint is only for office surveys.");

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Survey is in {survey.Status} status. Only Draft surveys can be finalized.");

        // Mark as finalized
        survey.MarkAsFinalized(currentUserId);

        await _surveyRepository.UpdateAsync(survey, cancellationToken);
        await _surveyRepository.SaveChangesAsync(cancellationToken);

        // Audit
        await _auditService.LogActionAsync(
            actionType: AuditActionType.StatusChange,
            actionDescription: $"Finalized office survey {survey.ReferenceCode}",
            entityType: "Survey",
            entityId: survey.Id,
            entityIdentifier: survey.ReferenceCode,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Draft" }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                Status = "Finalized",
                FinalizedAt = DateTime.UtcNow,
                FinalizedBy = currentUserId
            }),
            changedFields: "Status, FinalizedAt, FinalizedBy",
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
