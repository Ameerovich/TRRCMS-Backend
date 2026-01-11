using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

using JwtClaim = System.Security.Claims.Claim;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// JWT Token Service for generating and validating access and refresh tokens
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;

        // Read JWT settings from appsettings.json
        _secretKey = _configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured");
        _issuer = _configuration["JwtSettings:Issuer"] ?? "TRRCMS.API";
        _audience = _configuration["JwtSettings:Audience"] ?? "TRRCMS.Clients";
        _accessTokenExpirationMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15");
    }

    /// <summary>
    /// Generate a JWT access token with user claims
    /// </summary>
    public string GenerateAccessToken(User user, string? deviceId = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        // Create claims for the JWT
        var claims = new List<JwtClaim>
        {
            new JwtClaim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new JwtClaim(ClaimTypes.Name, user.Username),
            new JwtClaim(ClaimTypes.Role, user.Role.ToString()),
            new JwtClaim("full_name_arabic", user.FullNameArabic),
            new JwtClaim("security_stamp", user.SecurityStamp),
            new JwtClaim("has_mobile_access", user.HasMobileAccess.ToString()),
            new JwtClaim("has_desktop_access", user.HasDesktopAccess.ToString())
        };

        // Add optional claims
        if (!string.IsNullOrEmpty(user.Email))
            claims.Add(new JwtClaim(ClaimTypes.Email, user.Email));

        if (!string.IsNullOrEmpty(deviceId))
            claims.Add(new JwtClaim("device_id", deviceId));

        // Token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generate a cryptographically secure refresh token
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Extract user ID from JWT token without full validation
    /// </summary>
    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validate JWT token signature and expiration
    /// </summary>
    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}