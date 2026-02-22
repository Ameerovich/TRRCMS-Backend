using MediatR;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Households.Queries.GetAllHouseholds;

/// <summary>
/// Query to get all households with pagination
/// </summary>
public class GetAllHouseholdsQuery : PagedQuery, IRequest<PagedResult<HouseholdDto>> { }
