using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Security;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Enums;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Application.Users.Commands.ResetUserPassword;

/// <summary>
/// Handler for ResetUserPasswordCommand.
/// Sets a temporary password, forces a change at next login, ends every session of the
/// target user and resolves their pending "forgot password" request (if any).
/// </summary>
public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, ResetUserPasswordResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionValidator _sessionValidator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISecurityPolicyRepository _securityPolicyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public ResetUserPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISecurityPolicyRepository securityPolicyRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        ISessionValidator sessionValidator)
    {
        _sessionValidator = sessionValidator;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _securityPolicyRepository = securityPolicyRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<ResetUserPasswordResultDto> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        if (request.UserId == currentUserId)
            throw new ValidationException(
                "Administrators cannot reset their own password here. Use change-password instead.",
                "Message_CannotResetOwnPassword");

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        var hadPendingRequest = user.PasswordResetRequestedAtUtc.HasValue;

        string? generatedPassword = null;
        var temporaryPassword = request.TemporaryPassword;
        if (string.IsNullOrEmpty(temporaryPassword))
        {
            var activePolicy = await _securityPolicyRepository.GetActiveAsync(cancellationToken);
            generatedPassword = PasswordPolicyRules.GenerateTemporaryPassword(
                activePolicy?.PasswordPolicy ?? PasswordPolicy.Default());
            temporaryPassword = generatedPassword;
        }

        var hash = _passwordHasher.HashPassword(temporaryPassword, out var salt);
        user.ResetPassword(hash, salt, currentUserId);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        _sessionValidator.Invalidate(user.Id);

        // Never log the password itself
        await _auditService.LogActionAsync(
            actionType: AuditActionType.PasswordReset,
            actionDescription: $"Password reset by administrator for user '{user.Username}'"
                + (hadPendingRequest ? " (resolves the user's reset request)" : string.Empty),
            entityType: "User",
            entityId: user.Id,
            entityIdentifier: user.Username,
            changedFields: "PasswordHash, MustChangePassword, SecurityStamp, RefreshToken",
            cancellationToken: cancellationToken);

        return new ResetUserPasswordResultDto
        {
            UserId = user.Id,
            Username = user.Username,
            GeneratedTemporaryPassword = generatedPassword,
            MustChangePassword = user.MustChangePassword,
            ResetAtUtc = user.LastPasswordChangeDate ?? DateTime.UtcNow
        };
    }
}
