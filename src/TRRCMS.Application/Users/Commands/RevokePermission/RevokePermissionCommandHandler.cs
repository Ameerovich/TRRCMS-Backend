using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.RevokePermission;

/// <summary>
/// Handler for RevokePermissionCommand
/// Revokes a single permission from a user with audit trail
/// </summary>
public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public RevokePermissionCommandHandler(
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

    public async Task<UserDto> Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
    {
        // 1) Get current user (who is revoking permission)
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // 2) Get target user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        // 3) Check if user has the permission (before attempting to revoke)
        var userPermission = await _userRepository.GetUserPermissionAsync(user.Id, request.Permission, cancellationToken);
        if (userPermission == null || !userPermission.IsValid())
        {
            // No permission to revoke
            throw new ValidationException($"User '{user.Username}' does not have permission '{request.Permission}' or it is already revoked.");
        }

        // 4) Get permissions before revoking (for audit log)
        var permissionsBefore = user.GetActivePermissions().ToList();

        // 5) Revoke permission using domain method
        user.RevokePermission(
            request.Permission,
            revokedBy: currentUserId,
            reason: request.RevokeReason
        );

        // 6) Save changes (persist the changes in UserPermissions table)
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken); // Ensure changes are saved

        // 7) Get permissions after revoke (for audit log)
        var permissionsAfter = user.GetActivePermissions().ToList();

        // 8) Audit logging (log permission revoke action)
        await _auditService.LogActionAsync(
            actionType: AuditActionType.PermissionRevoked,  // Action type updated
            actionDescription: $"Revoked permission '{request.Permission}' from user '{user.Username}'. Reason: {request.RevokeReason}",
            entityType: "User",
            entityId: user.Id,
            entityIdentifier: user.Username,
            oldValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                PermissionCount = permissionsBefore.Count,
                RevokedPermission = request.Permission.ToString()
            }),
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                PermissionCount = permissionsAfter.Count
            }),
            changedFields: "Permissions",
            cancellationToken: cancellationToken
        );

        // 9) Return updated user
        return _mapper.Map<UserDto>(user);
    }
}
