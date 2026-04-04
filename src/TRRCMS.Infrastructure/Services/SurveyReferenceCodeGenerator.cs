using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Generates unique survey reference codes using timestamp + source identifier.
/// Format: SRV-{Source}-{YYYYMMDDHHmmss}
/// No database dependency — pure timestamp-based, works offline and online.
/// </summary>
public class SurveyReferenceCodeGenerator : ISurveyReferenceCodeGenerator
{
    public Task<string> GenerateNextAsync(
        string source, CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return Task.FromResult($"SRV-{source}-{timestamp}");
    }
}
