using MediatR;
using TRRCMS.Domain.Enums;
using TRRCMS.Application.Users.Dtos; // Importing the DTOs

namespace TRRCMS.Application.Users.Queries.GetAllUsers
{
    /// <summary>
    /// Query to fetch all users with optional filters for role, activity, and search term.
    /// This query returns a paginated list of users.
    /// </summary>
    public class GetAllUsersQuery : IRequest<GetAllUsersResponse>
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

        /// <summary>
        /// The page number for pagination.
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// The number of users per page for pagination.
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}
