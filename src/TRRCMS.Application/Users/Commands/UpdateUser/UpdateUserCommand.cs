using MediatR;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.UpdateUser;

/// <summary>
/// Update existing user command
/// UC-009: User & Role Management
/// </summary>
public class UpdateUserCommand : IRequest<UserDto>
{
    public Guid UserId { get; set; }

    // Profile fields (all optional - only update what's provided)
    public string? FullNameArabic { get; set; }
    public string? FullNameEnglish { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public UserRole? Role { get; set; }
    public string? Organization { get; set; }
    public string? JobTitle { get; set; }
    public string? EmployeeId { get; set; }
}