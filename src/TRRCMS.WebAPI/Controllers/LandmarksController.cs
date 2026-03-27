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
using TRRCMS.Application.Landmarks.Queries.GetLandmarkTypes;
using TRRCMS.Application.Landmarks.Commands.UpdateLandmarkTypeIcon;
using TRRCMS.Application.Common.Models;

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
    /// <param name="type">Optional landmark type filter (1=PoliceStation, 2=Mosque, 3=Square, 4=Shop, 5=School, 6=Clinic, 7=WaterTank, 8=FuelStation, 9=Hospital, 10=Park)</param>
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
    /// Bulk register landmarks (from QGIS plugin or desktop)
    /// تسجيل مجموعة معالم دفعة واحدة
    /// </summary>
    /// <remarks>
    /// Accepts an array of landmarks and processes them in a single transaction.
    ///
    /// **Behavior:**
    /// - Duplicates by Identifier are **skipped** (idempotent — safe to re-send)
    /// - Invalid WKT geometry items are **failed** with error details
    /// - Valid items are saved in a single database transaction
    /// - Partial success is possible: some items succeed while others skip/fail
    ///
    /// **Geometry format:** WKT POINT in longitude-latitude order, SRID 4326 (WGS84)
    ///
    /// **Landmark types:** 1=PoliceStation, 2=Mosque, 3=Square, 4=Shop, 5=School, 6=Clinic, 7=WaterTank, 8=FuelStation, 9=Hospital, 10=Park
    ///
    /// **Example request:**
    /// ```json
    /// {
    ///   "landmarks": [
    ///     { "identifier": 1, "name": "جامع الأموي", "type": 2, "locationWkt": "POINT(37.1340 36.2018)" },
    ///     { "identifier": 2, "name": "مدرسة الفرقان", "type": 5, "locationWkt": "POINT(37.1350 36.2025)" }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Array of landmarks to register</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bulk operation result with succeeded, failed, skipped counts and created items</returns>
    /// <response code="200">Operation completed. Check succeeded/failed/skipped counts in response.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("bulk-register")]
    [ProducesResponseType(typeof(Application.Common.Models.BulkOperationResult<LandmarkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Application.Common.Models.BulkOperationResult<LandmarkDto>>> BulkRegisterLandmarks(
        [FromBody] Application.Landmarks.Commands.BulkRegisterLandmarks.BulkRegisterLandmarksCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
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

    // ==================== LANDMARK TYPE ICONS ====================

    /// <summary>
    /// Get all landmark types with their SVG icons.
    /// الحصول على أنواع المعالم مع أيقوناتها
    /// </summary>
    /// <remarks>
    /// Returns all 10 landmark types with their current SVG icon content.
    /// Desktop/frontend should call this once on startup and cache the result.
    /// Re-fetch when icons may have been updated by an administrator.
    ///
    /// **Landmark types:** 1=PoliceStation, 2=Mosque, 3=Square, 4=Shop, 5=School, 6=Clinic, 7=WaterTank, 8=FuelStation, 9=Hospital, 10=Park
    /// </remarks>
    /// <response code="200">All landmark types with SVG icons.</response>
    [HttpGet("types")]
    [ProducesResponseType(typeof(ListResponse<LandmarkTypeIconDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListResponse<LandmarkTypeIconDto>>> GetLandmarkTypes(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLandmarkTypesQuery(), cancellationToken);
        return Ok(ListResponse<LandmarkTypeIconDto>.From(result));
    }

    /// <summary>
    /// Update the SVG icon for a landmark type (Admin only).
    /// تحديث أيقونة نوع المعلم
    /// </summary>
    /// <remarks>
    /// Replaces the SVG icon for the specified landmark type.
    /// The change takes effect immediately — desktop clients should re-fetch landmark types.
    ///
    /// **Example request:**
    /// ```json
    /// {
    ///   "type": 2,
    ///   "svgContent": "&lt;svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'&gt;...&lt;/svg&gt;"
    /// }
    /// ```
    /// </remarks>
    /// <param name="type">Landmark type code (1-10)</param>
    /// <param name="command">The update command with SVG content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Icon updated successfully.</response>
    /// <response code="400">Invalid type or SVG content.</response>
    /// <response code="403">Insufficient permissions (requires Landmarks_Manage).</response>
    /// <response code="404">Landmark type icon not found.</response>
    [HttpPut("types/{type:int}")]
    [Authorize(Policy = "CanManageLandmarks")]
    [ProducesResponseType(typeof(LandmarkTypeIconDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LandmarkTypeIconDto>> UpdateLandmarkTypeIcon(
        int type,
        [FromBody] UpdateLandmarkTypeIconCommand command,
        CancellationToken cancellationToken)
    {
        if (type != command.Type)
            return BadRequest("Route type does not match body type.");

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
