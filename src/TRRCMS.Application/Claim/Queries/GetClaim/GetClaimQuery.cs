using MediatR;
using TRRCMS.Application.Claims.Dtos;

namespace TRRCMS.Application.Claims.Queries.GetClaim;

/// <summary>
/// Query to get a single claim by ID
/// </summary>
public class GetClaimQuery : IRequest<ClaimDto?>
{
    /// <summary>
    /// Claim ID to retrieve
    /// </summary>
    public Guid Id { get; set; }
    
    public GetClaimQuery(Guid id)
    {
        Id = id;
    }
}
