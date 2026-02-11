using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Neighborhoods.Dtos;
using TRRCMS.Application.Neighborhoods.Queries.GetNeighborhoodByCode;
using TRRCMS.Application.Neighborhoods.Queries.GetNeighborhoods;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Neighborhood reference data API controller.
/// Provides spatial reference data for map navigation and boundary rendering.
/// بيانات مرجعية للأحياء
/// 
/// **Endpoints:**
/// | Method | Path | Description |
/// |--------|------|-------------|
/// | GET | /api/v1/neighborhoods | List all neighborhoods (filterable by hierarchy) |
/// | GET | /api/v1/neighborhoods/{code} | Get neighborhood by full 12-digit code |
/// | GET | /api/v1/neighborhoods/by-codes | Get neighborhood by individual hierarchy codes |
/// | GET | /api/v1/neighborhoods/by-bounds | Get neighborhoods visible in map viewport |
/// 
/// **Frontend Integration:**
/// - Use `centerLatitude`/`centerLongitude` for "fly-to" map navigation
/// - Use `boundaryWkt` to render neighborhood boundary polygon on map
/// - Use `zoomLevel` to set map zoom when navigating to neighborhood
/// - WKT coordinate order: **longitude latitude** (X Y), SRID 4326
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class NeighborhoodsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NeighborhoodsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    // ==================== LIST ====================

    /// <summary>
    /// Get all neighborhoods, optionally filtered by parent hierarchy
    /// </summary>
    /// <remarks>
    /// Returns all active neighborhoods with center coordinates and boundary polygons.
    /// Used for populating dropdown selectors and loading map boundaries.
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Example Request - All Aleppo neighborhoods:**
    /// ```
    /// GET /api/v1/neighborhoods?governorateCode=01&amp;districtCode=01
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "governorateCode": "01",
    ///     "districtCode": "01",
    ///     "subDistrictCode": "01",
    ///     "communityCode": "001",
    ///     "neighborhoodCode": "001",
    ///     "fullCode": "010101001001",
    ///     "nameArabic": "الجميلية",
    ///     "nameEnglish": "Al-Jamiliyah",
    ///     "centerLatitude": 36.2025,
    ///     "centerLongitude": 37.1325,
    ///     "boundaryWkt": "POLYGON((37.130 36.200, 37.135 36.200, 37.135 36.205, 37.130 36.205, 37.130 36.200))",
    ///     "areaSquareKm": 0.50,
    ///     "zoomLevel": 15,
    ///     "isActive": true
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="governorateCode">Filter by governorate code (محافظة)</param>
    /// <param name="districtCode">Filter by district code (مدينة)</param>
    /// <param name="subDistrictCode">Filter by sub-district code (بلدة)</param>
    /// <param name="communityCode">Filter by community code (قرية)</param>
    /// <returns>List of neighborhoods with spatial data</returns>
    /// <response code="200">Neighborhoods retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(List<NeighborhoodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<NeighborhoodDto>>> GetNeighborhoods(
        [FromQuery] string? governorateCode,
        [FromQuery] string? districtCode,
        [FromQuery] string? subDistrictCode,
        [FromQuery] string? communityCode,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNeighborhoodsQuery
        {
            GovernorateCode = governorateCode,
            DistrictCode = districtCode,
            SubDistrictCode = subDistrictCode,
            CommunityCode = communityCode
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // ==================== GET BY FULL CODE ====================

    /// <summary>
    /// Get neighborhood by full 12-digit code
    /// </summary>
    /// <remarks>
    /// Looks up a neighborhood using its full composite code (GGDDSSCCCCNNN).
    /// The full code matches the first 12 characters of Building.BuildingId.
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Example Request:**
    /// ```
    /// GET /api/v1/neighborhoods/010101001001
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "neighborhoodCode": "001",
    ///   "fullCode": "010101001001",
    ///   "nameArabic": "الجميلية",
    ///   "nameEnglish": "Al-Jamiliyah",
    ///   "centerLatitude": 36.2025,
    ///   "centerLongitude": 37.1325,
    ///   "boundaryWkt": "POLYGON((37.130 36.200, 37.135 36.200, 37.135 36.205, 37.130 36.205, 37.130 36.200))",
    ///   "areaSquareKm": 0.50,
    ///   "zoomLevel": 15,
    ///   "isActive": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="fullCode">Full 12-digit composite code</param>
    /// <returns>Neighborhood details with spatial data</returns>
    /// <response code="200">Neighborhood found</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">Neighborhood not found</response>
    [HttpGet("{fullCode}")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(NeighborhoodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NeighborhoodDto>> GetNeighborhoodByFullCode(
        string fullCode,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNeighborhoodByCodeQuery { FullCode = fullCode };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Neighborhood with code '{fullCode}' not found" });

        return Ok(result);
    }

    // ==================== GET BY INDIVIDUAL CODES ====================

    /// <summary>
    /// Get neighborhood by individual hierarchy codes
    /// </summary>
    /// <remarks>
    /// Looks up a neighborhood using separate admin hierarchy codes.
    /// Useful when frontend has codes from Building entity fields.
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Example Request:**
    /// ```
    /// GET /api/v1/neighborhoods/by-codes?governorateCode=01&amp;districtCode=01&amp;subDistrictCode=01&amp;communityCode=001&amp;neighborhoodCode=001
    /// ```
    /// </remarks>
    /// <param name="governorateCode">Governorate code — 2 digits</param>
    /// <param name="districtCode">District code — 2 digits</param>
    /// <param name="subDistrictCode">Sub-district code — 2 digits</param>
    /// <param name="communityCode">Community code — 3 digits</param>
    /// <param name="neighborhoodCode">Neighborhood code — 3 digits</param>
    /// <returns>Neighborhood details</returns>
    /// <response code="200">Neighborhood found</response>
    /// <response code="400">Missing required code parameters</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">Neighborhood not found</response>
    [HttpGet("by-codes")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(NeighborhoodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NeighborhoodDto>> GetNeighborhoodByCodes(
        [FromQuery] string governorateCode,
        [FromQuery] string districtCode,
        [FromQuery] string subDistrictCode,
        [FromQuery] string communityCode,
        [FromQuery] string neighborhoodCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(governorateCode) ||
            string.IsNullOrWhiteSpace(districtCode) ||
            string.IsNullOrWhiteSpace(subDistrictCode) ||
            string.IsNullOrWhiteSpace(communityCode) ||
            string.IsNullOrWhiteSpace(neighborhoodCode))
        {
            return BadRequest(new { message = "All five hierarchy codes are required" });
        }

        var query = new GetNeighborhoodByCodeQuery
        {
            GovernorateCode = governorateCode,
            DistrictCode = districtCode,
            SubDistrictCode = subDistrictCode,
            CommunityCode = communityCode,
            NeighborhoodCode = neighborhoodCode
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = "Neighborhood not found for the given codes" });

        return Ok(result);
    }

    // ==================== GET BY BOUNDING BOX ====================

    /// <summary>
    /// Get neighborhoods visible in map viewport
    /// </summary>
    /// <remarks>
    /// Returns neighborhoods whose boundaries intersect the given bounding box.
    /// Used to dynamically load neighborhood boundaries as the user pans the map.
    /// Uses PostGIS ST_Intersects with GiST spatial index.
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Example Request:**
    /// ```
    /// GET /api/v1/neighborhoods/by-bounds?swLat=36.185&amp;swLng=37.100&amp;neLat=36.220&amp;neLng=37.175
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "neighborhoodCode": "001",
    ///     "fullCode": "010101001001",
    ///     "nameArabic": "الجميلية",
    ///     "centerLatitude": 36.2025,
    ///     "centerLongitude": 37.1325,
    ///     "boundaryWkt": "POLYGON((37.130 36.200, 37.135 36.200, 37.135 36.205, 37.130 36.205, 37.130 36.200))"
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="swLat">Southwest corner latitude</param>
    /// <param name="swLng">Southwest corner longitude</param>
    /// <param name="neLat">Northeast corner latitude</param>
    /// <param name="neLng">Northeast corner longitude</param>
    /// <returns>Neighborhoods intersecting the viewport</returns>
    /// <response code="200">Neighborhoods retrieved</response>
    /// <response code="400">Invalid bounding box</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("by-bounds")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(List<NeighborhoodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<NeighborhoodDto>>> GetNeighborhoodsByBounds(
        [FromQuery] decimal swLat,
        [FromQuery] decimal swLng,
        [FromQuery] decimal neLat,
        [FromQuery] decimal neLng,
        CancellationToken cancellationToken = default)
    {
        if (swLat >= neLat || swLng >= neLng)
        {
            return BadRequest(new { message = "Invalid bounding box: SW must be less than NE" });
        }

        var repository = HttpContext.RequestServices
            .GetRequiredService<Application.Common.Interfaces.INeighborhoodRepository>();

        var neighborhoods = await repository.GetInBoundingBoxAsync(
            swLat, swLng, neLat, neLng, cancellationToken);

        var result = neighborhoods.Select(n => new NeighborhoodDto
        {
            Id = n.Id,
            GovernorateCode = n.GovernorateCode,
            DistrictCode = n.DistrictCode,
            SubDistrictCode = n.SubDistrictCode,
            CommunityCode = n.CommunityCode,
            NeighborhoodCode = n.NeighborhoodCode,
            FullCode = n.FullCode,
            NameArabic = n.NameArabic,
            NameEnglish = n.NameEnglish,
            CenterLatitude = n.CenterLatitude,
            CenterLongitude = n.CenterLongitude,
            BoundaryWkt = n.BoundaryWkt,
            AreaSquareKm = n.AreaSquareKm,
            ZoomLevel = n.ZoomLevel,
            IsActive = n.IsActive
        }).ToList();

        return Ok(result);
    }

    // ==================== GET BY POINT (Reverse Geocode) ====================

    /// <summary>
    /// Get the neighborhood containing a given point (lat/lng)
    /// </summary>
    /// <remarks>
    /// Returns the neighborhood whose boundary polygon contains the given coordinate.
    /// Uses PostGIS ST_Contains with GiST spatial index for fast lookup.
    /// 
    /// **Use Cases:**
    /// - When user clicks on the map → identify which neighborhood the click falls in
    /// - When creating a building → auto-detect its neighborhood from GPS coordinates
    /// - Reverse geocoding: lat/lng → neighborhood name and code
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Example Request:**
    /// ```
    /// GET /api/v1/neighborhoods/by-point?latitude=36.2025&amp;longitude=37.1325
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "neighborhoodCode": "001",
    ///   "fullCode": "010101001001",
    ///   "nameArabic": "الجميلية",
    ///   "nameEnglish": "Al-Jamiliyah",
    ///   "centerLatitude": 36.2025,
    ///   "centerLongitude": 37.1325,
    ///   "boundaryWkt": "POLYGON((37.130 36.200, ...))",
    ///   "zoomLevel": 15
    /// }
    /// ```
    /// </remarks>
    /// <param name="latitude">Latitude of the point (e.g. 36.2025)</param>
    /// <param name="longitude">Longitude of the point (e.g. 37.1325)</param>
    /// <returns>Neighborhood containing the point, or 404 if no neighborhood contains it</returns>
    /// <response code="200">Neighborhood found</response>
    /// <response code="400">Invalid coordinates</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">No neighborhood contains this point</response>
    [HttpGet("by-point")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(NeighborhoodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NeighborhoodDto>> GetNeighborhoodByPoint(
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        CancellationToken cancellationToken = default)
    {
        // Basic coordinate validation (Syria is roughly lat 32-37, lng 35-42)
        if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180)
        {
            return BadRequest(new { message = "Invalid coordinates: latitude must be -90 to 90, longitude must be -180 to 180" });
        }

        var repository = HttpContext.RequestServices
            .GetRequiredService<Application.Common.Interfaces.INeighborhoodRepository>();

        var neighborhood = await repository.GetContainingPointAsync(
            latitude, longitude, cancellationToken);

        if (neighborhood == null)
        {
            return NotFound(new { message = $"No neighborhood found containing point ({latitude}, {longitude})" });
        }

        var result = new NeighborhoodDto
        {
            Id = neighborhood.Id,
            GovernorateCode = neighborhood.GovernorateCode,
            DistrictCode = neighborhood.DistrictCode,
            SubDistrictCode = neighborhood.SubDistrictCode,
            CommunityCode = neighborhood.CommunityCode,
            NeighborhoodCode = neighborhood.NeighborhoodCode,
            FullCode = neighborhood.FullCode,
            NameArabic = neighborhood.NameArabic,
            NameEnglish = neighborhood.NameEnglish,
            CenterLatitude = neighborhood.CenterLatitude,
            CenterLongitude = neighborhood.CenterLongitude,
            BoundaryWkt = neighborhood.BoundaryWkt,
            AreaSquareKm = neighborhood.AreaSquareKm,
            ZoomLevel = neighborhood.ZoomLevel,
            IsActive = neighborhood.IsActive
        };

        return Ok(result);
    }
}