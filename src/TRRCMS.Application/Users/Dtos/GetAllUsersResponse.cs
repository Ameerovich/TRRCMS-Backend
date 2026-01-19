
namespace TRRCMS.Application.Users.Dtos
{
    /// <summary>
    /// Response DTO for fetching all users with pagination and optional filters.
    /// </summary>
    public class GetAllUsersResponse
    {
        /// <summary>
        /// List of users returned in the response.
        /// </summary>
        public List<UserListDto> Users { get; set; } = new();

        /// <summary>
        /// Total number of users matching the filters.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of users per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages based on the total count and page size.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
