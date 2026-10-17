using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Auth.Commands.RequestPasswordReset;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<RequestPasswordResetCommandHandler> _logger;

    public RequestPasswordResetCommandHandler(
        IUserRepository userRepository,
        IAuditService auditService,
        ILogger<RequestPasswordResetCommandHandler> logger)
    {
        _userRepository = userRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Unit> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var username = request.Username.Trim();
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);

        // Unknown usernames get the same response as known ones (no account enumeration)
        if (user == null)
        {
            _logger.LogWarning("Password reset requested for unknown username {Username}", username);
            await _auditService.LogSecurityActionAsync(
                AuditActionType.PasswordResetRequested,
                $"Password reset requested for unknown username '{username}'",
                isSecuritySensitive: true,
                cancellationToken);
            return Unit.Value;
        }

        user.RequestPasswordReset(request.Note);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.PasswordResetRequested,
            actionDescription: $"User '{user.Username}' requested a password reset",
            entityType: "User",
            entityId: user.Id,
            entityIdentifier: user.Username,
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
