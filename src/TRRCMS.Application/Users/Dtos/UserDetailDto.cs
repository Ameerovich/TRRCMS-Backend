using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Dtos;

/// <summary>
/// Detailed DTO for single user view
/// Includes full permissions list for UC-009 User Management requirements
/// </summary>
public class UserDetailDto : UserDto
{
    /// <summary>
    /// List of active permissions granted to this user
    /// Format: Permission enum names as strings (e.g., "Claims_ViewAll")
    /// </summary>
    public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Count of active permissions
    /// </summary>
    public int ActivePermissionsCount { get; set; }
}