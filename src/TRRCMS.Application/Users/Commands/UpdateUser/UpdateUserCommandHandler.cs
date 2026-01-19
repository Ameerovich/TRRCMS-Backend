using AutoMapper;
using MediatR;
using System.Text.Json;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.UpdateUser;

/// <summary>
/// Handler for UpdateUserCommand
/// Updates user profile and syncs permissions if role changed
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(
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

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Get current user (who is updating)
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get user to update
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with ID {request.UserId} not found");

        // Store old values for audit
        var oldValues = new
        {
            user.FullNameArabic,
            user.FullNameEnglish,
            user.Email,
            user.PhoneNumber,
            user.Role,
            user.Organization,
            user.JobTitle,
            user.EmployeeId
        };

        var changedFields = new List<string>();

        // Validate email uniqueness if changed
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            var existingUserByEmail = await _userRepository.GetByEmailAsync(
                request.Email,
                cancellationToken);

            if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
            {
                throw new ValidationException($"Email '{request.Email}' is already in use by another user");
            }
        }

        // Update profile using domain method
        user.UpdateProfile(
            fullNameArabic: request.FullNameArabic ?? user.FullNameArabic,
            fullNameEnglish: request.FullNameEnglish ?? user.FullNameEnglish,
            email: request.Email ?? user.Email,
            phoneNumber: request.PhoneNumber ?? user.PhoneNumber,
            organization: request.Organization ?? user.Organization,
            jobTitle: request.JobTitle ?? user.JobTitle,
            modifiedByUserId: currentUserId
        );

        if (request.FullNameArabic != null) changedFields.Add("FullNameArabic");
        if (request.FullNameEnglish != null) changedFields.Add("FullNameEnglish");
        if (request.Email != null) changedFields.Add("Email");
        if (request.PhoneNumber != null) changedFields.Add("PhoneNumber");
        if (request.EmployeeId != null) changedFields.Add("EmployeeId");
        if (request.Organization != null) changedFields.Add("Organization");
        if (request.JobTitle != null) changedFields.Add("JobTitle");

        // Handle role change (requires permission sync) - ✅ Now using Application layer
        if (request.Role.HasValue && request.Role.Value != user.Role)
        {
            var oldRole = user.Role;

            // Update role using domain method
            user.UpdateRole(request.Role.Value, currentUserId);
            changedFields.Add("Role");

            // Revoke old role permissions
            var oldPermissions = PermissionSeeder.GetDefaultPermissionsForRole(oldRole);
            foreach (var permission in oldPermissions)
            {
                user.RevokePermission(
                    permission,
                    revokedBy: currentUserId,
                    reason: $"Role changed from {oldRole} to {request.Role.Value}"
                );
            }

            // Grant new role permissions
            var newPermissions = PermissionSeeder.GetDefaultPermissionsForRole(request.Role.Value);
            user.GrantPermissions(
                newPermissions,
                grantedBy: currentUserId,
                reason: $"New role {request.Role.Value} assigned"
            );

            changedFields.Add("Permissions");
        }

        // Save changes
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Store new values for audit
        var newValues = new
        {
            user.FullNameArabic,
            user.FullNameEnglish,
            user.Email,
            user.PhoneNumber,
            user.Role,
            user.Organization,
            user.JobTitle,
            user.EmployeeId
        };

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated user '{user.Username}'",
            entityType: "User",
            entityId: user.Id,
            entityIdentifier: user.Username,
            oldValues: JsonSerializer.Serialize(oldValues),
            newValues: JsonSerializer.Serialize(newValues),
            changedFields: string.Join(", ", changedFields),
            cancellationToken: cancellationToken
        );

        // Return updated user
        return _mapper.Map<UserDto>(user);
    }
}