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
    /// Bulk register streets (from QGIS plugin or desktop)
    /// تسجيل مجموعة شوارع دفعة واحدة
    /// </summary>
    /// <remarks>
    /// Accepts an array of streets and processes them in a single transaction.
    ///
    /// **Behavior:**
    /// - Duplicates by Identifier are **skipped** (idempotent — safe to re-send)
    /// - Invalid WKT geometry items are **failed** with error details
    /// - Valid items are saved in a single database transaction
    /// - Partial success is possible: some items succeed while others skip/fail
    ///
    /// **Geometry format:** WKT LINESTRING in longitude-latitude order, SRID 4326 (WGS84)
    ///
    /// **Example request:**
    /// ```json
    /// {
    ///   "streets": [
    ///     { "identifier": 1, "name": "شارع النصر", "geometryWkt": "LINESTRING(37.1340 36.2018, 37.1350 36.2025)" },
    ///     { "identifier": 2, "name": "شارع حلب", "geometryWkt": "LINESTRING(37.1360 36.2030, 37.1370 36.2035, 37.1380 36.2040)" }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Array of streets to register</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bulk operation result with succeeded, failed, skipped counts and created items</returns>
    /// <response code="200">Operation completed. Check succeeded/failed/skipped counts in response.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("bulk-register")]
    [ProducesResponseType(typeof(Application.Common.Models.BulkOperationResult<StreetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Application.Common.Models.BulkOperationResult<StreetDto>>> BulkRegisterStreets(
        [FromBody] Application.Streets.Commands.BulkRegisterStreets.BulkRegisterStreetsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
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
