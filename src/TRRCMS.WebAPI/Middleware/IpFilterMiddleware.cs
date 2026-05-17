using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.WebAPI.Middleware;

/// <summary>
/// Enforces the active <c>AccessControlPolicy</c> IP allow/deny lists.
/// Loads the active <c>SecurityPolicy</c> per request via the scoped repository (one
/// indexed read against the singleton-ish policy table — adequate at current scale; can
/// be replaced with a cached policy provider later).
///
/// Behavior:
///   - If the caller's IP appears in <c>IpDenylist</c>, return 403 immediately, regardless of
///     <c>EnforceIpAllowlist</c>.
///   - If <c>EnforceIpAllowlist</c> is true and <c>IpAllowlist</c> doesn't match the caller, return 403.
///   - Both lists are comma-separated; entries can be plain IPv4/IPv6 addresses or CIDR ranges (e.g. "10.0.0.0/24").
///
/// Loopback (127.0.0.1, ::1) is always allowed so local development and the API's own
/// health checks aren't blocked even if the operator misconfigures the lists.
/// </summary>
public class IpFilterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpFilterMiddleware> _logger;

    public IpFilterMiddleware(RequestDelegate next, ILogger<IpFilterMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISecurityPolicyRepository securityPolicyRepository)
    {
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp == null)
        {
            // No discernible source address — let it pass; downstream auth still applies.
            await _next(context);
            return;
        }

        // Always allow loopback; we don't want to brick local dev / health probes.
        if (IPAddress.IsLoopback(remoteIp))
        {
            await _next(context);
            return;
        }

        var policy = await securityPolicyRepository.GetActiveAsync(context.RequestAborted);
        if (policy == null)
        {
            await _next(context);
            return;
        }

        var acp = policy.AccessControlPolicy;

        // Denylist wins regardless of other settings.
        if (IpMatchesAny(remoteIp, acp.IpDenylist))
        {
            _logger.LogWarning("IP {Ip} blocked by AccessControlPolicy denylist (policy v{Version})",
                remoteIp, policy.Version);
            await WriteForbiddenAsync(context, "Your IP address is denied by the security policy.");
            return;
        }

        if (acp.EnforceIpAllowlist)
        {
            if (!IpMatchesAny(remoteIp, acp.IpAllowlist))
            {
                _logger.LogWarning("IP {Ip} not in AccessControlPolicy allowlist (policy v{Version})",
                    remoteIp, policy.Version);
                await WriteForbiddenAsync(context, "Your IP address is not in the security policy allowlist.");
                return;
            }
        }

        await _next(context);
    }

    private static async Task WriteForbiddenAsync(HttpContext context, string message)
    {
        if (context.Response.HasStarted) return;
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new
        {
            status = 403,
            title = "Forbidden",
            message
        });
        await context.Response.WriteAsync(body);
    }

    /// <summary>
    /// Returns true when <paramref name="remoteIp"/> matches any comma-separated entry
    /// in <paramref name="list"/>. Entries may be a single address or a CIDR range. Invalid
    /// entries are silently skipped (logged at the call site if needed) rather than causing
    /// a hard failure for the whole request — operator-misconfigured lists should fail open
    /// for non-blocked traffic and not produce 500s.
    /// </summary>
    private static bool IpMatchesAny(IPAddress remoteIp, string? list)
    {
        if (string.IsNullOrWhiteSpace(list)) return false;

        foreach (var raw in list.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (TryMatch(remoteIp, raw)) return true;
        }
        return false;
    }

    private static bool TryMatch(IPAddress remoteIp, string entry)
    {
        // CIDR (a.b.c.d/p or [ipv6]/p)
        var slash = entry.IndexOf('/');
        if (slash > 0)
        {
            var addrPart = entry[..slash];
            var prefixPart = entry[(slash + 1)..];
            if (!IPAddress.TryParse(addrPart, out var networkAddr)) return false;
            if (!int.TryParse(prefixPart, out var prefixLength)) return false;
            return IsInCidr(remoteIp, networkAddr, prefixLength);
        }

        // Plain address
        return IPAddress.TryParse(entry, out var parsed) && remoteIp.Equals(parsed);
    }

    private static bool IsInCidr(IPAddress remoteIp, IPAddress networkAddr, int prefixLength)
    {
        if (remoteIp.AddressFamily != networkAddr.AddressFamily) return false;

        var ipBytes = remoteIp.GetAddressBytes();
        var netBytes = networkAddr.GetAddressBytes();
        if (ipBytes.Length != netBytes.Length) return false;

        int totalBits = ipBytes.Length * 8;
        if (prefixLength < 0 || prefixLength > totalBits) return false;

        int fullBytes = prefixLength / 8;
        int remainingBits = prefixLength % 8;

        for (int i = 0; i < fullBytes; i++)
        {
            if (ipBytes[i] != netBytes[i]) return false;
        }

        if (remainingBits == 0) return true;

        int mask = unchecked((byte)(0xFF << (8 - remainingBits)));
        return (ipBytes[fullBytes] & mask) == (netBytes[fullBytes] & mask);
    }
}
