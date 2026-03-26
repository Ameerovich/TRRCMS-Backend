using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Buildings.Queries.GetBuildingsForMap;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Buildings v2 — list endpoints wrapped in ListResponse.
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class BuildingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BuildingsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get buildings for map display.
    /// </summary>
    [HttpPost("map")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(ListResponse<BuildingMapDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ListResponse<BuildingMapDto>>> GetBuildingsForMap(
        [FromBody] GetBuildingsForMapQuery query)
    {
        var buildings = await _mediator.Send(query);
        return Ok(ListResponse<BuildingMapDto>.From(buildings));
    }
}
