using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.ActivateUser;

/// <summary>
/// Handler for ActivateUserCommand
/// Activates a deactivated user account
/// </summary>
public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public ActivateUserCommandHandler(
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

    public async Task<UserDto> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get user to activate
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        // Check if already active
        if (user.IsActive)
        {
            throw new ValidationException($"User '{user.Username}' is already active");
        }

        // Activate user using domain method
        user.Reactivate(currentUserId);

        // Save changes
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Activated user '{user.Username}'",
            entityType: "User",
            entityId: user.Id,
            entityIdentifier: user.Username,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new { IsActive = false }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new { IsActive = true }),
            changedFields: "IsActive, IsLockedOut, FailedLoginAttempts",
            cancellationToken: cancellationToken
        );

        // Return updated user
        return _mapper.Map<UserDto>(user);
    }
}