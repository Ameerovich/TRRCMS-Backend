namespace TRRCMS.Application.Common.Models;

/// <summary>
/// Result of a delete operation, including all cascaded soft deletes
/// </summary>
public class DeleteResultDto
{
    public Guid PrimaryEntityId { get; set; }
    public string PrimaryEntityType { get; set; } = string.Empty;
    public List<DeletedEntityInfo> AffectedEntities { get; set; } = new();
    public int TotalAffected { get; set; }
    public DateTime DeletedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class DeletedEntityInfo
{
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string? EntityIdentifier { get; set; }
}
