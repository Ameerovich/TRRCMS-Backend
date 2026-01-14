using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Infrastructure.Persistence;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Service to retrieve current authenticated user information
/// Extracts user details from JWT claims in HttpContext
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        ApplicationDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    /// <summary>
    /// Get current user ID from claims
    /// Looks for "sub" (subject), "userId", or "nameidentifier" claims
    /// </summary>
    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? _httpContextAccessor.HttpContext?.User?
                .FindFirst("sub")?.Value
                ?? _httpContextAccessor.HttpContext?.User?
                .FindFirst("userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    /// <summary>
    /// Get current username from claims
    /// </summary>
    public string? Username
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Name)?.Value
                ?? _httpContextAccessor.HttpContext?.User?
                .FindFirst("username")?.Value;
        }
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }

    /// <summary>
    /// Get current user's IP address
    /// </summary>
    public string? IpAddress
    {
        get
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
    }

    /// <summary>
    /// Get source application (Mobile, Desktop, API)
    /// From custom header X-Source-Application
    /// </summary>
    public string? SourceApplication
    {
        get
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["X-Source-Application"].ToString()
                ?? "Desktop"; // Default to Desktop
        }
    }

    /// <summary>
    /// Get full User entity with permissions eager loaded
    /// </summary>
    public async Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!UserId.HasValue)
            return null;

        return await _context.Users
            .Include(u => u.Permissions.Where(p => p.IsActive && !p.IsDeleted))
            .FirstOrDefaultAsync(u => u.Id == UserId.Value, cancellationToken);
    }

}
