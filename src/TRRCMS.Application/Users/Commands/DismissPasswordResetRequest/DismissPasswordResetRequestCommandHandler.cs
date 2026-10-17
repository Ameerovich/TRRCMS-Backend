using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.DismissPasswordResetRequest;

public class DismissPasswordResetRequestCommandHandler : IRequestHandler<DismissPasswordResetRequestCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DismissPasswordResetRequestCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Unit> Handle(DismissPasswordResetRequestCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        if (!user.PasswordResetRequestedAtUtc.HasValue)
            throw new NotFoundException($"User '{user.Username}' has no pending password reset request");

        user.DismissPasswordResetRequest(currentUserId);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Dismissed password reset request of user '{user.Username}'",
            entityType: "User",
            entityId: user.Id,
            entityIdentifier: user.Username,
            changedFields: "PasswordResetRequestedAtUtc, PasswordResetRequestNote",
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
