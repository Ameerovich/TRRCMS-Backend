using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.UnlockUser;

/// <summary>
/// Handler for UnlockUserCommand
/// Unlocks a locked user account and resets failed login attempts
/// </summary>
public class UnlockUserCommandHandler : IRequestHandler<UnlockUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UnlockUserCommandHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserDto> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get user to unlock
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        // Check if actually locked
        if (!user.IsLockedOut)
        {
            throw new ValidationException($"User '{user.Username}' is not locked out");
        }

        // Unlock user using domain method
        user.Unlock(currentUserId);

        // Save changes
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Unlocked user '{user.Username}'",
            entityType: "User",
            entityId: user.Id,
            entityIdentifier: user.Username,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                IsLockedOut = true,
                FailedLoginAttempts = user.FailedLoginAttempts
            }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                IsLockedOut = false,
                FailedLoginAttempts = 0
            }),
            changedFields: "IsLockedOut, FailedLoginAttempts, LockoutEndDate",
            cancellationToken: cancellationToken
        );

        // Return updated user
        return _mapper.Map<UserDto>(user);
    }
}