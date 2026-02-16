using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Auth.Commands.ChangePassword;

/// <summary>
/// Handler for changing user password
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
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
            throw new UnauthorizedAccessException("Current password is incorrect.");
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

        return true;
    }
}