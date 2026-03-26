using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Streets.Dtos;
using TRRCMS.Application.Streets.Queries.GetStreetsForMap;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Streets API v2 — list endpoints with ListResponse wrapper.
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
public class StreetsController : ControllerBase
{
    private readonly IMediator _mediator;
    public StreetsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get streets intersecting a bounding box for map rendering.
    /// </summary>
    [HttpGet("map")]
    [ProducesResponseType(typeof(ListResponse<StreetMapDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListResponse<StreetMapDto>>> GetStreetsForMap(
        [FromQuery] decimal northEastLat,
        [FromQuery] decimal northEastLng,
        [FromQuery] decimal southWestLat,
        [FromQuery] decimal southWestLng,
        CancellationToken cancellationToken)
    {
        var query = new GetStreetsForMapQuery
        {
            NorthEastLat = northEastLat,
            NorthEastLng = northEastLng,
            SouthWestLat = southWestLat,
            SouthWestLng = southWestLng
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ListResponse<StreetMapDto>.From(result));
    }
}
