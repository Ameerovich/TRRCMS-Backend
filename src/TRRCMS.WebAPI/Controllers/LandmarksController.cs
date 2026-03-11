using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Landmarks.Commands.DeleteLandmark;
using TRRCMS.Application.Landmarks.Commands.RegisterLandmark;
using TRRCMS.Application.Landmarks.Commands.UpdateLandmark;
using TRRCMS.Application.Landmarks.Dtos;
using TRRCMS.Application.Landmarks.Queries.GetLandmarkById;
using TRRCMS.Application.Landmarks.Queries.GetLandmarksForMap;
using TRRCMS.Application.Landmarks.Queries.SearchLandmarks;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Landmarks — point reference features on the map (mosques, schools, shops, etc.).
/// معالم — نقاط مرجعية على الخريطة
/// Managed via QGIS plugin through the API.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LandmarksController : ControllerBase
{
    private readonly IMediator _mediator;

    public LandmarksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ==================== QUERIES ====================

    /// <summary>
    /// Get landmarks within a bounding box for map rendering (point layer).
    /// Returns landmarks within the specified bounding box.
    /// </summary>
    /// <param name="northEastLat">North-east corner latitude</param>
    /// <param name="northEastLng">North-east corner longitude</param>
    /// <param name="southWestLat">South-west corner latitude</param>
    /// <param name="southWestLng">South-west corner longitude</param>
    /// <param name="type">Optional landmark type filter (1=PoliceStation, 2=Mosque, 3=PublicBuilding, 4=Shop, 5=School, 6=Clinic, 7=WaterTank, 8=FuelStation)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("map")]
    [ProducesResponseType(typeof(List<LandmarkMapDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LandmarkMapDto>>> GetLandmarksForMap(
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
        return Ok(result);
    }

    /// <summary>
    /// Search landmarks by name with optional type filter.
    /// Used by desk officers to locate landmarks based on applicant descriptions.
    /// </summary>
    /// <param name="query">Search text (partial name match)</param>
    /// <param name="type">Optional landmark type filter</param>
    /// <param name="maxResults">Max results (default 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<LandmarkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LandmarkDto>>> SearchLandmarks(
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
        return Ok(result);
    }

    /// <summary>
    /// Get a landmark by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LandmarkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LandmarkDto>> GetLandmarkById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLandmarkByIdQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    // ==================== COMMANDS (QGIS Plugin) ====================

    /// <summary>
    /// Register a new landmark (from QGIS plugin).
    /// Creates a new landmark with point geometry.
    /// تسجيل معلم جديد
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LandmarkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LandmarkDto>> RegisterLandmark(
        [FromBody] RegisterLandmarkCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetLandmarkById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing landmark (name, type, and optionally location).
    /// تعديل معلم موجود
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LandmarkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LandmarkDto>> UpdateLandmark(
        Guid id,
        [FromBody] UpdateLandmarkCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route ID does not match body ID.");

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a landmark (soft delete).
    /// حذف معلم
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLandmark(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteLandmarkCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}
