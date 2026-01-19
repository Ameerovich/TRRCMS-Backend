using AutoMapper;
using MediatR;
using System.Text.Json;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.GrantPermissions;

/// <summary>
/// Handler for GrantPermissionsCommand
/// Grants multiple permissions to a user with audit trail.
/// Uses explicit repository persistence for UserPermission to avoid EF tracking/concurrency issues.
/// </summary>
public class GrantPermissionsCommandHandler : IRequestHandler<GrantPermissionsCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public GrantPermissionsCommandHandler(
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

    public async Task<UserDto> Handle(GrantPermissionsCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Load user (for validation + return DTO)
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        // Snapshot before
        var before = user.GetActivePermissions().ToList();

        var requested = (request.Permissions ?? new List<Permission>())
            .Distinct()
            .ToList();

        // No permissions provided => no-op
        if (requested.Count == 0)
        {
            return _mapper.Map<UserDto>(user);
        }

        var granted = new List<Permission>();

        // Persist permissions explicitly to avoid EF treating new rows as UPDATE
        foreach (var permission in requested)
        {
            var existing = await _userRepository.GetUserPermissionAsync(user.Id, permission, cancellationToken);

            if (existing is null)
            {
                // New permission row => INSERT
                var userPermission = UserPermission.Create(
                    userId: user.Id,
                    permission: permission,
                    grantedBy: currentUserId,
                    grantReason: request.GrantReason);

                await _userRepository.AddUserPermissionAsync(userPermission, cancellationToken);
                granted.Add(permission);
                continue;
            }

            // Exists
            if (existing.IsValid())
            {
                // Already active => ignore (requirement: duplicates ignored)
                continue;
            }

            // Reactivate existing row => UPDATE
            existing.Reactivate(currentUserId, request.GrantReason);
            await _userRepository.UpdateUserPermissionAsync(existing, cancellationToken);
            granted.Add(permission);
        }

        // Save once
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Reload user (to return updated permissions state if needed)
        user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        var after = user.GetActivePermissions().ToList();

        // Audit (only if something changed)
        if (granted.Count > 0)
        {
            await _auditService.LogActionAsync(
                actionType: AuditActionType.PermissionGranted,
                actionDescription: $"Granted {granted.Count} permission(s) to user '{user.Username}'. Reason: {request.GrantReason}",
                entityType: "User",
                entityId: user.Id,
                entityIdentifier: user.Username,
                oldValues: JsonSerializer.Serialize(new
                {
                    PermissionCount = before.Count,
                    Permissions = before.Select(p => p.ToString()).ToList()
                }),
                newValues: JsonSerializer.Serialize(new
                {
                    PermissionCount = after.Count,
                    Permissions = after.Select(p => p.ToString()).ToList(),
                    GrantedPermissions = granted.Select(p => p.ToString()).ToList()
                }),
                changedFields: "Permissions",
                cancellationToken: cancellationToken);
        }

        return _mapper.Map<UserDto>(user);
    }
}
