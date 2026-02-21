using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

public class EvidenceRelationRepository : IEvidenceRelationRepository
{
    private readonly ApplicationDbContext _context;

    public EvidenceRelationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EvidenceRelation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EvidenceRelations
            .Include(er => er.Evidence)
            .Include(er => er.PersonPropertyRelation)
            .FirstOrDefaultAsync(er => er.Id == id && !er.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<EvidenceRelation>> GetByEvidenceIdAsync(Guid evidenceId, CancellationToken cancellationToken = default)
    {
        return await _context.EvidenceRelations
            .Include(er => er.PersonPropertyRelation)
            .Where(er => er.EvidenceId == evidenceId && !er.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EvidenceRelation>> GetActiveByEvidenceIdAsync(Guid evidenceId, CancellationToken cancellationToken = default)
    {
        return await _context.EvidenceRelations
            .Include(er => er.PersonPropertyRelation)
            .Where(er => er.EvidenceId == evidenceId && er.IsActive && !er.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EvidenceRelation>> GetByRelationIdAsync(Guid relationId, CancellationToken cancellationToken = default)
    {
        return await _context.EvidenceRelations
            .Include(er => er.Evidence)
            .Where(er => er.PersonPropertyRelationId == relationId && !er.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EvidenceRelation>> GetActiveByRelationIdAsync(Guid relationId, CancellationToken cancellationToken = default)
    {
        return await _context.EvidenceRelations
            .Include(er => er.Evidence)
            .Where(er => er.PersonPropertyRelationId == relationId && er.IsActive && !er.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<EvidenceRelation?> GetByEvidenceAndRelationAsync(Guid evidenceId, Guid relationId, CancellationToken cancellationToken = default)
    {
        return await _context.EvidenceRelations
            .FirstOrDefaultAsync(er =>
                er.EvidenceId == evidenceId
                && er.PersonPropertyRelationId == relationId
                && !er.IsDeleted, cancellationToken);
    }

    public async Task<bool> LinkExistsAsync(Guid evidenceId, Guid relationId, CancellationToken cancellationToken = default)
    {
        return await _context.EvidenceRelations
            .AnyAsync(er =>
                er.EvidenceId == evidenceId
                && er.PersonPropertyRelationId == relationId
                && er.IsActive
                && !er.IsDeleted, cancellationToken);
    }

    public async Task<EvidenceRelation> AddAsync(EvidenceRelation evidenceRelation, CancellationToken cancellationToken = default)
    {
        await _context.EvidenceRelations.AddAsync(evidenceRelation, cancellationToken);
        return evidenceRelation;
    }

    public Task UpdateAsync(EvidenceRelation evidenceRelation, CancellationToken cancellationToken = default)
    {
        _context.EvidenceRelations.Update(evidenceRelation);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
