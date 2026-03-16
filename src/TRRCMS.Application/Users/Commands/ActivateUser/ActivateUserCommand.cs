using MediatR;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Commands.ActivateUser;

/// <summary>
/// Activate user account command.
/// </summary>
public class ActivateUserCommand : IRequest<UserDto>
{
    /// <summary>
    /// User ID to activate
    /// </summary>
    public Guid UserId { get; set; }
}