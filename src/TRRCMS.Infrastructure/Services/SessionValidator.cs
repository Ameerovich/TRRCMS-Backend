using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Infrastructure.Persistence;

namespace TRRCMS.Infrastructure.Services;

/// <inheritdoc />
public class SessionValidator : ISessionValidator
{
    // Short TTL bounds the DB cost to ~1 small query per user per 30s; explicit
    // Invalidate() calls make password resets and deactivations take effect immediately.
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    public const string RevokedMessageKey = "Message_SessionRevoked";

    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public SessionValidator(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<string?> ValidateAsync(Guid userId, string? securityStamp, CancellationToken cancellationToken = default)
    {
        var state = await _cache.GetOrCreateAsync(CacheKey(userId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId && !u.IsDeleted)
                .Select(u => new SessionState(u.SecurityStamp, u.IsActive))
                .FirstOrDefaultAsync(cancellationToken);
        });

        if (state == null || !state.IsActive)
            return RevokedMessageKey;

        return string.Equals(state.SecurityStamp, securityStamp, StringComparison.Ordinal)
            ? null
            : RevokedMessageKey;
    }

    public void Invalidate(Guid userId) => _cache.Remove(CacheKey(userId));

    private static string CacheKey(Guid userId) => $"session-state:{userId}";

    private sealed record SessionState(string SecurityStamp, bool IsActive);
}
