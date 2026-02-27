using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for SecurityPolicy aggregate.
/// UC-011: Security Settings.
/// 
/// SecurityPolicy uses a versioned singleton pattern:
///   - Only one policy is active at any time.
///   - Previous versions are preserved for audit trail.
/// </summary>
public interface ISecurityPolicyRepository
{
    /// <summary>
    /// Get the currently active (enforced) security policy.
    /// Returns null only if no policy has ever been seeded.
    /// </summary>
    Task<SecurityPolicy?> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific policy version by its ID.
    /// </summary>
    Task<SecurityPolicy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the highest version number currently stored.
    /// Returns 0 if no policies exist.
    /// </summary>
    Task<int> GetLatestVersionNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all policy versions ordered by version descending (newest first).
    /// Used for UC-011 audit: view full history of security policy changes.
    /// </summary>
    Task<List<SecurityPolicy>> GetVersionHistoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new security policy version.
    /// </summary>
    Task<SecurityPolicy> AddAsync(SecurityPolicy policy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing security policy (e.g., to deactivate a superseded version).
    /// </summary>
    Task UpdateAsync(SecurityPolicy policy, CancellationToken cancellationToken = default);
}
