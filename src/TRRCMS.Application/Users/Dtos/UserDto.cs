using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Dtos;

/// <summary>
/// Base data transfer object for User entity
/// Used for create/update operations and basic user info
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FullNameArabic { get; set; } = string.Empty;
    public string? FullNameEnglish { get; set; }
    public string? EmployeeId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Organization { get; set; }
    public string? JobTitle { get; set; }
    public UserRole Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool HasMobileAccess { get; set; }
    public bool HasDesktopAccess { get; set; }
    public bool IsActive { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? LockoutEndDate { get; set; }
    public int FailedLoginAttempts { get; set; } 
    public DateTime? LastLoginDate { get; set; }
    public DateTime? LastPasswordChangeDate { get; set; }
    public bool MustChangePassword { get; set; }
    public string? AssignedTabletId { get; set; }
    public DateTime? TabletAssignedDate { get; set; }
    public Guid? SupervisorUserId { get; set; }
    public string? SupervisorName { get; set; }
    public string? TeamName { get; set; }
    public string PreferredLanguage { get; set; } = string.Empty;
    public bool TwoFactorEnabled { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
}