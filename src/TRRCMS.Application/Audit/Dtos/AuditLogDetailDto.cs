namespace TRRCMS.Application.Audit.Dtos;

public class AuditLogDetailDto
{
    public Guid Id { get; set; }
    public long AuditLogNumber { get; set; }
    public DateTime Timestamp { get; set; }

    public int ActionType { get; set; }
    public string ActionTypeName { get; set; } = string.Empty;
    public string ActionDescription { get; set; } = string.Empty;
    public string ActionResult { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;

    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? EntityIdentifier { get; set; }

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    /// <summary>
    /// Comma-separated list of changed field names (legacy hint).
    /// Semantics:
    ///   <c>null</c>  → not tracked for this entry (e.g. Create/Delete or older logs).
    ///   <c>""</c>    → tracked but no scalar field changes detected.
    ///   populated → authoritative list of changed field names.
    /// Prefer <see cref="Changes"/> for rendering; this remains for backward-compat.
    /// </summary>
    public string? ChangedFields { get; set; }

    /// <summary>
    /// Structured per-field diff parsed from <see cref="OldValues"/> /
    /// <see cref="NewValues"/>. Empty when the entry has no JSON payload
    /// (e.g. Login, View, Delete).
    /// </summary>
    public List<AuditChangeDto> Changes { get; set; } = new();

    public string? IpAddress { get; set; }
    public string? SourceApplication { get; set; }

    public string? ErrorMessage { get; set; }

    public bool IsSecuritySensitive { get; set; }
    public bool RequiresLegalRetention { get; set; }

    public Guid? CorrelationId { get; set; }
}
