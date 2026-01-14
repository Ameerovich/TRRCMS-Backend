namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service for generating sequential claim numbers
/// Format: CLM-YYYY-NNNNNNNNN (e.g., CLM-2026-000000001)
/// </summary>
public interface IClaimNumberGenerator
{
    /// <summary>
    /// Generate next sequential claim number
    /// Thread-safe, guaranteed unique
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Formatted claim number (e.g., "CLM-2026-000000001")</returns>
    Task<string> GenerateNextClaimNumberAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current sequence value without incrementing
    /// Useful for reporting/statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current sequence number</returns>
    Task<long> GetCurrentSequenceValueAsync(CancellationToken cancellationToken = default);
}
