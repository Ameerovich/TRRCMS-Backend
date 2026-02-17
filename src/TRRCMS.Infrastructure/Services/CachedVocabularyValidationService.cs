using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Infrastructure.Persistence;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Singleton service that caches vocabulary codes in-memory for fast validation.
/// Uses IServiceScopeFactory to create short-lived scopes for DB access (singletons can't hold scoped deps).
/// Cache is invalidated when vocabularies are created/updated/activated/deactivated via the management API.
/// </summary>
public class CachedVocabularyValidationService : IVocabularyValidationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CachedVocabularyValidationService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private volatile Dictionary<string, HashSet<int>> _cache = new();
    private volatile bool _loaded;

    private static readonly IReadOnlySet<int> EmptySet = new HashSet<int>();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CachedVocabularyValidationService(
        IServiceScopeFactory scopeFactory,
        ILogger<CachedVocabularyValidationService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsValidCode(string vocabularyName, int code)
    {
        EnsureLoaded();
        return _cache.TryGetValue(vocabularyName, out var codes) && codes.Contains(code);
    }

    public IReadOnlySet<int> GetValidCodes(string vocabularyName)
    {
        EnsureLoaded();
        return _cache.TryGetValue(vocabularyName, out var codes) ? codes : EmptySet;
    }

    public void InvalidateCache()
    {
        _loaded = false;
        _logger.LogInformation("Vocabulary validation cache invalidated");
    }

    public async Task WarmupAsync(CancellationToken cancellationToken = default)
    {
        await LoadCacheAsync(cancellationToken);
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;

        // Blocking async-over-sync is acceptable here because:
        // 1. This only happens on first access or after invalidation
        // 2. The cache warmup is fast (single DB query)
        // 3. The SemaphoreSlim ensures only one thread does the load
        _lock.Wait();
        try
        {
            if (_loaded) return;
            LoadCacheAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task LoadCacheAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var vocabularies = await context.Vocabularies
                .Where(v => !v.IsDeleted && v.IsCurrentVersion && v.IsActive)
                .Select(v => new { v.VocabularyName, v.ValuesJson })
                .ToListAsync(cancellationToken);

            var newCache = new Dictionary<string, HashSet<int>>(vocabularies.Count);

            foreach (var vocab in vocabularies)
            {
                var codes = ParseCodes(vocab.ValuesJson);
                newCache[vocab.VocabularyName] = codes;
            }

            _cache = newCache;
            _loaded = true;

            _logger.LogInformation(
                "Vocabulary validation cache loaded: {VocabularyCount} vocabularies, {TotalCodes} total codes",
                newCache.Count,
                newCache.Values.Sum(s => s.Count));
        }
        finally
        {
            _lock.Release();
        }
    }

    private static HashSet<int> ParseCodes(string valuesJson)
    {
        if (string.IsNullOrWhiteSpace(valuesJson) || valuesJson == "[]")
            return new HashSet<int>();

        try
        {
            var values = JsonSerializer.Deserialize<List<VocabularyCodeEntry>>(valuesJson, JsonOptions);
            if (values is null) return new HashSet<int>();

            return values.Select(v => v.Code).ToHashSet();
        }
        catch
        {
            return new HashSet<int>();
        }
    }

    private class VocabularyCodeEntry
    {
        public int Code { get; set; }
    }
}
