using MediatR;
using TRRCMS.Application.Auth.Dtos;

namespace TRRCMS.Application.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user and generate tokens
/// </summary>
public class LoginCommand : IRequest<LoginResponse>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DeviceId { get; set; }

    public LoginCommand(string username, string password, string? deviceId = null)
    {
        Username = username;
        Password = password;
        DeviceId = deviceId;
    }
}