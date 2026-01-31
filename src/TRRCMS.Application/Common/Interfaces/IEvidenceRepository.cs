using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Evidence entity operations
/// </summary>
public interface IEvidenceRepository
{
    Task<Evidence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Evidence>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Evidence>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Evidence>> GetByRelationIdAsync(Guid relationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Evidence>> GetByRelationIdAsync(Guid relationId, EvidenceType? evidenceType, bool onlyCurrentVersions, CancellationToken cancellationToken = default);
    Task<IEnumerable<Evidence>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Evidence>> GetCurrentVersionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all evidence for a survey context (by persons and property relations in survey's building)
    /// </summary>
    Task<List<Evidence>> GetBySurveyContextAsync(
        Guid buildingId,
        EvidenceType? evidenceType = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Evidence>> GetVersionHistoryAsync(Guid evidenceId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Evidence> AddAsync(Evidence evidence, CancellationToken cancellationToken = default);
    Task UpdateAsync(Evidence evidence, CancellationToken cancellationToken = default);
    Task DeleteAsync(Evidence evidence, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
