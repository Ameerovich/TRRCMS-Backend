namespace TRRCMS.Application.Evidences.Dtos;

/// <summary>
/// DTO for EvidenceRelation join entity (Evidence ↔ PersonPropertyRelation many-to-many)
/// </summary>
public class EvidenceRelationDto
{
    public Guid Id { get; set; }
    public Guid EvidenceId { get; set; }
    public Guid PersonPropertyRelationId { get; set; }
    public string? LinkReason { get; set; }
    public DateTime LinkedAtUtc { get; set; }
    public Guid LinkedBy { get; set; }
    public bool IsActive { get; set; }

    // Audit fields
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
}
