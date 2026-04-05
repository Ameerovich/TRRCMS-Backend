namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service for generating sequential case numbers
/// Format: CASE-YYYY-NNNNN (e.g., CASE-2026-00001)
/// </summary>
public interface ICaseNumberGenerator
{
    Task<string> GenerateNextCaseNumberAsync(CancellationToken cancellationToken = default);
}
