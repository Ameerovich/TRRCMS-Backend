using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnitsByBuilding;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Property Units API v2 — list endpoints with ListResponse wrapper.
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class PropertyUnitsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PropertyUnitsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get all property units for a specific building.
    /// </summary>
    [HttpGet("building/{buildingId}")]
    [Authorize(Policy = "CanViewPropertyUnits")]
    [ProducesResponseType(typeof(ListResponse<PropertyUnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListResponse<PropertyUnitDto>>> GetPropertyUnitsByBuilding(Guid buildingId)
    {
        try
        {
            var query = new GetPropertyUnitsByBuildingQuery(buildingId);
            var result = await _mediator.Send(query);
            return Ok(ListResponse<PropertyUnitDto>.From(result));
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
