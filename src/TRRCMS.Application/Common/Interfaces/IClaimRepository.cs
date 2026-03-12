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

    // ==================== QUERY BY WORKFLOW STATES ====================

    /// <summary>
    /// Get all claims by case status
    /// </summary>
    Task<IEnumerable<Claim>> GetByCaseStatusAsync(CaseStatus caseStatus, CancellationToken cancellationToken = default);

    // ==================== FILTERED QUERY ====================

    /// <summary>
    /// Get claims with combined server-side filtering.
    /// Includes PropertyUnit (with Building) and PrimaryClaimant navigation properties.
    /// All filters are AND-combined; null filters are ignored.
    /// </summary>
    Task<List<Claim>> GetFilteredAsync(
        CaseStatus? caseStatus,
        ClaimSource? source,
        Guid? createdByUserId,
        Guid? claimId,
        string? buildingCode = null,
        Guid? propertyUnitId = null,
        Guid? originatingSurveyId = null,
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

    // ==================== AGGREGATE QUERIES ====================

    /// <summary>
    /// Get count of claims by case status
    /// </summary>
    Task<int> GetCountByCaseStatusAsync(CaseStatus caseStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total claims count
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    // ==================== GROUPED COUNTS (Dashboard) ====================

    /// <summary>
    /// Get count of claims grouped by case status.
    /// Used for dashboard summary tiles.
    /// </summary>
    Task<Dictionary<CaseStatus, int>> GetCaseStatusCountsAsync(CancellationToken cancellationToken = default);

    // ==================== DASHBOARD EXTENDED QUERIES ====================

    /// <summary>
    /// Get count of claims grouped by claim type (OwnershipClaim/OccupancyClaim).
    /// </summary>
    Task<Dictionary<ClaimType, int>> GetClaimTypeCountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get monthly creation counts for time-series trends.
    /// </summary>
    Task<List<(int Year, int Month, int Count)>> GetMonthlyCreationCountsAsync(
        DateTime? from = null, DateTime? to = null,
        CancellationToken cancellationToken = default);
}
