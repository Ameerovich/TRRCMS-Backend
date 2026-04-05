using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Cases.Dtos;

/// <summary>
/// Full Case DTO with related entity summaries
/// </summary>
public class CaseDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid PropertyUnitId { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime OpenedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public Guid? ClosedByClaimId { get; set; }
    public bool IsEditable { get; set; }
    public string? Notes { get; set; }

    // Aggregated counts
    public int SurveyCount { get; set; }
    public int ClaimCount { get; set; }
    public int PersonPropertyRelationCount { get; set; }

    // Related entity IDs
    public List<Guid> SurveyIds { get; set; } = new();
    public List<Guid> ClaimIds { get; set; } = new();

    // Audit
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}

/// <summary>
/// Lightweight Case DTO for lists
/// </summary>
public class CaseSummaryDto
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid PropertyUnitId { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime OpenedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public bool IsEditable { get; set; }
    public int SurveyCount { get; set; }
    public int ClaimCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
