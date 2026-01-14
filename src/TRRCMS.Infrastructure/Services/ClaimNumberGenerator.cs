using Microsoft.EntityFrameworkCore;
using Npgsql;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Infrastructure.Persistence;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Generates sequential claim numbers using PostgreSQL sequence
/// Format: CLM-YYYY-NNNNNNNNN
/// Thread-safe and collision-free
/// </summary>
public class ClaimNumberGenerator : IClaimNumberGenerator
{
    private readonly ApplicationDbContext _context;

    public ClaimNumberGenerator(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Generate next sequential claim number
    /// Uses PostgreSQL sequence for thread-safety
    /// </summary>
    public async Task<string> GenerateNextClaimNumberAsync(CancellationToken cancellationToken = default)
    {
        // Get next value from PostgreSQL sequence
        var sequenceValue = await GetNextSequenceValueAsync(cancellationToken);

        // Get current year
        var year = DateTime.UtcNow.Year;

        // Format: CLM-YYYY-NNNNNNNNN (9 digits, zero-padded)
        var claimNumber = $"CLM-{year}-{sequenceValue:D9}";

        return claimNumber;
    }

    /// <summary>
    /// Get current sequence value without incrementing
    /// </summary>
    public async Task<long> GetCurrentSequenceValueAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT last_value FROM \"ClaimNumberSequence\"";

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt64(result);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    /// <summary>
    /// Get next value from sequence (increments automatically)
    /// </summary>
    private async Task<long> GetNextSequenceValueAsync(CancellationToken cancellationToken)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT nextval('\"ClaimNumberSequence\"')";

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt64(result);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}