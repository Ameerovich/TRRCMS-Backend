using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for EvidenceRelation join entity operations
/// </summary>
public interface IEvidenceRelationRepository
{
    Task<EvidenceRelation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EvidenceRelation>> GetByEvidenceIdAsync(Guid evidenceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EvidenceRelation>> GetActiveByEvidenceIdAsync(Guid evidenceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EvidenceRelation>> GetByRelationIdAsync(Guid relationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EvidenceRelation>> GetActiveByRelationIdAsync(Guid relationId, CancellationToken cancellationToken = default);
    Task<EvidenceRelation?> GetByEvidenceAndRelationAsync(Guid evidenceId, Guid relationId, CancellationToken cancellationToken = default);
    Task<bool> LinkExistsAsync(Guid evidenceId, Guid relationId, CancellationToken cancellationToken = default);
    Task<EvidenceRelation> AddAsync(EvidenceRelation evidenceRelation, CancellationToken cancellationToken = default);
    Task UpdateAsync(EvidenceRelation evidenceRelation, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
