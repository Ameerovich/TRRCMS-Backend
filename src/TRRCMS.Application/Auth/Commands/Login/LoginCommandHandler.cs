using MediatR;
using Microsoft.Extensions.Configuration;
using TRRCMS.Application.Auth.Dtos;
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

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Find user by username
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        // Step 2: Check if account is locked
        if (user.IsLockedOut)
        {
            // Check if lockout has expired
            if (!user.IsLockoutExpired())
            {
                var remainingTime = user.LockoutEndDate!.Value - DateTime.UtcNow;
                throw new UnauthorizedAccessException(
                    $"Account is locked due to multiple failed login attempts. " +
                    $"Please try again in {remainingTime.Minutes} minutes.");
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
            throw new UnauthorizedAccessException("Account is inactive. Please contact your administrator.");
        }

        // Step 4: Verify password
        bool isPasswordValid = _passwordHasher.VerifyPassword(
            request.Password,
            user.PasswordHash,
            user.PasswordSalt);

        if (!isPasswordValid)
        {
            // Record failed login attempt
            user.RecordFailedLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        // Step 5: Check if password is expired
        if (user.IsPasswordExpired())
        {
            throw new UnauthorizedAccessException(
                "Your password has expired. Please contact your administrator to reset it.");
        }

        // Step 6: Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user, request.DeviceId);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Get token expiration settings
        int accessTokenExpirationMinutes = int.Parse(
            _configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15");
        int refreshTokenExpirationDays = int.Parse(
            _configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");

        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes);
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenExpirationDays);

        // Step 7: Update user's refresh token and login tracking
        user.UpdateRefreshToken(refreshToken, refreshTokenExpiry);
        user.RecordSuccessfulLogin();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Step 8: Build and return response
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
            MustChangePassword = user.MustChangePassword
        };
    }
}