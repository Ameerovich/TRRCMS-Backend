using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Households.Queries.GetHousehold;

/// <summary>
/// Query to get a household by ID
/// </summary>
public class GetHouseholdQuery : IRequest<HouseholdDto?>
{
    public Guid Id { get; }

    public GetHouseholdQuery(Guid id)
    {
        Id = id;
    }
}
