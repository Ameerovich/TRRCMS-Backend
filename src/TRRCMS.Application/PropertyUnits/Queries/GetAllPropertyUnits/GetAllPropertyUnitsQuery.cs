using MediatR;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Queries.GetAllPropertyUnits;

/// <summary>
/// Query to get all property units
/// </summary>
public class GetAllPropertyUnitsQuery : IRequest<List<PropertyUnitDto>>
{
}