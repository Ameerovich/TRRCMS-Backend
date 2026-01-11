using MediatR;
using TRRCMS.Application.Auth.Dtos;

namespace TRRCMS.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Command to refresh an expired access token using a refresh token
/// </summary>
public class RefreshTokenCommand : IRequest<RefreshTokenResponse>
{
    public string RefreshToken { get; set; } = string.Empty;
    public string? DeviceId { get; set; }

    public RefreshTokenCommand(string refreshToken, string? deviceId = null)
    {
        RefreshToken = refreshToken;
        DeviceId = deviceId;
    }
}