using MediatR;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Queries.GetUserAuditLog
{
    /// <summary>
    /// Query to retrieve recent audit activity for a specific user.
    /// Used by UC-009 (User Management) to show user history / activity trail.
    /// </summary>
    public class GetUserAuditLogQuery : IRequest<List<AuditLogDto>>
    {
        /// <summary>
        /// Target user id to fetch audit logs for.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Max number of entries to return (defaults to 50).
        /// </summary>
        public int Count { get; set; } = 50;
    }
}
