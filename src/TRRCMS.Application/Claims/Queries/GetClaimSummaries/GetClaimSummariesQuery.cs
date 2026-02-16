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
    /// Filter by claim status (integer code).
    /// Maps to ClaimStatus enum: Draft=1, Finalized=2, UnderReview=3, etc.
    /// </summary>
    public int? ClaimStatus { get; set; }

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
    /// Filter by survey visit ID.
    /// Joins via Survey.ClaimId to find the claim linked to this survey.
    /// </summary>
    public Guid? SurveyVisitId { get; set; }

    /// <summary>
    /// Filter by building code (17-digit GGDDSSCCNCNNBBBBB).
    /// Exact match against the related Building.BuildingId.
    /// </summary>
    public string? BuildingCode { get; set; }
}
