using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Buildings.Commands.CreateBuilding;
using TRRCMS.Application.Buildings.Commands.DeleteBuilding;
using TRRCMS.Application.Buildings.Commands.UpdateBuilding;
using TRRCMS.Application.Buildings.Commands.UpdateBuildingGeometry;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Buildings.Queries.GetAllBuildings;
using TRRCMS.Application.Buildings.Queries.GetBuilding;
using TRRCMS.Application.Buildings.Queries.GetBuildingsForMap;
using TRRCMS.Application.Buildings.Queries.GetBuildingsInPolygon;
using TRRCMS.Application.Buildings.Queries.SearchBuildings;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Buildings management API controller
/// </summary>
/// <remarks>
/// Provides endpoints for building CRUD and search operations.
/// إدارة المباني - UC-007 Building Management
/// 
/// **Building Code Format (رمز البناء):**
/// - Stored: GGDDSSCCNCNNBBBBB (17 digits, no dashes)
/// - Displayed: GG-DD-SS-CCC-NNN-BBBBB (with dashes via `buildingIdFormatted`)
/// 
/// **Code Segments:**
/// | Segment | Digits | Arabic | Example |
/// |---------|--------|--------|---------|
/// | GG | 2 | محافظة (Governorate) | 01 |
/// | DD | 2 | مدينة (District) | 01 |
/// | SS | 2 | بلدة (SubDistrict) | 01 |
/// | CCC | 3 | قرية (Community) | 001 |
/// | NNN | 3 | حي (Neighborhood) | 001 |
/// | BBBBB | 5 | رقم البناء (Building) | 00001 |
/// 
/// **BuildingType Values (نوع البناء):**
/// | Value | Name | Arabic |
/// |-------|------|--------|
/// | 1 | Residential | سكني |
/// | 2 | Commercial | تجاري |
/// | 3 | MixedUse | مختلط |
/// | 4 | Industrial | صناعي |
/// 
/// **BuildingStatus Values (حالة البناء):**
/// | Value | Name | Arabic |
/// |-------|------|--------|
/// | 1 | Existing | قائم |
/// | 2 | UnderConstruction | قيد الإنشاء |
/// | 3 | Damaged | متضرر |
/// | 4 | Destroyed | مدمر |
/// | 5 | Demolished | مهدوم |
/// | 99 | Unknown | غير معروف |
/// 
/// **Permissions:**
/// - Buildings_View (4000) - CanViewAllBuildings
/// - Buildings_Create (4001) - CanCreateBuildings
/// - Buildings_Update (4002) - CanEditBuildings
/// - Buildings_Delete (4003) - CanDeleteBuildings
/// 
/// **Building Geometry (PostGIS):**
/// 
/// Buildings support polygon geometry stored via PostGIS (SRID 4326 / WGS84).
/// All endpoints that return building data include `buildingGeometryWkt` in responses.
/// 
/// | Field | Type | Description |
/// |-------|------|-------------|
/// | `latitude` | decimal? | GPS latitude (center or polygon centroid) |
/// | `longitude` | decimal? | GPS longitude (center or polygon centroid) |
/// | `buildingGeometryWkt` | string? | WKT geometry (POLYGON or POINT) |
/// 
/// **WKT coordinate order:** longitude latitude (X Y) — e.g. `POLYGON((37.134 36.202, ...))` 
/// 
/// **Frontend rendering:**
/// - Parse `buildingGeometryWkt` to detect type: starts with `POLYGON` → render footprint, starts with `POINT` → render marker
/// - If `buildingGeometryWkt` is `null`, fall back to `latitude`/`longitude` as a marker
/// - If all three are `null`, the building has no spatial data
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class BuildingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BuildingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ==================== CREATE ====================

    /// <summary>
    /// Create a new building 
    /// </summary>
    /// <remarks>
    /// Creates a new building record with administrative codes and details.
    /// إنشاء مبنى جديد
    /// 
    /// **Use Case**: UC-007 Building Management - Create Building
    /// 
    /// **Required Permission**: Buildings_Create (4001) - CanCreateBuildings policy
    /// 
    /// **Building Code Auto-Generation:**
    /// The system automatically generates a 17-digit building code from the administrative codes:
    /// `{governorateCode}{districtCode}{subDistrictCode}{communityCode}{neighborhoodCode}{buildingNumber}`
    /// 
    /// **Example Request:**
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
    ///   "buildingGeometryWkt": "POLYGON((37.1340 36.2018, 37.1346 36.2018, 37.1346 36.2024, 37.1340 36.2024, 37.1340 36.2018))",
    ///   "locationDescription": "بجانب المسجد الكبير",
    ///   "notes": "بناء سكني مؤلف من 5 طوابق"
    /// }
    /// ```
    /// 
    /// **Geometry Input Options:**
    /// - **Polygon only**: Provide `buildingGeometryWkt` — `latitude`/`longitude` will be computed from the centroid
    /// - **Point only**: Provide `latitude`/`longitude` — a POINT geometry will be auto-created
    /// - **Both**: Provide all three — polygon is stored as-is, lat/lng as provided
    /// - **Neither**: Building is created without spatial data (can be added later via `PUT /{id}/geometry`)
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "buildingId": "01010100300200001",
    ///   "buildingIdFormatted": "01-01-01-003-002-00001",
    ///   "buildingType": "Residential",
    ///   "status": "Existing",
    ///   "latitude": 36.2021,
    ///   "longitude": 37.1343,
    ///   "buildingGeometryWkt": "POLYGON((37.1340 36.2018, 37.1346 36.2018, 37.1346 36.2024, 37.1340 36.2024, 37.1340 36.2018))",
    ///   "createdAtUtc": "2026-01-31T10:00:00Z"
    /// }
    /// ```
    /// 
    /// **Geometry in Response:**
    /// - If `buildingGeometryWkt` was provided as POLYGON, `latitude`/`longitude` are the centroid
    /// - If only `latitude`/`longitude` were provided, `buildingGeometryWkt` returns a POINT geometry
    /// - WKT coordinate order: **longitude latitude** (X Y), SRID 4326 (WGS84)
    /// </remarks>
    /// <param name="command">Building creation details</param>
    /// <returns>Created building details</returns>
    /// <response code="201">Building created successfully</response>
    /// <response code="400">Invalid request or building code already exists</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_Create (4001)</response>
    /// <response code="409">Building with same code already exists</response>
    [HttpPost]
    [Authorize(Policy = "CanCreateBuildings")]
    [ProducesResponseType(typeof(BuildingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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

    // ==================== GET BY ID ====================

    /// <summary>
    /// Get building by ID
    /// </summary>
    /// <remarks>
    /// Returns complete building details including all codes, attributes, and location data.
    /// عرض تفاصيل المبنى
    /// 
    /// **Use Case**: UC-007 - View Building Details
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "buildingId": "01010100300200001",
    ///   "buildingIdFormatted": "01-01-01-003-002-00001",
    ///   "governorateCode": "01",
    ///   "governorateName": "حلب",
    ///   "districtCode": "01",
    ///   "districtName": "مدينة حلب",
    ///   "subDistrictCode": "01",
    ///   "subDistrictName": "حلب",
    ///   "communityCode": "003",
    ///   "communityName": "الجميلية",
    ///   "neighborhoodCode": "002",
    ///   "neighborhoodName": "العزيزية",
    ///   "buildingNumber": "00001",
    ///   "buildingType": "Residential",
    ///   "status": "Existing",
    ///   "numberOfPropertyUnits": 10,
    ///   "numberOfApartments": 8,
    ///   "numberOfShops": 2,
    ///   "latitude": 36.2021,
    ///   "longitude": 37.1343,
    ///   "buildingGeometryWkt": "POLYGON((37.1340 36.2018, 37.1346 36.2018, 37.1346 36.2024, 37.1340 36.2024, 37.1340 36.2018))",
    ///   "locationDescription": "بجانب المسجد الكبير",
    ///   "createdAtUtc": "2026-01-31T10:00:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Building GUID</param>
    /// <returns>Building details</returns>
    /// <response code="200">Building found</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_View (4000)</response>
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

    // ==================== GET ALL ====================

    /// <summary>
    /// Get all buildings
    /// </summary>
    /// <remarks>
    /// Returns list of all buildings. For large datasets, use search endpoint with pagination.
    /// عرض جميع المباني
    /// 
    /// **Use Case**: UC-007 - List Buildings
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Note**: For production use with large datasets, use `POST /search` with pagination.
    /// </remarks>
    /// <returns>List of all buildings</returns>
    /// <response code="200">Buildings retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_View (4000)</response>
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

    // ==================== SEARCH ====================

    /// <summary>
    /// Search buildings with filters and pagination
    /// </summary>
    /// <remarks>
    /// Search buildings with advanced filters and pagination support.
    /// البحث عن المباني
    /// 
    /// **Use Case**: UC-007 - Search Buildings
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Filter Options:**
    /// 
    /// | Filter | Type | Description |
    /// |--------|------|-------------|
    /// | governorateCode | string | Filter by governorate (محافظة) |
    /// | districtCode | string | Filter by district (مدينة) |
    /// | subDistrictCode | string | Filter by sub-district (بلدة) |
    /// | communityCode | string | Filter by community (قرية) |
    /// | neighborhoodCode | string | Filter by neighborhood (حي) |
    /// | buildingId | string | **Partial match** on building code (رمز البناء) - supports with/without dashes |
    /// | buildingNumber | string | Exact match on building number |
    /// | buildingType | int | 1=Residential, 2=Commercial, 3=MixedUse, 4=Industrial |
    /// | status | int | Building status |
    /// 
    /// **BuildingId Partial Match Examples:**
    /// - `"01-01"` → Finds all buildings starting with governorate 01, district 01
    /// - `"01010101"` → Finds buildings in specific area (works without dashes)
    /// - `"00001"` → Finds buildings with "00001" anywhere in the code
    /// 
    /// **Pagination:**
    /// - page: Page number (default: 1)
    /// - pageSize: Items per page (default: 20, max: 100)
    /// 
    /// **Sorting:**
    /// - sortBy: "buildingId", "createdDate", "status", "buildingType"
    /// - sortDescending: true/false (default: false)
    /// 
    /// **Example Request - Search by partial building code:**
    /// ```json
    /// {
    ///   "buildingId": "01-01-01",
    ///   "page": 1,
    ///   "pageSize": 20
    /// }
    /// ```
    /// 
    /// **Example Request - Filter by area and type:**
    /// ```json
    /// {
    ///   "governorateCode": "01",
    ///   "districtCode": "01",
    ///   "buildingType": 1,
    ///   "page": 1,
    ///   "pageSize": 20,
    ///   "sortBy": "createdDate",
    ///   "sortDescending": true
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "buildings": [
    ///     {
    ///       "id": "guid-here",
    ///       "buildingId": "01010100300200001",
    ///       "buildingIdFormatted": "01-01-01-003-002-00001",
    ///       "buildingType": "Residential",
    ///       "status": "Existing",
    ///       "latitude": 36.2021,
    ///       "longitude": 37.1343,
    ///       "buildingGeometryWkt": "POLYGON((37.1340 36.2018, 37.1346 36.2018, 37.1346 36.2024, 37.1340 36.2024, 37.1340 36.2018))"
    ///     }
    ///   ],
    ///   "totalCount": 150,
    ///   "page": 1,
    ///   "pageSize": 20,
    ///   "totalPages": 8,
    ///   "hasPreviousPage": false,
    ///   "hasNextPage": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="query">Search criteria with filters and pagination</param>
    /// <returns>Paginated list of buildings</returns>
    /// <response code="200">Buildings retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_View (4000)</response>
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

    // ==================== UPDATE ====================

    /// <summary>
    /// Update building details
    /// </summary>
    /// <remarks>
    /// Updates building attributes. Administrative codes cannot be changed after creation.
    /// تحديث بيانات المبنى
    /// 
    /// **Use Case**: UC-007 - Update Building
    /// 
    /// **Required Permission**: Buildings_Update (4002) - CanEditBuildings policy
    /// 
    /// **Updatable Fields:**
    /// - buildingType
    /// - buildingStatus
    /// - damageLevel
    /// - numberOfPropertyUnits
    /// - numberOfApartments
    /// - numberOfShops
    /// - numberOfFloors
    /// - yearOfConstruction
    /// - locationDescription
    /// - notes
    /// 
    /// **Cannot Change:**
    /// - Administrative codes (governorate, district, etc.)
    /// - buildingId (auto-generated from codes)
    /// 
    /// **Example Request:**
    /// ```json
    /// {
    ///   "buildingType": 3,
    ///   "buildingStatus": 1,
    ///   "numberOfPropertyUnits": 12,
    ///   "notes": "تم تحديث البيانات"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Building ID</param>
    /// <param name="command">Update details</param>
    /// <returns>Updated building</returns>
    /// <response code="200">Building updated successfully</response>
    /// <response code="400">Invalid request or validation failed</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_Update (4002)</response>
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

    // ==================== UPDATE GEOMETRY ====================

    /// <summary>
    /// Update building geometry and coordinates
    /// </summary>
    /// <remarks>
    /// Updates GPS coordinates and/or polygon geometry for the building.
    /// تحديث إحداثيات المبنى
    /// 
    /// **Use Case**: UC-007 - Update Building Location
    /// 
    /// **Required Permission**: Buildings_Update (4002) - CanEditBuildings policy
    /// 
    /// **Fields:**
    /// - latitude: GPS latitude (-90 to 90)
    /// - longitude: GPS longitude (-180 to 180)
    /// - buildingGeometryWkt: Polygon in WKT format (optional)
    /// 
    /// **WKT Format Example:**
    /// ```
    /// POLYGON((37.1340 36.2020, 37.1345 36.2020, 37.1345 36.2025, 37.1340 36.2025, 37.1340 36.2020))
    /// ```
    /// 
    /// **Example Request - Point only:**
    /// ```json
    /// {
    ///   "latitude": 36.2021,
    ///   "longitude": 37.1343
    /// }
    /// ```
    /// 
    /// **Example Request - Polygon (building footprint):**
    /// ```json
    /// {
    ///   "latitude": 36.2021,
    ///   "longitude": 37.1343,
    ///   "buildingGeometryWkt": "POLYGON((37.1340 36.2018, 37.1346 36.2018, 37.1346 36.2024, 37.1340 36.2024, 37.1340 36.2018))"
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "buildingId": "01010100300200001",
    ///   "buildingIdFormatted": "01-01-01-003-002-00001",
    ///   "latitude": 36.2021,
    ///   "longitude": 37.1343,
    ///   "buildingGeometryWkt": "POLYGON((37.1340 36.2018, 37.1346 36.2018, 37.1346 36.2024, 37.1340 36.2024, 37.1340 36.2018))",
    ///   "buildingType": "Residential",
    ///   "status": "Existing"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Building ID</param>
    /// <param name="command">Geometry update details</param>
    /// <returns>Updated building</returns>
    /// <response code="200">Geometry updated successfully</response>
    /// <response code="400">Invalid coordinates or geometry</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_Update (4002)</response>
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

    // ==================== DELETE (SOFT DELETE) ====================

    /// <summary>
    /// Delete building (soft delete)
    /// </summary>
    /// <remarks>
    /// Soft deletes a building - marks as deleted but retains data for audit.
    /// حذف المبنى (حذف ناعم)
    /// 
    /// **Use Case**: UC-007 - Delete Building
    /// 
    /// **Required Permission**: Buildings_Delete (4003) - CanDeleteBuildings policy
    /// 
    /// **What happens:**
    /// - Sets `IsDeleted = true`
    /// - Records `DeletedAtUtc` timestamp
    /// - Records `DeletedBy` user ID
    /// - Building no longer appears in queries
    /// - Data retained for audit and potential recovery
    /// 
    /// **Restrictions:**
    /// - Cannot delete buildings with active surveys
    /// - Cannot delete buildings with linked claims
    /// 
    /// **Note:** This is a soft delete. Data can be recovered if needed.
    /// </remarks>
    /// <param name="id">Building ID to delete</param>
    /// <param name="command">Optional deletion reason</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Building deleted successfully</response>
    /// <response code="400">Building has active surveys or claims</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_Delete (4003)</response>
    /// <response code="404">Building not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanDeleteBuildings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBuilding(
        Guid id,
        [FromBody] DeleteBuildingCommand? command = null)
    {
        var deleteCommand = command ?? new DeleteBuildingCommand();
        deleteCommand.BuildingId = id;

        await _mediator.Send(deleteCommand);
        return NoContent();
    }

    // ==================== MAP ENDPOINTS ====================

    /// <summary>
    /// Get buildings for map display
    /// </summary>
    /// <remarks>
    /// Returns lightweight building data optimized for map rendering.
    /// Use bounding box coordinates to limit results to visible area.
    /// عرض المباني على الخريطة
    /// 
    /// **Use Case**: UC-007 - Map View
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Example Request:**
    /// ```json
    /// {
    ///   "minLatitude": 36.1900,
    ///   "maxLatitude": 36.2100,
    ///   "minLongitude": 37.1200,
    ///   "maxLongitude": 37.1500,
    ///   "buildingType": 1,
    ///   "maxResults": 500
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "buildingId": "01010100300200001",
    ///     "buildingIdFormatted": "01-01-01-003-002-00001",
    ///     "latitude": 36.2021,
    ///     "longitude": 37.1343,
    ///     "buildingGeometryWkt": "POLYGON((37.1340 36.2018, 37.1346 36.2018, 37.1346 36.2024, 37.1340 36.2024, 37.1340 36.2018))",
    ///     "status": "Existing",
    ///     "buildingType": "Residential",
    ///     "numberOfPropertyUnits": 8,
    ///     "numberOfApartments": 6,
    ///     "numberOfShops": 2
    ///   }
    /// ]
    /// ```
    /// 
    /// **Geometry Notes:**
    /// - `buildingGeometryWkt` is `null` for buildings without spatial data
    /// - POLYGON geometries can be rendered as building footprints on the map
    /// - POINT geometries should be rendered as markers
    /// - WKT coordinate order: **longitude latitude** (X Y)
    /// </remarks>
    /// <param name="query">Bounding box and filters</param>
    /// <returns>List of buildings with map data</returns>
    /// <response code="200">Buildings retrieved successfully</response>
    /// <response code="400">Invalid bounding box</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_View (4000)</response>
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

    // ==================== POLYGON SEARCH (PostGIS) ====================

    /// <summary>
    /// Get buildings within a polygon area
    /// </summary>
    /// <remarks>
    /// Search for buildings that fall within a specified polygon using PostGIS ST_Within.
    /// البحث عن المباني داخل مضلع جغرافي
    /// 
    /// **Use Case**: UC-007 - Spatial Search / Map Selection
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Polygon Input Options:**
    /// 
    /// **Option 1: WKT Format (recommended for complex polygons)**
    /// ```json
    /// {
    ///   "polygonWkt": "POLYGON((37.13 36.20, 37.14 36.20, 37.14 36.21, 37.13 36.21, 37.13 36.20))"
    /// }
    /// ```
    /// 
    /// **Option 2: Coordinates Array (easier for frontend)**
    /// ```json
    /// {
    ///   "coordinates": [
    ///     [37.13, 36.20],
    ///     [37.14, 36.20],
    ///     [37.14, 36.21],
    ///     [37.13, 36.21]
    ///   ]
    /// }
    /// ```
    /// Note: Polygon will be auto-closed if first != last coordinate
    /// 
    /// **WKT Format Rules:**
    /// - Coordinates are in `longitude latitude` order (X Y)
    /// - First and last coordinate must be identical to close the polygon
    /// - Use SRID 4326 (WGS84 - GPS coordinates)
    /// 
    /// **Optional Filters:**
    /// - buildingType: 1=Residential, 2=Commercial, 3=MixedUse, 4=Industrial
    /// - status: Building status filter
    /// - damageLevel: Damage assessment filter
    /// 
    /// **Pagination:**
    /// - page: Page number (default: 1)
    /// - pageSize: Items per page (default: 100, max: 1000)
    /// 
    /// **Performance Options:**
    /// - includeFullDetails: false (default) = lightweight response for map markers
    /// - includeFullDetails: true = full building details (slower)
    /// 
    /// **Example Request - Draw selection on map:**
    /// ```json
    /// {
    ///   "coordinates": [
    ///     [37.1340, 36.2020],
    ///     [37.1380, 36.2020],
    ///     [37.1380, 36.2060],
    ///     [37.1340, 36.2060]
    ///   ],
    ///   "buildingType": 1,
    ///   "page": 1,
    ///   "pageSize": 100,
    ///   "includeFullDetails": false
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "buildings": [
    ///     {
    ///       "id": "guid-here",
    ///       "buildingId": "01010100300200001",
    ///       "buildingIdFormatted": "01-01-01-003-002-00001",
    ///       "latitude": 36.2021,
    ///       "longitude": 37.1343,
    ///       "buildingGeometryWkt": "POLYGON((37.1340 36.2018, 37.1346 36.2018, 37.1346 36.2024, 37.1340 36.2024, 37.1340 36.2018))",
    ///       "buildingType": "Residential",
    ///       "status": "Existing",
    ///       "numberOfPropertyUnits": 8,
    ///       "neighborhoodName": "العزيزية",
    ///       "communityName": "الجميلية"
    ///     }
    ///   ],
    ///   "totalCount": 25,
    ///   "page": 1,
    ///   "pageSize": 100,
    ///   "totalPages": 1,
    ///   "polygonAreaSquareMeters": 156000.5,
    ///   "polygonWkt": "POLYGON((37.134 36.202, ...))"
    /// }
    /// ```
    /// </remarks>
    /// <param name="query">Polygon definition and optional filters</param>
    /// <returns>Buildings within the polygon with pagination</returns>
    /// <response code="200">Buildings retrieved successfully</response>
    /// <response code="400">Invalid polygon or coordinates</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_View (4000)</response>
    [HttpPost("polygon")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(GetBuildingsInPolygonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetBuildingsInPolygonResponse>> GetBuildingsInPolygon(
        [FromBody] GetBuildingsInPolygonQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
