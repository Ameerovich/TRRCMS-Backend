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
    /// </summary>
    /// <remarks>
    /// **Purpose**: Creates a new building record with administrative codes and details.
    /// 
    /// **Building Code Format (رمز البناء)**:
    /// - Stored: GGDDSSCCNCNNBBBBB (17 digits, no dashes)
    /// - Displayed: GG-DD-SS-CCC-NNN-BBBBB (with dashes via `buildingIdFormatted`)
    /// 
    /// **Code Segments**:
    /// - GG: Governorate code (2 digits)
    /// - DD: District code (2 digits)  
    /// - SS: Sub-district code (2 digits)
    /// - CCC: Community code (3 digits)
    /// - NNN: Neighborhood code (3 digits)
    /// - BBBBB: Building number (5 digits)
    /// 
    /// **Building Types (نوع البناء)**:
    /// - 1 = Residential (سكني)
    /// - 2 = Commercial (تجاري)
    /// - 3 = MixedUse (مختلط)
    /// - 4 = Industrial (صناعي)
    /// 
    /// **Example Request**:
    /// ```json
    /// {
    ///   "governorateCode": "01",
    ///   "districtCode": "01",
    ///   "subDistrictCode": "01",
    ///   "communityCode": "003",
    ///   "neighborhoodCode": "002",
    ///   "buildingNumber": "00001",
    ///   "buildingType": 1,
    ///   "buildingStatus": 1,
    ///   "numberOfPropertyUnits": 10,
    ///   "numberOfApartments": 8,
    ///   "numberOfShops": 2,
    ///   "latitude": 36.2021,
    ///   "longitude": 37.1343,
    ///   "locationDescription": "بجانب المسجد الكبير",
    ///   "notes": "بناء سكني مؤلف من 5 طوابق"
    /// }
    /// ```
    /// 
    /// **Response**: Returns the created building with generated building code.
    /// </remarks>
    /// <param name="command">Building creation details</param>
    /// <returns>Created building details</returns>
    /// <response code="201">Building created successfully</response>
    /// <response code="400">Invalid request or building code already exists</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission</response>
    [HttpPost]
    [Authorize(Policy = "CanCreateBuildings")]
    [ProducesResponseType(typeof(BuildingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BuildingDto>> CreateBuilding([FromBody] CreateBuildingCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetBuilding), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get building by ID
    /// </summary>
    /// <remarks>
    /// Returns complete building details including all codes, attributes, and location data.
    /// </remarks>
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
            return NotFound(new { error = $"Building with ID {id} not found." });

        return Ok(building);
    }

    /// <summary>
    /// Get all buildings
    /// </summary>
    /// <remarks>
    /// Returns list of all buildings. For large datasets, use search endpoint with pagination.
    /// </remarks>
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
    /// Search buildings with filters and pagination
    /// </summary>
    /// <remarks>
    /// **Purpose**: Search buildings with advanced filters and pagination support.
    /// 
    /// **Filter Options**:
    /// - governorateCode: Filter by governorate
    /// - districtCode: Filter by district
    /// - buildingType: Filter by type (1=Residential, 2=Commercial, 3=MixedUse, 4=Industrial)
    /// - status: Filter by status
    /// - searchText: Search in building codes and addresses
    /// 
    /// **Pagination**:
    /// - page: Page number (default: 1)
    /// - pageSize: Items per page (default: 20, max: 100)
    /// </remarks>
    /// <param name="query">Search criteria with filters and pagination</param>
    /// <returns>Paginated list of buildings</returns>
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
    /// Update building details
    /// </summary>
    /// <remarks>
    /// **Note**: Administrative codes cannot be changed after creation.
    /// 
    /// **Updatable Fields**:
    /// - buildingType
    /// - buildingStatus
    /// - numberOfPropertyUnits
    /// - numberOfApartments
    /// - numberOfShops
    /// - locationDescription
    /// - notes
    /// </remarks>
    /// <param name="id">Building ID</param>
    /// <param name="command">Update details</param>
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
    /// Update building geometry and coordinates
    /// </summary>
    /// <remarks>
    /// Updates GPS coordinates and/or polygon geometry for the building.
    /// 
    /// **Fields**:
    /// - latitude: GPS latitude
    /// - longitude: GPS longitude
    /// - buildingGeometryWkt: Polygon in WKT format (optional)
    /// </remarks>
    /// <param name="id">Building ID</param>
    /// <param name="command">Geometry update details</param>
    /// <returns>Updated building</returns>
    /// <response code="200">Geometry updated successfully</response>
    /// <response code="400">Invalid coordinates or geometry</response>
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
    /// Get buildings for map display
    /// </summary>
    /// <remarks>
    /// Returns lightweight building data optimized for map rendering.
    /// Use bounding box coordinates to limit results to visible area.
    /// </remarks>
    /// <param name="query">Bounding box and filters</param>
    /// <returns>List of buildings with map data</returns>
    /// <response code="200">Buildings retrieved successfully</response>
    /// <response code="400">Invalid bounding box</response>
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