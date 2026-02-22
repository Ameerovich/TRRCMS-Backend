using MediatR;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Queries.GetAllUsers;

/// <summary>
/// Query to fetch all users with optional filters for role, activity, and search term.
/// This query returns a paginated list of users.
/// </summary>
public class GetAllUsersQuery : PagedQuery, IRequest<PagedResult<UserListDto>>
{
    /// <summary>
    /// The role of the user to filter by (optional).
    /// </summary>
    public UserRole? Role { get; set; }

    /// <summary>
    /// The active status of the user to filter by (optional).
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// The search term to filter by (username, name, email) (optional).
    /// </summary>
    public string? SearchTerm { get; set; }
}
