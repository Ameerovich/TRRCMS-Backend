using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Buildings.Commands.CreateBuilding;
using TRRCMS.Application.Buildings.Commands.UpdateBuilding;
using TRRCMS.Application.Buildings.Commands.UpdateBuildingGeometry;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Buildings.Queries.GetAllBuildings;
using TRRCMS.Application.Buildings.Queries.GetBuilding;
using TRRCMS.Application.Buildings.Queries.GetBuildingsForMap;
using TRRCMS.Application.Buildings.Queries.SearchBuildings;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Buildings management API controller
/// Provides endpoints for building CRUD and search operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BuildingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BuildingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new building
    /// Requires: Buildings_Create permission
    /// </summary>
    /// <param name="command">Building creation details</param>
    /// <returns>Created building ID</returns>
    /// <response code="201">Building created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission</response>
    [HttpPost]
    [Authorize(Policy = "CanCreateBuildings")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Guid>> CreateBuilding(CreateBuildingCommand command)
    {
        try
        {
            var buildingId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetBuilding), new { id = buildingId }, buildingId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get building by ID
    /// Requires: Buildings_View permission
    /// </summary>
    /// <param name="id">Building GUID</param>
    /// <returns>Building details</returns>
    /// <response code="200">Building found</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission</response>
    /// <response code="404">Building not found</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(BuildingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDto>> GetBuilding(Guid id)
    {
        var query = new GetBuildingQuery { Id = id };
        var building = await _mediator.Send(query);

        if (building == null)
            return NotFound($"Building with ID {id} not found.");

        return Ok(building);
    }

    /// <summary>
    /// Get all buildings (basic list without filters)
    /// Requires: Buildings_View permission
    /// </summary>
    /// <returns>List of all buildings</returns>
    /// <response code="200">Buildings retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(List<BuildingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<BuildingDto>>> GetAllBuildings()
    {
        var query = new GetAllBuildingsQuery();
        var buildings = await _mediator.Send(query);
        return Ok(buildings);
    }

    /// <summary>
    /// Search buildings with advanced filters and pagination
    /// UC-000: Manage Building Data
    /// Requires: Buildings_View permission
    /// </summary>
    /// <param name="query">Search criteria with filters and pagination</param>
    /// <returns>Paginated list of buildings matching search criteria</returns>
    /// <response code="200">Buildings retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission</response>
    [HttpPost("search")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(SearchBuildingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SearchBuildingsResponse>> SearchBuildings(
        [FromBody] SearchBuildingsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update existing building
    /// UC-000: Manage Building Data
    /// Requires: Buildings_Update permission
    /// Note: Administrative codes cannot be changed after creation
    /// </summary>
    /// <param name="id">Building ID</param>
    /// <param name="command">Update details with reason for modification</param>
    /// <returns>Updated building</returns>
    /// <response code="200">Building updated successfully</response>
    /// <response code="400">Invalid request or validation failed</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission</response>
    /// <response code="404">Building not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanEditBuildings")]
    [ProducesResponseType(typeof(BuildingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDto>> UpdateBuilding(
        Guid id,
        [FromBody] UpdateBuildingCommand command)
    {
        command.BuildingId = id;
        var building = await _mediator.Send(command);
        return Ok(building);
    }
    /// <summary>
    /// Update building geometry and GPS coordinates
    /// UC-000: Manage Building Data
    /// Requires: Buildings_Update permission
    /// </summary>
    /// <param name="id">Building ID</param>
    /// <param name="command">Geometry/coordinate update details</param>
    /// <returns>Updated building</returns>
    /// <response code="200">Geometry updated successfully</response>
    /// <response code="400">Invalid request or validation failed</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission</response>
    /// <response code="404">Building not found</response>
    [HttpPut("{id}/geometry")]
    [Authorize(Policy = "CanEditBuildings")]
    [ProducesResponseType(typeof(BuildingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDto>> UpdateBuildingGeometry(
        Guid id,
        [FromBody] UpdateBuildingGeometryCommand command)
    {
        command.BuildingId = id;
        var building = await _mediator.Send(command);
        return Ok(building);
    }
    /// <summary>
    /// Get buildings for map display within bounding box
    /// UC-000: Manage Building Data - Map View
    /// Returns lightweight DTOs optimized for rendering thousands of buildings
    /// Requires: Buildings_View permission
    /// </summary>
    /// <param name="query">Bounding box and optional filters</param>
    /// <returns>List of buildings with essential map data</returns>
    /// <response code="200">Buildings retrieved successfully</response>
    /// <response code="400">Invalid bounding box parameters</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission</response>
    [HttpPost("map")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(List<BuildingMapDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<BuildingMapDto>>> GetBuildingsForMap(
        [FromBody] GetBuildingsForMapQuery query)
    {
        var buildings = await _mediator.Send(query);
        return Ok(buildings);
    }
}