using MediatR;

namespace TRRCMS.Application.Users.Commands.DismissPasswordResetRequest;

/// <summary>
/// Administrator dismisses a user's pending "forgot password" request without resetting the password
/// (e.g. the request was not made by the account owner).
/// </summary>
public class DismissPasswordResetRequestCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
}
