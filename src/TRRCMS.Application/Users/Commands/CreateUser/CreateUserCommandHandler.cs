using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;
using TRRCMS.Application.Common.Services;

namespace TRRCMS.Application.Users.Commands.CreateUser;

/// <summary>
/// Handler for CreateUserCommand
/// Creates new user with hashed password and default role-based permissions
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Get current user (who is creating this user)
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Check username uniqueness
        var existingUserByUsername = await _userRepository.GetByUsernameAsync(
            request.Username,
            cancellationToken);

        if (existingUserByUsername != null)
        {
            throw new ValidationException($"Username '{request.Username}' is already taken");
        }

        // Check email uniqueness
        var existingUserByEmail = await _userRepository.GetByEmailAsync(
            request.Email,
            cancellationToken);

        if (existingUserByEmail != null)
        {
            throw new ValidationException($"Email '{request.Email}' is already in use");
        }

        // Hash password with salt
        string passwordSalt;
        string passwordHash = _passwordHasher.HashPassword(request.Password, out passwordSalt);

        // Create user entity
        var user = User.Create(
            username: request.Username,
            fullNameArabic: request.FullNameArabic,
            passwordHash: passwordHash,
            passwordSalt: passwordSalt,
            role: request.Role,
            hasMobileAccess: request.HasMobileAccess,
            hasDesktopAccess: request.HasDesktopAccess,
            email: request.Email,
            phoneNumber: request.PhoneNumber,
            createdByUserId: currentUserId
        );

        // Set optional fields using UpdateProfile
        user.UpdateProfile(
            fullNameArabic: request.FullNameArabic,
            fullNameEnglish: request.FullNameEnglish,
            email: request.Email,
            phoneNumber: request.PhoneNumber,
            organization: request.Organization,
            jobTitle: request.JobTitle,
            modifiedByUserId: currentUserId
        );

        // Grant default permissions based on role
        var defaultPermissions = PermissionSeeder.GetDefaultPermissionsForRole(request.Role);
        user.GrantPermissions(
            defaultPermissions,
            grantedBy: currentUserId,
            reason: $"Default permissions for role {request.Role}"
        );

        // Save to repository
        await _userRepository.AddAsync(user, cancellationToken);

        // Audit logging
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created user '{user.Username}' with role {user.Role}",
            entityType: "User",
            entityId: user.Id,
            entityIdentifier: user.Username,
            oldValues: null,
            newValues: System.Text.Json.JsonSerializer.Serialize(new
            {
                user.Username,
                user.FullNameArabic,
                user.Email,
                user.Role,
                user.HasMobileAccess,
                user.HasDesktopAccess,
                PermissionsGranted = defaultPermissions.Count()
            }),
            changedFields: "New User",
            cancellationToken: cancellationToken
        );

        // Return DTO
        return _mapper.Map<UserDto>(user);
    }
}