namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Generates sequential survey reference codes using a PostgreSQL sequence.
/// Format: {Prefix}-YYYY-NNNNN  (e.g., ALG-2026-00001, OFC-2026-00002)
/// Thread-safe and collision-free — shared sequence across all survey types.
/// </summary>
public interface ISurveyReferenceCodeGenerator
{
    /// <summary>
    /// Generate the next sequential survey reference code.
    /// </summary>
    /// <param name="prefix">ALG for field surveys, OFC for office surveys.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Formatted reference code (e.g., "ALG-2026-00001").</returns>
    Task<string> GenerateNextAsync(string prefix, CancellationToken cancellationToken = default);
}
