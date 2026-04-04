using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

public interface IIdentificationDocumentRepository
{
    Task<IdentificationDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<IdentificationDocument>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);
    Task AddAsync(IdentificationDocument entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(IdentificationDocument entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
