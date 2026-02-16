namespace TRRCMS.WebAPI.Middleware;

/// <summary>
/// Standardized error response body returned by the API for all error scenarios.
/// </summary>
public class ErrorResponse
{
    public int Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; set; }
}
