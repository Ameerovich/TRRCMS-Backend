using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Auth.Commands.ChangePassword;

/// <summary>
/// Handler for changing user password
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Validate that new password and confirmation match
        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new ArgumentException("New password and confirmation password do not match.");
        }

        // Step 2: Validate password strength (minimum requirements)
        if (request.NewPassword.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long.");
        }

        // Step 3: Find user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found.");
        }

        // Step 4: Verify current password
        bool isCurrentPasswordValid = _passwordHasher.VerifyPassword(
            request.CurrentPassword,
            user.PasswordHash,
            user.PasswordSalt);

        if (!isCurrentPasswordValid)
        {
            _logger.LogWarning("Password change rejected for user {UserId}: current password incorrect", request.UserId);
            throw new InvalidCredentialsException(
                "Message_CurrentPasswordIncorrect",
                "Current password is incorrect.");
        }

        // Step 5: Ensure new password is different from current password
        bool isSamePassword = _passwordHasher.VerifyPassword(
            request.NewPassword,
            user.PasswordHash,
            user.PasswordSalt);

        if (isSamePassword)
        {
            throw new ArgumentException("New password must be different from the current password.");
        }

        // Step 6: Hash new password
        string newPasswordHash = _passwordHasher.HashPassword(request.NewPassword, out string newSalt);

        // Step 7: Update user password (this invalidates all existing tokens via SecurityStamp)
        user.ChangePassword(newPasswordHash, newSalt, request.ModifiedByUserId);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogSecurityActionAsync(
            AuditActionType.PasswordChange,
            $"Password changed for user '{user.Username}'",
            isSecuritySensitive: true,
            cancellationToken);

        return true;
    }
}