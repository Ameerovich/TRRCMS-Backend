using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Auth.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Auth.Commands.RefreshToken;

/// <summary>
/// Handler for refreshing access tokens
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IConfiguration configuration,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Find user by refresh token
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Refresh token rejected: no user for token (device {DeviceId})",
                request.DeviceId ?? "unknown");
            throw new InvalidCredentialsException(
                "Message_InvalidRefreshToken",
                "Invalid or expired refresh token.");
        }

        // Step 2: Validate refresh token expiry
        if (user.RefreshTokenExpiryDate == null || user.RefreshTokenExpiryDate < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token expired for user {UserId} (device {DeviceId})",
                user.Id, request.DeviceId ?? "unknown");
            throw new InvalidCredentialsException(
                "Message_RefreshTokenExpired",
                "Refresh token has expired. Please login again.");
        }

        // Step 3: Check if account is still active
        if (!user.IsActive)
        {
            _logger.LogWarning("Refresh rejected for inactive user {UserId}", user.Id);
            throw new InvalidCredentialsException(
                "Message_AccountInactive",
                "Account is inactive. Please contact your administrator.");
        }

        // Step 4: Check if account is locked
        if (user.IsLockedOut && !user.IsLockoutExpired())
        {
            _logger.LogWarning("Refresh rejected for locked user {UserId}", user.Id);
            throw new InvalidCredentialsException(
                "Message_AccountLockedNoCountdown",
                "Account is locked.");
        }

        // Step 4b: Block refresh if user must change password
        if (user.MustChangePassword)
        {
            _logger.LogWarning("Refresh rejected for user {UserId}: must change password", user.Id);
            throw new InvalidCredentialsException(
                "Message_MustChangePassword",
                "You must change your password before continuing. Please login and change your password.");
        }

        // Step 5: Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user, request.DeviceId);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Get token expiration settings
        int accessTokenExpirationMinutes = int.Parse(
            _configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15");
        int refreshTokenExpirationDays = int.Parse(
            _configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");

        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);

        // Step 6: Update user's refresh token
        user.UpdateRefreshToken(newRefreshToken, refreshTokenExpiry);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Step 7: Return new tokens
        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiry = accessTokenExpiry,
            RefreshTokenExpiry = refreshTokenExpiry
        };
    }
}