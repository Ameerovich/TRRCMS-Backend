using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Streets.Commands.DeleteStreet;
using TRRCMS.Application.Streets.Commands.RegisterStreet;
using TRRCMS.Application.Streets.Commands.UpdateStreet;
using TRRCMS.Application.Streets.Dtos;
using TRRCMS.Application.Streets.Queries.GetStreetById;
using TRRCMS.Application.Streets.Queries.GetStreetsForMap;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Streets — line reference features on the map.
/// شوارع — خطوط مرجعية على الخريطة
/// Managed via QGIS plugin through the API.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class StreetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StreetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ==================== QUERIES ====================

    /// <summary>
    /// Get streets intersecting a bounding box for map rendering (line layer).
    /// Returns streets intersecting the specified bounding box.
    /// </summary>
    /// <param name="northEastLat">North-east corner latitude</param>
    /// <param name="northEastLng">North-east corner longitude</param>
    /// <param name="southWestLat">South-west corner latitude</param>
    /// <param name="southWestLng">South-west corner longitude</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("map")]
    [ProducesResponseType(typeof(List<StreetMapDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StreetMapDto>>> GetStreetsForMap(
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
        return Ok(result);
    }

    /// <summary>
    /// Get a street by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StreetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StreetDto>> GetStreetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStreetByIdQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    // ==================== COMMANDS (QGIS Plugin) ====================

    /// <summary>
    /// Register a new street (from QGIS plugin).
    /// Creates a new street with linestring geometry.
    /// تسجيل شارع جديد
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(StreetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StreetDto>> RegisterStreet(
        [FromBody] RegisterStreetCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetStreetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing street (name, and optionally geometry).
    /// تعديل شارع موجود
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(StreetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StreetDto>> UpdateStreet(
        Guid id,
        [FromBody] UpdateStreetCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route ID does not match body ID.");

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a street (soft delete).
    /// حذف شارع
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStreet(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteStreetCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}
