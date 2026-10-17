using MediatR;

namespace TRRCMS.Application.Auth.Commands.RequestPasswordReset;

/// <summary>
/// Anonymous "forgot password" request. Queues the request for an administrator;
/// always completes silently so the endpoint cannot be used to discover usernames.
/// </summary>
public class RequestPasswordResetCommand : IRequest<Unit>
{
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Optional note for the administrator (e.g. phone number or office)
    /// </summary>
    public string? Note { get; set; }
}
