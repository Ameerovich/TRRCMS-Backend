using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Auth.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Auth.Commands.Login;

/// <summary>
/// Handler for user login authentication
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ISecurityPolicyRepository _securityPolicyRepository;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IConfiguration configuration,
        ISecurityPolicyRepository securityPolicyRepository,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _configuration = configuration;
        _securityPolicyRepository = securityPolicyRepository;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Find user by username
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for unknown username {Username}", request.Username);
            throw new InvalidCredentialsException(
                "Message_InvalidCredentials",
                "Invalid username or password.");
        }

        // Step 2: Check if account is locked
        if (user.IsLockedOut)
        {
            // Check if lockout has expired
            if (!user.IsLockoutExpired())
            {
                var remainingTime = user.LockoutEndDate!.Value - DateTime.UtcNow;
                _logger.LogWarning(
                    "Failed login attempt for locked account {Username}, remaining lockout minutes: {Minutes}",
                    request.Username, remainingTime.Minutes);
                throw new InvalidCredentialsException(
                    "Message_AccountLocked",
                    $"Account is locked due to multiple failed login attempts. Please try again in {remainingTime.Minutes} minutes.",
                    remainingTime.Minutes);
            }
            else
            {
                // Lockout has expired, unlock the account
                user.Unlock(user.Id); // Self-unlock
                await _userRepository.UpdateAsync(user, cancellationToken);
                await _userRepository.SaveChangesAsync(cancellationToken);
            }
        }

        // Step 3: Check if account is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Failed login attempt for inactive account {Username}", request.Username);
            throw new InvalidCredentialsException(
                "Message_AccountInactive",
                "Account is inactive. Please contact your administrator.");
        }

        // Step 4: Verify password
        bool isPasswordValid = _passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash,
            user.PasswordSalt);

        if (!isPasswordValid)
        {
            // Record failed login attempt using active security policy lockout settings
            var policy = await _securityPolicyRepository.GetActiveAsync(cancellationToken);
            int maxAttempts = policy?.SessionLockoutPolicy.MaxFailedLoginAttempts ?? 5;
            int lockoutMinutes = policy?.SessionLockoutPolicy.LockoutDurationMinutes ?? 30;
            user.RecordFailedLogin(maxAttempts, lockoutMinutes);
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Failed login attempt for username {Username}: wrong password", request.Username);
            throw new InvalidCredentialsException(
                "Message_InvalidCredentials",
                "Invalid username or password.");
        }

        // Step 5: Check if user must change password (first login)
        if (user.MustChangePassword)
        {
            // Generate restricted token — only allows access to change-password endpoint
            var limitedToken = _tokenService.GenerateAccessToken(user, request.DeviceId, isPasswordChangeOnly: true);
            var limitedExpiry = DateTime.UtcNow.AddMinutes(10);

            user.RecordSuccessfulLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            return new LoginResponse
            {
                UserId = user.Id,
                Username = user.Username,
                FullNameArabic = user.FullNameArabic,
                FullNameEnglish = user.FullNameEnglish,
                Email = user.Email,
                Role = user.Role,
                RoleName = user.Role.ToString(),
                HasMobileAccess = user.HasMobileAccess,
                HasDesktopAccess = user.HasDesktopAccess,
                PreferredLanguage = user.PreferredLanguage,
                AccessToken = limitedToken,
                RefreshToken = null,
                AccessTokenExpiry = limitedExpiry,
                RefreshTokenExpiry = null,
                MustChangePassword = true
            };
        }

        // Step 6: Check if password is expired
        if (user.IsPasswordExpired())
        {
            _logger.LogWarning("Failed login attempt for username {Username}: password expired", request.Username);
            throw new InvalidCredentialsException(
                "Message_PasswordExpired",
                "Your password has expired. Please contact your administrator to reset it.");
        }

        // Step 7: Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user, request.DeviceId);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Get token expiration settings
        int accessTokenExpirationMinutes = int.Parse(
            _configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15");
        int refreshTokenExpirationDays = int.Parse(
            _configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");

        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);

        // Step 8: Update user's refresh token and login tracking
        user.UpdateRefreshToken(refreshToken, refreshTokenExpiry);
        user.RecordSuccessfulLogin();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Step 9: Build and return response
        return new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            FullNameArabic = user.FullNameArabic,
            FullNameEnglish = user.FullNameEnglish,
            Email = user.Email,
            Role = user.Role,
            RoleName = user.Role.ToString(),
            HasMobileAccess = user.HasMobileAccess,
            HasDesktopAccess = user.HasDesktopAccess,
            PreferredLanguage = user.PreferredLanguage,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = accessTokenExpiry,
            RefreshTokenExpiry = refreshTokenExpiry,
            MustChangePassword = false
        };
    }
}