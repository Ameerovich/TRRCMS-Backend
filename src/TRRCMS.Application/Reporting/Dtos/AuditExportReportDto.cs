namespace TRRCMS.Application.Reporting.Dtos;

public sealed class AuditExportReportDto
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public string? EntityTypeFilter { get; set; }
    public bool SecurityOnly { get; set; }
    public DateTime GeneratedAtUtc { get; set; }

    public List<AuditExportRow> Entries { get; set; } = new();
}

public sealed class AuditExportRow
{
    public long AuditLogNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ActionDescription { get; set; } = string.Empty;
    public string ActionResult { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityIdentifier { get; set; }
    public string? IpAddress { get; set; }
    public bool IsSecuritySensitive { get; set; }
}
