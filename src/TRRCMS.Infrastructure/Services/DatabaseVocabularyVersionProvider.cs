using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Infrastructure.Persistence;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Database-backed vocabulary version provider.
/// Replaces hardcoded ServerVocabularyVersions from appsettings.json.
/// Used by ImportService for vocabulary compatibility checking.
/// </summary>
public class DatabaseVocabularyVersionProvider : IVocabularyVersionProvider
{
    private readonly ApplicationDbContext _context;

    public DatabaseVocabularyVersionProvider(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string?> GetCurrentVersionAsync(string vocabularyName, CancellationToken cancellationToken = default)
    {
        return await _context.Vocabularies
            .Where(v => !v.IsDeleted && v.IsCurrentVersion && v.VocabularyName == vocabularyName)
            .Select(v => v.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<string, string>> GetAllCurrentVersionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Vocabularies
            .Where(v => !v.IsDeleted && v.IsCurrentVersion && v.IsActive)
            .ToDictionaryAsync(v => v.VocabularyName, v => v.Version, cancellationToken);
    }
}
