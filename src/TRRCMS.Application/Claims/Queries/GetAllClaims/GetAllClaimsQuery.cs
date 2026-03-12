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
    /// Filter by case status (optional): Open=1, Closed=2
    /// </summary>
    public CaseStatus? CaseStatus { get; set; }

    /// <summary>
    /// Filter by primary claimant (optional)
    /// </summary>
    public Guid? PrimaryClaimantId { get; set; }

    /// <summary>
    /// Filter by property unit (optional)
    /// </summary>
    public Guid? PropertyUnitId { get; set; }
}
