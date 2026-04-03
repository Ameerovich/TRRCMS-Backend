using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Cases.Commands.SetCaseEditable;

public class SetCaseEditableCommandHandler : IRequestHandler<SetCaseEditableCommand, Unit>
{
    private readonly ICaseRepository _caseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public SetCaseEditableCommandHandler(
        ICaseRepository caseRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<Unit> Handle(SetCaseEditableCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var caseEntity = await _caseRepository.GetByIdAsync(request.CaseId, cancellationToken)
            ?? throw new NotFoundException($"Case with ID {request.CaseId} not found");

        var oldValue = caseEntity.IsEditable;
        caseEntity.SetEditable(request.IsEditable, currentUserId);

        await _caseRepository.UpdateAsync(caseEntity, cancellationToken);
        await _caseRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Case {caseEntity.CaseNumber} {(request.IsEditable ? "unlocked for editing" : "locked from editing")}",
            entityType: "Case",
            entityId: caseEntity.Id,
            entityIdentifier: caseEntity.CaseNumber,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new { IsEditable = oldValue }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new { IsEditable = caseEntity.IsEditable }),
            changedFields: "IsEditable",
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
