using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Vocabulary repository implementation using Entity Framework Core.
/// </summary>
public class VocabularyRepository : IVocabularyRepository
{
    private readonly ApplicationDbContext _context;

    public VocabularyRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Vocabulary?> GetByNameAsync(string vocabularyName, bool currentOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Vocabularies
            .Where(v => !v.IsDeleted && v.VocabularyName == vocabularyName);

        if (currentOnly)
            query = query.Where(v => v.IsCurrentVersion);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Vocabulary>> GetAllCurrentAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Vocabularies
            .Where(v => !v.IsDeleted && v.IsCurrentVersion && v.IsActive)
            .OrderBy(v => v.Category)
            .ThenBy(v => v.VocabularyName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Vocabulary>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.Vocabularies
            .Where(v => !v.IsDeleted && v.IsCurrentVersion && v.IsActive && v.Category == category)
            .OrderBy(v => v.VocabularyName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Vocabulary>> GetVersionHistoryAsync(string vocabularyName, CancellationToken cancellationToken = default)
    {
        return await _context.Vocabularies
            .Where(v => !v.IsDeleted && v.VocabularyName == vocabularyName)
            .OrderByDescending(v => v.VersionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string vocabularyName, CancellationToken cancellationToken = default)
    {
        return await _context.Vocabularies
            .AnyAsync(v => !v.IsDeleted && v.VocabularyName == vocabularyName && v.IsCurrentVersion, cancellationToken);
    }

    public async Task<Vocabulary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vocabularies
            .Where(v => !v.IsDeleted && v.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Vocabulary> AddAsync(Vocabulary vocabulary, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Vocabularies.AddAsync(vocabulary, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(Vocabulary vocabulary, CancellationToken cancellationToken = default)
    {
        _context.Vocabularies.Update(vocabulary);
        return Task.CompletedTask;
    }
}
