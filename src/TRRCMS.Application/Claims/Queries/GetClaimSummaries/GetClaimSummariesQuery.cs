using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Claims.Queries.GetClaimSummaries;

/// <summary>
/// Query to retrieve claim summaries with optional filtering.
/// Returns CreatedClaimSummaryDto list suitable for the claims overview UI.
/// All enum filters use integer codes matching the Vocabulary API.
/// </summary>
public class GetClaimSummariesQuery : IRequest<List<CreatedClaimSummaryDto>>
{
    /// <summary>
    /// Filter by case status (integer code).
    /// Maps to CaseStatus enum: Open=1, Closed=2.
    /// </summary>
    public int? CaseStatus { get; set; }

    /// <summary>
    /// Filter by claim source (integer code).
    /// Maps to ClaimSource enum: FieldCollection=1, OfficeSubmission=2, etc.
    /// </summary>
    public int? ClaimSource { get; set; }

    /// <summary>
    /// Filter by the user who created the claim (CreatedBy audit field).
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Filter by originating survey ID.
    /// Returns only claims created during that specific survey finalization.
    /// Uses Claim.OriginatingSurveyId for direct lookup.
    /// </summary>
    public Guid? SurveyVisitId { get; set; }

    /// <summary>
    /// Filter by property unit ID.
    /// Returns all claims for the specified property unit.
    /// </summary>
    public Guid? PropertyUnitId { get; set; }

    /// <summary>
    /// Filter by building code (17-digit GGDDSSCCNCNNBBBBB).
    /// Exact match against the related Building.BuildingId.
    /// </summary>
    public string? BuildingCode { get; set; }
}
