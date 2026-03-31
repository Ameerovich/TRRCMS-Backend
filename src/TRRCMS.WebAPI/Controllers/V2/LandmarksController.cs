using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Landmarks.Dtos;
using TRRCMS.Application.Landmarks.Queries.GetLandmarksForMap;
using TRRCMS.Application.Landmarks.Queries.SearchLandmarks;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Landmarks v2 — list endpoints wrapped in ListResponse.
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class LandmarksController : ControllerBase
{
    private readonly IMediator _mediator;

    public LandmarksController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get landmarks within a bounding box for map rendering.
    /// </summary>
    [HttpGet("map")]
    [ProducesResponseType(typeof(ListResponse<LandmarkMapDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListResponse<LandmarkMapDto>>> GetLandmarksForMap(
        [FromQuery] decimal northEastLat,
        [FromQuery] decimal northEastLng,
        [FromQuery] decimal southWestLat,
        [FromQuery] decimal southWestLng,
        [FromQuery] int? type,
        CancellationToken cancellationToken)
    {
        var query = new GetLandmarksForMapQuery
        {
            NorthEastLat = northEastLat,
            NorthEastLng = northEastLng,
            SouthWestLat = southWestLat,
            SouthWestLng = southWestLng,
            Type = type
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ListResponse<LandmarkMapDto>.From(result));
    }

    /// <summary>
    /// Search landmarks by name with optional type filter.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ListResponse<LandmarkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListResponse<LandmarkDto>>> SearchLandmarks(
        [FromQuery] string query,
        [FromQuery] int? type,
        [FromQuery] int maxResults = 50,
        CancellationToken cancellationToken = default)
    {
        var searchQuery = new SearchLandmarksQuery
        {
            Query = query,
            Type = type,
            MaxResults = maxResults
        };

        var result = await _mediator.Send(searchQuery, cancellationToken);
        return Ok(ListResponse<LandmarkDto>.From(result));
    }

}
