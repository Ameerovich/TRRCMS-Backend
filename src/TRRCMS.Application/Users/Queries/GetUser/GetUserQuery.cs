using MediatR;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Queries.GetUser
{
    /// <summary>
    /// Query to fetch a user by their unique identifier.
    /// This query returns the detailed information of a user.
    /// </summary>
    public class GetUserQuery : IRequest<UserDetailDto>
    {
        /// <summary>
        /// The unique identifier of the user to retrieve.
        /// </summary>
        public Guid UserId { get; set; }
    }
}
