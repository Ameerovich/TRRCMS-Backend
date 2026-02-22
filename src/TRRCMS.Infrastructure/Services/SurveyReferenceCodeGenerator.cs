using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Infrastructure.Persistence;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Generates sequential survey reference codes using a PostgreSQL sequence.
/// Format: {Prefix}-YYYY-NNNNN  (e.g., ALG-2026-00001)
/// Thread-safe, collision-free, survives app restarts.
/// </summary>
public class SurveyReferenceCodeGenerator : ISurveyReferenceCodeGenerator
{
    private readonly ApplicationDbContext _context;

    public SurveyReferenceCodeGenerator(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string> GenerateNextAsync(
        string prefix, CancellationToken cancellationToken = default)
    {
        var sequenceValue = await GetNextSequenceValueAsync(cancellationToken);
        var year = DateTime.UtcNow.Year;
        return $"{prefix}-{year}-{sequenceValue:D5}";
    }

    private async Task<long> GetNextSequenceValueAsync(CancellationToken cancellationToken)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT nextval('\"SurveyReferenceSequence\"')";

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt64(result);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
