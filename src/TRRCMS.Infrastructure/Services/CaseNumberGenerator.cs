using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Infrastructure.Persistence;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Generates sequential case numbers using PostgreSQL sequence
/// Format: CASE-YYYY-NNNNN
/// </summary>
public class CaseNumberGenerator : ICaseNumberGenerator
{
    private readonly ApplicationDbContext _context;

    public CaseNumberGenerator(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<string> GenerateNextCaseNumberAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();
        var wasAlreadyOpen = connection.State == System.Data.ConnectionState.Open;

        if (!wasAlreadyOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT nextval('\"CaseNumberSequence\"')";

            var currentTransaction = _context.Database.CurrentTransaction;
            if (currentTransaction != null)
                command.Transaction = currentTransaction.GetDbTransaction();

            var result = await command.ExecuteScalarAsync(cancellationToken);
            var sequenceValue = Convert.ToInt64(result);

            return $"CASE-{DateTime.UtcNow.Year}-{sequenceValue:D5}";
        }
        finally
        {
            if (!wasAlreadyOpen)
                await connection.CloseAsync();
        }
    }
}
