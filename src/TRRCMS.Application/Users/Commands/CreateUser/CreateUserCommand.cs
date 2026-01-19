using MediatR;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Enums;
namespace TRRCMS.Application.Users.Commands.CreateUser;

/// <summary>
/// Create new user command
/// UC-009: User & Role Management
/// </summary>
public class CreateUserCommand : IRequest<UserDto>
{
    /// <summary>
    /// Username for login (unique, required)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Full name in Arabic (required)
    /// </summary>
    public string FullNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// Full name in English (optional)
    /// </summary>
    public string? FullNameEnglish { get; set; }

    /// <summary>
    /// Email address (unique, required)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number (optional)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// User role (required)
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Initial password (required, will be hashed)
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Mobile app access flag
    /// </summary>
    public bool HasMobileAccess { get; set; }

    /// <summary>
    /// Desktop app access flag
    /// </summary>
    public bool HasDesktopAccess { get; set; }

    /// <summary>
    /// Organization/department (optional)
    /// </summary>
    public string? Organization { get; set; }

    /// <summary>
    /// Job title (optional)
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Employee ID (optional)
    /// </summary>
    public string? EmployeeId { get; set; }
}