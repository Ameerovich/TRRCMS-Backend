using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Infrastructure.Persistence;
using TRRCMS.Infrastructure.Persistence.SeedData;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Infrastructure-tier implementation of <see cref="IBuildingsImportService"/>.
/// Delegates to <see cref="BuildingsImporter"/> so the migration path (raw SQL via
/// MigrationBuilder) and the admin-endpoint path (EF UPSERT) share the same loader.
/// </summary>
public class BuildingsImportService : IBuildingsImportService
{
    private readonly ApplicationDbContext _context;

    public BuildingsImportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BuildingsImportSummary> ApplyFromJsonAsync(
        string json,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var dataset = BuildingsImporter.LoadFromJson(json);
        var summary = await BuildingsImporter.ApplyAsync(_context, dataset, userId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new BuildingsImportSummary
        {
            Inserted = summary.Inserted,
            Updated = summary.Updated,
            Unchanged = summary.Unchanged,
            Skipped = summary.Skipped,
        };
    }
}
