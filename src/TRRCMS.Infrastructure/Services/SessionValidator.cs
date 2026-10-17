using Microsoft.Extensions.Caching.Memory;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Infrastructure.Services;

/// <inheritdoc />
public class SessionValidator : ISessionValidator
{
    // Short TTL bounds the DB cost to ~1 small query per user per 30s; explicit
    // Invalidate() calls make password resets and (de)activations take effect immediately.
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    public const string RevokedMessageKey = "Message_SessionRevoked";

    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;

    public SessionValidator(IUserRepository userRepository, IMemoryCache cache)
    {
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<string?> ValidateAsync(Guid userId, string? securityStamp, CancellationToken cancellationToken = default)
    {
        var state = await _cache.GetOrCreateAsync(CacheKey(userId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await _userRepository.GetSessionStateAsync(userId, cancellationToken);
        });

        if (state is not { IsActive: true })
            return RevokedMessageKey;

        return string.Equals(state.Value.SecurityStamp, securityStamp, StringComparison.Ordinal)
            ? null
            : RevokedMessageKey;
    }

    public void Invalidate(Guid userId) => _cache.Remove(CacheKey(userId));

    private static string CacheKey(Guid userId) => $"session-state:{userId}";
}
