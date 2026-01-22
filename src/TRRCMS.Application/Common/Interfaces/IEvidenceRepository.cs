using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Evidence entity operations
/// </summary>
public interface IEvidenceRepository
{
    /// <summary>
    /// Get evidence by ID
    /// </summary>
    Task<Evidence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all evidences
    /// </summary>
    Task<IEnumerable<Evidence>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get evidences by person ID
    /// </summary>
    Task<IEnumerable<Evidence>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get evidences by person-property relation ID
    /// </summary>
    Task<IEnumerable<Evidence>> GetByRelationIdAsync(Guid relationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get evidences by claim ID
    /// </summary>
    Task<IEnumerable<Evidence>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get only current versions of evidences (IsCurrentVersion = true)
    /// </summary>
    Task<IEnumerable<Evidence>> GetCurrentVersionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get evidence history (all versions) for a specific evidence
    /// </summary>
    /// /// <summary>
    /// Get all evidence for a survey context (by persons and property relations in survey's building)
    /// </summary>
    Task<List<Evidence>> GetBySurveyContextAsync(
        Guid buildingId,
        string? evidenceType = null,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Evidence>> GetVersionHistoryAsync(Guid evidenceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if evidence exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new evidence
    /// </summary>
    Task<Evidence> AddAsync(Evidence evidence, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update evidence
    /// </summary>
    Task UpdateAsync(Evidence evidence, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes to database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
