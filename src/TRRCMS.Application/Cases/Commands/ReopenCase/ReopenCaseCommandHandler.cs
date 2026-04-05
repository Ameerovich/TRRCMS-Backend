using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Cases.Commands.ReopenCase;

public class ReopenCaseCommandHandler : IRequestHandler<ReopenCaseCommand, Unit>
{
    private readonly ICaseRepository _caseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public ReopenCaseCommandHandler(
        ICaseRepository caseRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<Unit> Handle(ReopenCaseCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var caseEntity = await _caseRepository.GetByIdAsync(request.CaseId, cancellationToken)
            ?? throw new NotFoundException($"Case with ID {request.CaseId} not found");

        if (caseEntity.Status != CaseLifecycleStatus.Closed)
            throw new ValidationException("Only closed cases can be reopened.");

        caseEntity.Reopen(currentUserId);

        await _caseRepository.UpdateAsync(caseEntity, cancellationToken);
        await _caseRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Case {caseEntity.CaseNumber} reopened",
            entityType: "Case",
            entityId: caseEntity.Id,
            entityIdentifier: caseEntity.CaseNumber,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Closed" }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new { Status = "Open" }),
            changedFields: "Status, ClosedDate, ClosedByClaimId",
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
