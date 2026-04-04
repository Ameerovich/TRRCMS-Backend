namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Generates unique survey reference codes using timestamp + source identifier.
/// Format: SRV-{Source}-{YYYYMMDDHHmmss}
/// Examples: SRV-OFC-20260404143527 (office), SRV-T01-20260404091205 (tablet T01)
/// Unique per device per second — no database sequence needed.
/// Mobile devices generate codes offline using their DeviceId as source.
/// </summary>
public interface ISurveyReferenceCodeGenerator
{
    /// <summary>
    /// Generate a survey reference code.
    /// </summary>
    /// <param name="source">Source identifier: "OFC" for office surveys, device ID for mobile (e.g., "T01").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Formatted reference code (e.g., "SRV-OFC-20260404143527").</returns>
    Task<string> GenerateNextAsync(string source, CancellationToken cancellationToken = default);
}
