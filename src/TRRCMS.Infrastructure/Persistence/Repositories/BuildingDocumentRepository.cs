using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for BuildingDocument entities.
/// Provides basic CRUD and deduplication support.
/// </summary>
public class BuildingDocumentRepository : IBuildingDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public BuildingDocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BuildingDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BuildingDocuments
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);
    }

    public async Task<BuildingDocument?> GetByFileHashAsync(string fileHash, CancellationToken cancellationToken = default)
    {
        return await _context.BuildingDocuments
            .Where(d => d.FileHash == fileHash && !d.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(BuildingDocument document, CancellationToken cancellationToken = default)
    {
        await _context.BuildingDocuments.AddAsync(document, cancellationToken);
    }

    public Task UpdateAsync(BuildingDocument document, CancellationToken cancellationToken = default)
    {
        _context.BuildingDocuments.Update(document);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
