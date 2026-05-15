namespace TRRCMS.Application.Audit.Dtos;

/// <summary>
/// Structured representation of a single field change inside an audit entry.
/// Derived from <c>OldValues</c>/<c>NewValues</c> JSON snapshots and (when available)
/// the <c>ChangedFields</c> hint. Display labels (e.g. Arabic field names) are the
/// frontend's responsibility — backend only ships raw field identifiers and values.
/// </summary>
public class AuditChangeDto
{
    /// <summary>Backend field identifier (camelCase).</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>Previous value as a JSON token (string, number, bool, object, array, or null).</summary>
    public object? OldValue { get; set; }

    /// <summary>New value as a JSON token.</summary>
    public object? NewValue { get; set; }

    /// <summary>
    /// Coarse type hint inferred from the JSON token: "string", "number", "boolean",
    /// "date", "object", "array", or "null". Lets the frontend pick a renderer
    /// without re-sniffing the value.
    /// </summary>
    public string Type { get; set; } = "string";
}
