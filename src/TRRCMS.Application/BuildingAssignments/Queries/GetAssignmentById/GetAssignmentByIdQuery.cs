using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetAssignmentById;

/// <summary>
/// Query to get a specific building assignment by ID
/// </summary>
public record GetAssignmentByIdQuery : IRequest<BuildingAssignmentDto?>
{
    public Guid AssignmentId { get; init; }
}
