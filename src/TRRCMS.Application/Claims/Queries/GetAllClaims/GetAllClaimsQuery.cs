using MediatR;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Claims.Queries.GetAllClaims;

/// <summary>
/// Query to get all claims with optional filtering and pagination
/// </summary>
public class GetAllClaimsQuery : PagedQuery, IRequest<PagedResult<ClaimDto>>
{
    /// <summary>
    /// Filter by lifecycle stage (optional)
    /// </summary>
    public LifecycleStage? LifecycleStage { get; set; }

    /// <summary>
    /// Filter by status (optional)
    /// </summary>
    public ClaimStatus? Status { get; set; }

    /// <summary>
    /// Filter by priority (optional)
    /// </summary>
    public CasePriority? Priority { get; set; }

    /// <summary>
    /// Filter by assigned user (optional)
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// Filter by primary claimant (optional)
    /// </summary>
    public Guid? PrimaryClaimantId { get; set; }

    /// <summary>
    /// Filter by property unit (optional)
    /// </summary>
    public Guid? PropertyUnitId { get; set; }

    /// <summary>
    /// Filter by verification status (optional)
    /// </summary>
    public VerificationStatus? VerificationStatus { get; set; }

    /// <summary>
    /// Show only conflicting claims (optional)
    /// </summary>
    public bool? HasConflicts { get; set; }

    /// <summary>
    /// Show only overdue claims (optional)
    /// </summary>
    public bool? IsOverdue { get; set; }

    /// <summary>
    /// Show only claims awaiting documents (optional)
    /// </summary>
    public bool? AwaitingDocuments { get; set; }
}
