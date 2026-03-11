using MediatR;
using TRRCMS.Application.Claims.Dtos;

namespace TRRCMS.Application.Claims.Queries.GetClaimByNumber;

/// <summary>
/// Query to get a single claim by its claim number (e.g. CLM-2026-000000015).
/// </summary>
public record GetClaimByNumberQuery(string ClaimNumber) : IRequest<ClaimDto?>;
