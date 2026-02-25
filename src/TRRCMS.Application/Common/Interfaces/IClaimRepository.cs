using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Claim entity operations
/// Provides data access methods for claims with comprehensive query support
/// </summary>
public interface IClaimRepository
{
    // ==================== BASIC CRUD OPERATIONS ====================

    /// <summary>
    /// Get claim by ID
    /// </summary>
    Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims
    /// </summary>
    Task<IEnumerable<Claim>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new claim
    /// </summary>
    Task AddAsync(Claim claim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing claim
    /// </summary>
    Task UpdateAsync(Claim claim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save all changes to database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // ==================== QUERY BY UNIQUE IDENTIFIERS ====================

    /// <summary>
    /// Get claim by claim number
    /// </summary>
    Task<Claim?> GetByClaimNumberAsync(string claimNumber, CancellationToken cancellationToken = default);

    // ==================== QUERY BY RELATIONSHIPS ====================

    /// <summary>
    /// Get claim by property unit ID
    /// </summary>
    Task<Claim?> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims for a property unit (used by PropertyUnit merge to re-point all claims).
    /// Unlike <see cref="GetByPropertyUnitIdAsync"/> which returns a single match,
    /// this returns all claims — a property unit may have multiple claims.
    /// </summary>
    Task<List<Claim>> GetAllByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims by primary claimant (person)
    /// </summary>
    Task<IEnumerable<Claim>> GetByPrimaryClaimantIdAsync(Guid personId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims assigned to a specific user
    /// </summary>
    Task<IEnumerable<Claim>> GetByAssignedUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // ==================== QUERY BY WORKFLOW STATES ====================

    /// <summary>
    /// Get all claims by lifecycle stage
    /// </summary>
    Task<IEnumerable<Claim>> GetByLifecycleStageAsync(LifecycleStage stage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims by status
    /// </summary>
    Task<IEnumerable<Claim>> GetByStatusAsync(ClaimStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims by priority
    /// </summary>
    Task<IEnumerable<Claim>> GetByPriorityAsync(CasePriority priority, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims by verification status
    /// </summary>
    Task<IEnumerable<Claim>> GetByVerificationStatusAsync(VerificationStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims by certificate status
    /// </summary>
    Task<IEnumerable<Claim>> GetByCertificateStatusAsync(CertificateStatus status, CancellationToken cancellationToken = default);

    // ==================== SPECIALIZED QUERIES ====================

    /// <summary>
    /// Get all claims with conflicts
    /// </summary>
    Task<IEnumerable<Claim>> GetConflictingClaimsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all overdue claims (past target completion date)
    /// </summary>
    Task<IEnumerable<Claim>> GetOverdueClaimsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims awaiting documents
    /// </summary>
    Task<IEnumerable<Claim>> GetClaimsAwaitingDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims pending verification
    /// </summary>
    Task<IEnumerable<Claim>> GetClaimsPendingVerificationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims for adjudication (conflicts detected or in adjudication stage)
    /// </summary>
    Task<IEnumerable<Claim>> GetClaimsForAdjudicationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims submitted within date range
    /// </summary>
    Task<IEnumerable<Claim>> GetClaimsBySubmittedDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all claims with decisions within date range
    /// </summary>
    Task<IEnumerable<Claim>> GetClaimsByDecisionDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    // ==================== FILTERED QUERY ====================

    /// <summary>
    /// Get claims with combined server-side filtering.
    /// Includes PropertyUnit (with Building) and PrimaryClaimant navigation properties.
    /// All filters are AND-combined; null filters are ignored.
    /// </summary>
    Task<List<Claim>> GetFilteredAsync(
        ClaimStatus? status,
        ClaimSource? source,
        Guid? createdByUserId,
        Guid? claimId,
        string? buildingCode = null,
        CancellationToken cancellationToken = default);

    // ==================== EXISTENCE CHECKS ====================

    /// <summary>
    /// Check if claim exists by ID
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if claim exists by claim number
    /// </summary>
    Task<bool> ExistsByClaimNumberAsync(string claimNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if property unit has any claims
    /// </summary>
    Task<bool> HasClaimsAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if property unit has conflicting claims
    /// </summary>
    Task<bool> HasConflictingClaimsAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);

    // ==================== AGGREGATE QUERIES ====================

    /// <summary>
    /// Get conflict count for a property unit
    /// </summary>
    Task<int> GetConflictCountAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of claims by lifecycle stage
    /// </summary>
    Task<int> GetCountByLifecycleStageAsync(LifecycleStage stage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of claims by status
    /// </summary>
    Task<int> GetCountByStatusAsync(ClaimStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of claims assigned to user
    /// </summary>
    Task<int> GetCountByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total claims count
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
