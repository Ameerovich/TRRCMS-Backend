using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

public class IdentificationDocumentRepository : IIdentificationDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public IdentificationDocumentRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IdentificationDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.IdentificationDocuments
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
    }

    public async Task<List<IdentificationDocument>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        return await _context.IdentificationDocuments
            .Where(d => d.PersonId == personId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(IdentificationDocument entity, CancellationToken cancellationToken = default)
    {
        await _context.IdentificationDocuments.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(IdentificationDocument entity, CancellationToken cancellationToken = default)
    {
        _context.IdentificationDocuments.Update(entity);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
