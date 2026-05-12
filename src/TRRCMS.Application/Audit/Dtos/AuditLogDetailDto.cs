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
    public string? ChangedFields { get; set; }

    public string? IpAddress { get; set; }
    public string? SourceApplication { get; set; }

    public string? ErrorMessage { get; set; }

    public bool IsSecuritySensitive { get; set; }
    public bool RequiresLegalRetention { get; set; }

    public Guid? CorrelationId { get; set; }
}
