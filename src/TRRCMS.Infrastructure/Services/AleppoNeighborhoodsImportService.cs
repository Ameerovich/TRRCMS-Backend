using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Infrastructure.Persistence;
using TRRCMS.Infrastructure.Persistence.SeedData;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Infrastructure-tier implementation of <see cref="IAleppoNeighborhoodsImportService"/>.
/// Delegates the heavy lifting to <see cref="AleppoNeighborhoodsImporter"/> so the migration
/// path (raw SQL via MigrationBuilder) and the admin-endpoint path (EF UPSERT) share the
/// same loader, and the SQL fallback (BuildSeedSqlStatements) remains the single source of truth.
/// </summary>
public class AleppoNeighborhoodsImportService : IAleppoNeighborhoodsImportService
{
    private readonly ApplicationDbContext _context;

    public AleppoNeighborhoodsImportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NeighborhoodImportSummary> ApplyFromJsonAsync(
        string json,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var dataset = AleppoNeighborhoodsImporter.LoadFromJson(json);
        var summary = await AleppoNeighborhoodsImporter.ApplyAsync(_context, dataset, userId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new NeighborhoodImportSummary
        {
            Inserted = summary.Inserted,
            Updated = summary.Updated,
            Restored = summary.Restored,
            SoftDeletedPlaceholders = summary.SoftDeletedPlaceholders,
        };
    }
}
