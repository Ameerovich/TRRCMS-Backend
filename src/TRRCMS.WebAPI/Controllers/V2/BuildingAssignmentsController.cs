using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.BuildingAssignments.Queries.GetAvailableFieldCollectors;
using TRRCMS.Application.BuildingAssignments.Queries.GetPropertyUnitsForRevisit;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Building assignments v2 — list endpoints wrapped in ListResponse.
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class BuildingAssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BuildingAssignmentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get property units for revisit selection.
    /// </summary>
    [HttpGet("buildings/{buildingId:guid}/property-units")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(ListResponse<PropertyUnitForRevisitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListResponse<PropertyUnitForRevisitDto>>> GetPropertyUnitsForRevisit(
        Guid buildingId,
        [FromQuery] bool onlyWithCompletedSurveys = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPropertyUnitsForRevisitQuery
        {
            BuildingId = buildingId,
            OnlyWithCompletedSurveys = onlyWithCompletedSurveys
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ListResponse<PropertyUnitForRevisitDto>.From(result));
    }

    /// <summary>
    /// Get available field collectors for assignment.
    /// </summary>
    [HttpGet("field-collectors")]
    [Authorize(Policy = "CanAssignBuildings")]
    [ProducesResponseType(typeof(ListResponse<AvailableFieldCollectorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ListResponse<AvailableFieldCollectorDto>>> GetAvailableFieldCollectors(
        [FromQuery] bool? isAvailable,
        [FromQuery] string? teamName,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? hasAssignedTablet,
        [FromQuery] bool sortByWorkloadAscending = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableFieldCollectorsQuery
        {
            IsAvailable = isAvailable,
            TeamName = teamName,
            SearchTerm = searchTerm,
            HasAssignedTablet = hasAssignedTablet,
            SortByWorkloadAscending = sortByWorkloadAscending
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ListResponse<AvailableFieldCollectorDto>.From(result));
    }
}
