namespace TRRCMS.WebAPI.Middleware;

/// <summary>
/// Standardized error response body returned by the API for all error scenarios.
/// </summary>
public class ErrorResponse
{
    public int Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// Original English exception message preserved for developer debugging.
    /// Populated only when Accept-Language requests a non-English locale.
    /// Omitted from JSON when null (no change for English consumers).
    /// </summary>
    public string? Detail { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
    /// <summary>
    /// Optional payload for conflict responses (e.g., the existing entity that caused the conflict).
    /// </summary>
    public object? ConflictData { get; set; }
}
