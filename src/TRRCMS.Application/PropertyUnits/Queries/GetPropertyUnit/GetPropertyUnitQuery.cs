using MediatR;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnit;

/// <summary>
/// Query to get a property unit by ID
/// </summary>
public class GetPropertyUnitQuery : IRequest<PropertyUnitDto?>
{
    public Guid Id { get; set; }

    public GetPropertyUnitQuery(Guid id)
    {
        Id = id;
    }
}