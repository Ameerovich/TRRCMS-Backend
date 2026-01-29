using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Households.Queries.GetHousehold;

/// <summary>
/// Query to get a single household by ID
/// </summary>
public record GetHouseholdQuery(Guid Id) : IRequest<HouseholdDto?>;
