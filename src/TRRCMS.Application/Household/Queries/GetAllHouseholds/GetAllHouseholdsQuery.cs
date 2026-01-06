using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Households.Queries.GetAllHouseholds;

/// <summary>
/// Query to get all households
/// </summary>
public class GetAllHouseholdsQuery : IRequest<IEnumerable<HouseholdDto>>
{
}
