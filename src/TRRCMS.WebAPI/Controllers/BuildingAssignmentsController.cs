using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.BuildingAssignments.Commands.AssignBuildings;
using TRRCMS.Application.BuildingAssignments.Commands.UnassignBuilding;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.BuildingAssignments.Queries.GetAssignmentById;
using TRRCMS.Application.BuildingAssignments.Queries.GetAvailableFieldCollectors;
using TRRCMS.Application.BuildingAssignments.Queries.GetBuildingsForAssignment;
using TRRCMS.Application.BuildingAssignments.Queries.GetFieldCollectorAssignments;
using TRRCMS.Application.BuildingAssignments.Queries.GetPropertyUnitsForRevisit;
using TRRCMS.Domain.Enums;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Building Assignment API controller
/// تعيين المباني لجامعي البيانات الميدانيين
/// </summary>
/// <remarks>
/// Provides endpoints for assigning buildings to field collectors.
/// UC-012: Assign Buildings to Field Collectors
/// 
/// **Workflow Overview:**
/// 1. Search buildings by administrative hierarchy, location, OR polygon (S01-S03)
/// 2. Review property units for revisit selection (S04-S05)
/// 3. Select field collector with workload info
/// 4. Assign buildings to collector (S06-S07)
/// 5. Transfer happens during tablet synchronization (S08-S12)
/// 
/// **TransferStatus Values (حالة النقل):**
/// | Value | Name | Arabic | Description |
/// |-------|------|--------|-------------|
/// | 1 | Pending | قيد الانتظار | Assignment created, not yet transferred |
/// | 2 | InProgress | جاري النقل | Transfer in progress |
/// | 3 | Transferred | تم النقل | Successfully transferred to tablet |
/// | 4 | Failed | فشل النقل | Transfer failed |
/// | 5 | Cancelled | ملغي | Assignment cancelled |
/// | 6 | PartialTransfer | نقل جزئي | Partial transfer completed |
/// | 7 | Synchronized | متزامن | Data synchronized back from tablet |
/// 
/// **Priority Values (الأولوية):**
/// | Value | Arabic |
/// |-------|--------|
/// | Normal | عادي |
/// | High | عالي |
/// | Urgent | عاجل |
/// 
/// **Permissions:**
/// - Buildings_View (4000) - CanViewAllBuildings - View buildings and assignments
/// - Buildings_Assign (4003) - CanAssignBuildings - Assign/unassign buildings
/// 
/// **Roles with Buildings_Assign Permission:**
/// - Administrator (مدير النظام)
/// - DataManager (مدير البيانات)
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class BuildingAssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BuildingAssignmentsController> _logger;

    public BuildingAssignmentsController(IMediator mediator, ILogger<BuildingAssignmentsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ==================== BUILDING SEARCH ====================

    /// <summary>
    /// Get buildings available for assignment
    /// البحث عن المباني المتاحة للتعيين
    /// </summary>
    /// <remarks>
    /// Search and filter buildings for assignment to field collectors.
    /// UC-012: S01-S03 - Search and select buildings
    /// 
    /// **Use Case**: UC-012 Assign Buildings - Building Search
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Supported Filters:**
    /// 
    /// **Administrative Hierarchy (التسلسل الإداري):**
    /// - governorateCode: محافظة (e.g., "01")
    /// - districtCode: مدينة (e.g., "01")
    /// - subDistrictCode: بلدة (e.g., "01")
    /// - communityCode: قرية (e.g., "001")
    /// - neighborhoodCode: حي (e.g., "001")
    /// 
    /// **Building Filters:**
    /// - buildingCode: رمز البناء (partial match search)
    /// - address: العنوان (partial match)
    /// - buildingType: نوع البناء (1=Residential, 2=Commercial, 3=MixedUse, 4=Industrial)
    /// - buildingStatus: حالة البناء (1=Existing, 2=UnderConstruction, 3=Damaged, 4=Destroyed, 5=Demolished)
    /// 
    /// **Assignment Filter:**
    /// - hasActiveAssignment: null=all, true=assigned only, false=unassigned only
    /// 
    /// **Spatial Filters (البحث الجغرافي):**
    /// 
    /// *Option 1: Radius Search (البحث بنصف القطر)*
    /// - latitude, longitude: Center point coordinates
    /// - radiusMeters: Search radius in meters
    /// 
    /// *Option 2: Polygon Search (البحث بالمضلع)*
    /// - polygonWkt: Polygon in WKT format (e.g., "POLYGON((37.13 36.20, 37.14 36.20, 37.14 36.21, 37.13 36.21, 37.13 36.20))")
    /// - Note: Coordinates are in longitude-latitude order. First and last coordinate must be identical.
    /// 
    /// **Example Request (Regular):**
    /// ```
    /// GET /api/v1/buildingassignments/buildings?governorateCode=01&amp;districtCode=01&amp;hasActiveAssignment=false&amp;page=1&amp;pageSize=20
    /// ```
    /// 
    /// **Example Request (Radius Search):**
    /// ```
    /// GET /api/v1/buildingassignments/buildings?latitude=36.2021&amp;longitude=37.1343&amp;radiusMeters=1000&amp;hasActiveAssignment=false
    /// ```
    /// 
    /// **Example Request (Polygon Search):**
    /// ```
    /// GET /api/v1/buildingassignments/buildings?polygonWkt=POLYGON((37.13 36.20, 37.14 36.20, 37.14 36.21, 37.13 36.21, 37.13 36.20))&amp;hasActiveAssignment=false
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "buildingCode": "01010100300200001",
    ///       "address": "شارع الجمهورية",
    ///       "governorateCode": "01",
    ///       "governorateName": "حلب",
    ///       "numberOfPropertyUnits": 10,
    ///       "buildingType": "Residential",
    ///       "buildingStatus": "Existing",
    ///       "latitude": 36.2021,
    ///       "longitude": 37.1343,
    ///       "hasActiveAssignment": false,
    ///       "currentAssignmentId": null,
    ///       "currentAssigneeName": null
    ///     }
    ///   ],
    ///   "totalCount": 150,
    ///   "page": 1,
    ///   "pageSize": 20,
    ///   "totalPages": 8,
    ///   "polygonWkt": null,
    ///   "polygonAreaSquareMeters": null
    /// }
    /// ```
    /// 
    /// **Example Response (Polygon Search):**
    /// ```json
    /// {
    ///   "items": [...],
    ///   "totalCount": 25,
    ///   "page": 1,
    ///   "pageSize": 20,
    ///   "totalPages": 2,
    ///   "polygonWkt": "POLYGON((37.13 36.20, 37.14 36.20, 37.14 36.21, 37.13 36.21, 37.13 36.20))",
    ///   "polygonAreaSquareMeters": 156000.5
    /// }
    /// ```
    /// </remarks>
    /// <param name="governorateCode">Filter by governorate code (محافظة)</param>
    /// <param name="districtCode">Filter by district code (مدينة)</param>
    /// <param name="subDistrictCode">Filter by sub-district code (بلدة)</param>
    /// <param name="communityCode">Filter by community code (قرية)</param>
    /// <param name="neighborhoodCode">Filter by neighborhood code (حي)</param>
    /// <param name="buildingCode">Search by building code - partial match (رمز البناء)</param>
    /// <param name="address">Search by address - partial match (العنوان)</param>
    /// <param name="buildingType">Filter by building type (نوع البناء)</param>
    /// <param name="buildingStatus">Filter by building status (حالة البناء)</param>
    /// <param name="hasActiveAssignment">Filter by assignment status (حالة التعيين)</param>
    /// <param name="latitude">Center latitude for radius search</param>
    /// <param name="longitude">Center longitude for radius search</param>
    /// <param name="radiusMeters">Radius in meters for radius search</param>
    /// <param name="polygonWkt">Polygon in WKT format for polygon search (البحث بالمضلع)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100 for regular, 1000 for polygon)</param>
    /// <param name="sortBy">Sort field (e.g., "buildingCode", "address")</param>
    /// <param name="sortDescending">Sort direction (default: false)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of buildings available for assignment</returns>
    /// <response code="200">Buildings retrieved successfully</response>
    /// <response code="400">Invalid polygon format</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_View (4000)</response>
    [HttpGet("buildings")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(BuildingsForAssignmentPagedResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BuildingsForAssignmentPagedResult>> GetBuildingsForAssignment(
        [FromQuery] string? governorateCode,
        [FromQuery] string? districtCode,
        [FromQuery] string? subDistrictCode,
        [FromQuery] string? communityCode,
        [FromQuery] string? neighborhoodCode,
        [FromQuery] string? buildingCode,
        [FromQuery] string? address,
        [FromQuery] BuildingType? buildingType,
        [FromQuery] BuildingStatus? buildingStatus,
        [FromQuery] bool? hasActiveAssignment,
        [FromQuery] decimal? latitude,
        [FromQuery] decimal? longitude,
        [FromQuery] int? radiusMeters,
        [FromQuery] string? polygonWkt,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        // Determine max page size based on search type
        var maxPageSize = !string.IsNullOrWhiteSpace(polygonWkt) ? 1000 : 100;

        var query = new GetBuildingsForAssignmentQuery
        {
            GovernorateCode = governorateCode,
            DistrictCode = districtCode,
            SubDistrictCode = subDistrictCode,
            CommunityCode = communityCode,
            NeighborhoodCode = neighborhoodCode,
            BuildingCode = buildingCode,
            Address = address,
            BuildingType = buildingType,
            BuildingStatus = buildingStatus,
            HasActiveAssignment = hasActiveAssignment,
            Latitude = latitude,
            Longitude = longitude,
            RadiusMeters = radiusMeters,
            PolygonWkt = polygonWkt,
            Page = page,
            PageSize = Math.Min(pageSize, maxPageSize),
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // ==================== BUILDING SEARCH (POST - Polygon with Coordinates) ====================

    /// <summary>
    /// Search buildings for assignment using polygon coordinates
    /// البحث عن المباني داخل مضلع جغرافي باستخدام الإحداثيات
    /// </summary>
    /// <remarks>
    /// Search buildings within a polygon area using coordinate arrays.
    /// Use this endpoint when drawing selection on map (easier for frontend).
    /// UC-012: S01-S03 - Search buildings in polygon
    /// 
    /// **Use Case**: UC-012 Assign Buildings - Polygon Search (Map Drawing)
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Polygon Input Options:**
    /// 
    /// *Option 1: WKT Format*
    /// ```json
    /// {
    ///   "polygonWkt": "POLYGON((37.13 36.20, 37.14 36.20, 37.14 36.21, 37.13 36.21, 37.13 36.20))"
    /// }
    /// ```
    /// 
    /// *Option 2: Coordinates Array (easier for frontend)*
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
    /// Note: Coordinates are [longitude, latitude] pairs. Polygon auto-closed if needed.
    /// 
    /// **Full Example Request:**
    /// ```json
    /// {
    ///   "coordinates": [
    ///     [37.13, 36.20],
    ///     [37.14, 36.20],
    ///     [37.14, 36.21],
    ///     [37.13, 36.21]
    ///   ],
    ///   "governorateCode": "01",
    ///   "hasActiveAssignment": false,
    ///   "buildingType": 1,
    ///   "page": 1,
    ///   "pageSize": 100
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "buildingCode": "01010100300200001",
    ///       "address": "شارع الجمهورية",
    ///       "governorateCode": "01",
    ///       "governorateName": "حلب",
    ///       "numberOfPropertyUnits": 10,
    ///       "buildingType": "Residential",
    ///       "buildingStatus": "Existing",
    ///       "latitude": 36.2021,
    ///       "longitude": 37.1343,
    ///       "hasActiveAssignment": false,
    ///       "currentAssignmentId": null,
    ///       "currentAssigneeName": null
    ///     }
    ///   ],
    ///   "totalCount": 25,
    ///   "page": 1,
    ///   "pageSize": 100,
    ///   "totalPages": 1,
    ///   "polygonWkt": "POLYGON((37.13 36.20, 37.14 36.20, 37.14 36.21, 37.13 36.21, 37.13 36.20))",
    ///   "polygonAreaSquareMeters": 156000.5
    /// }
    /// ```
    /// 
    /// **Frontend Integration (Leaflet.js Example):**
    /// ```javascript
    /// map.on('draw:created', async function(e) {
    ///   const layer = e.layer;
    ///   const latlngs = layer.getLatLngs()[0];
    ///   // Convert to [lng, lat] format
    ///   const coordinates = latlngs.map(ll => [ll.lng, ll.lat]);
    ///   
    ///   const response = await fetch('/api/v1/buildingassignments/buildings/search', {
    ///     method: 'POST',
    ///     headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ...' },
    ///     body: JSON.stringify({ coordinates, hasActiveAssignment: false })
    ///   });
    /// });
    /// ```
    /// </remarks>
    /// <param name="request">Search parameters with polygon coordinates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of buildings within polygon</returns>
    /// <response code="200">Buildings retrieved successfully</response>
    /// <response code="400">Invalid polygon format or coordinates</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_View (4000)</response>
    [HttpPost("buildings/search")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(BuildingsForAssignmentPagedResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BuildingsForAssignmentPagedResult>> SearchBuildingsForAssignment(
        [FromBody] BuildingSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBuildingsForAssignmentQuery
        {
            GovernorateCode = request.GovernorateCode,
            DistrictCode = request.DistrictCode,
            SubDistrictCode = request.SubDistrictCode,
            CommunityCode = request.CommunityCode,
            NeighborhoodCode = request.NeighborhoodCode,
            BuildingCode = request.BuildingCode,
            Address = request.Address,
            BuildingType = request.BuildingType,
            BuildingStatus = request.BuildingStatus,
            HasActiveAssignment = request.HasActiveAssignment,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            RadiusMeters = request.RadiusMeters,
            PolygonWkt = request.PolygonWkt,
            Coordinates = request.Coordinates,
            Page = request.Page,
            PageSize = Math.Min(request.PageSize, 1000), // Cap at 1000 for polygon
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // ==================== PROPERTY UNITS FOR REVISIT ====================

    /// <summary>
    /// Get property units for revisit selection
    /// عرض الوحدات العقارية لاختيار إعادة الزيارة
    /// </summary>
    /// <remarks>
    /// Returns property units within a building for selecting specific units to revisit.
    /// UC-012: S04-S05 - Review property units and select for revisit
    /// 
    /// **Use Case**: UC-012 Assign Buildings - Revisit Selection
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **When to Use:**
    /// Use this endpoint when creating a revisit assignment to select specific
    /// property units that need to be re-surveyed.
    /// 
    /// **Example Request:**
    /// ```
    /// GET /api/v1/buildingassignments/buildings/3fa85f64-5717-4562-b3fc-2c963f66afa6/property-units?onlyWithCompletedSurveys=true
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "unit-guid-here",
    ///     "unitCode": "A-101",
    ///     "unitType": "Apartment",
    ///     "floorNumber": 1,
    ///     "description": "شقة سكنية",
    ///     "hasCompletedSurvey": true,
    ///     "lastSurveyDate": "2026-01-15T10:00:00Z",
    ///     "personCount": 4,
    ///     "householdCount": 1,
    ///     "claimCount": 1
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="buildingId">Building GUID</param>
    /// <param name="onlyWithCompletedSurveys">Filter to show only units with completed surveys</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of property units with survey status</returns>
    /// <response code="200">Property units retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_View (4000)</response>
    /// <response code="404">Building not found</response>
    [HttpGet("buildings/{buildingId:guid}/property-units")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(List<PropertyUnitForRevisitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PropertyUnitForRevisitDto>>> GetPropertyUnitsForRevisit(
        Guid buildingId,
        [FromQuery] bool onlyWithCompletedSurveys = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPropertyUnitsForRevisitQuery
        {
            BuildingId = buildingId,
            OnlyWithCompletedSurveys = onlyWithCompletedSurveys
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // ==================== FIELD COLLECTORS ====================

    /// <summary>
    /// Get available field collectors for assignment
    /// عرض جامعي البيانات المتاحين للتعيين
    /// </summary>
    /// <remarks>
    /// Returns field collectors with their current workload information.
    /// UC-012: Select field collector for assignment
    /// 
    /// **Use Case**: UC-012 Assign Buildings - Field Collector Selection
    /// 
    /// **Required Permission**: Buildings_Assign (4003) - CanAssignBuildings policy
    /// 
    /// **Filters:**
    /// - isAvailable: حالة التوفر (null=all, true=available, false=unavailable)
    /// - teamName: اسم الفريق
    /// - searchTerm: Search by name or tablet ID
    /// - hasAssignedTablet: Has a tablet assigned
    /// 
    /// **Sorting:**
    /// By default, collectors are sorted by workload ascending (least busy first).
    /// 
    /// **Example Request:**
    /// ```
    /// GET /api/v1/buildingassignments/field-collectors?isAvailable=true&amp;hasAssignedTablet=true&amp;sortByWorkloadAscending=true
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "collector-guid",
    ///     "username": "fcollector01",
    ///     "fullNameArabic": "أحمد محمد",
    ///     "fullNameEnglish": "Ahmad Mohammad",
    ///     "assignedTabletId": "TAB-001",
    ///     "teamName": "فريق حلب 1",
    ///     "isAvailable": true,
    ///     "activeAssignments": 5,
    ///     "pendingTransferCount": 2,
    ///     "totalPropertyUnitsAssigned": 45
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="isAvailable">Filter by availability status (حالة التوفر)</param>
    /// <param name="teamName">Filter by team name (اسم الفريق)</param>
    /// <param name="searchTerm">Search by name or tablet ID</param>
    /// <param name="hasAssignedTablet">Filter by tablet assignment</param>
    /// <param name="sortByWorkloadAscending">Sort by workload (default: true = least busy first)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of field collectors with workload info</returns>
    /// <response code="200">Field collectors retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_Assign (4003)</response>
    [HttpGet("field-collectors")]
    [Authorize(Policy = "CanAssignBuildings")]
    [ProducesResponseType(typeof(List<AvailableFieldCollectorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AvailableFieldCollectorDto>>> GetAvailableFieldCollectors(
        [FromQuery] bool? isAvailable,
        [FromQuery] string? teamName,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? hasAssignedTablet,
        [FromQuery] bool sortByWorkloadAscending = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableFieldCollectorsQuery
        {
            IsAvailable = isAvailable,
            TeamName = teamName,
            SearchTerm = searchTerm,
            HasAssignedTablet = hasAssignedTablet,
            SortByWorkloadAscending = sortByWorkloadAscending
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get assignments for a field collector
    /// عرض المباني المعينة لجامع بيانات
    /// </summary>
    /// <remarks>
    /// Returns all assignments for a specific field collector with summary statistics.
    /// UC-012: View collector's current tasks
    /// 
    /// **Use Case**: UC-012 Assign Buildings - View Collector Tasks
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Filters:**
    /// - isActive: Only active assignments (default: null = all)
    /// - transferStatus: Filter by specific transfer status
    /// - isRevisit: Filter revisit assignments only
    /// 
    /// **Example Request:**
    /// ```
    /// GET /api/v1/buildingassignments/field-collectors/collector-guid/assignments?isActive=true
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "fieldCollectorId": "collector-guid",
    ///   "fieldCollectorName": "أحمد محمد",
    ///   "totalAssignments": 10,
    ///   "pendingTransfer": 3,
    ///   "readyForSurvey": 5,
    ///   "inProgress": 1,
    ///   "completed": 1,
    ///   "assignments": [
    ///     {
    ///       "id": "assignment-guid",
    ///       "buildingId": "building-guid",
    ///       "buildingCode": "01010100300200001",
    ///       "transferStatus": "Transferred",
    ///       "transferStatusName": "تم النقل",
    ///       "isActive": true,
    ///       "isOverdue": false
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="fieldCollectorId">Field collector GUID</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="transferStatus">Filter by transfer status</param>
    /// <param name="isRevisit">Filter revisit assignments</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Field collector's assignments with summary</returns>
    /// <response code="200">Assignments retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_View (4000)</response>
    /// <response code="404">Field collector not found</response>
    [HttpGet("field-collectors/{fieldCollectorId:guid}/assignments")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(FieldCollectorTasksDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FieldCollectorTasksDto>> GetFieldCollectorAssignments(
        Guid fieldCollectorId,
        [FromQuery] bool? isActive,
        [FromQuery] TransferStatus? transferStatus,
        [FromQuery] bool? isRevisit,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFieldCollectorAssignmentsQuery
        {
            FieldCollectorId = fieldCollectorId,
            IsActive = isActive,
            TransferStatus = transferStatus,
            IsRevisit = isRevisit
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // ==================== GET ASSIGNMENT BY ID ====================

    /// <summary>
    /// Get assignment by ID
    /// عرض تفاصيل التعيين
    /// </summary>
    /// <remarks>
    /// Returns complete details for a specific building assignment.
    /// 
    /// **Use Case**: UC-012 - View Assignment Details
    /// 
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    /// 
    /// **Example Request:**
    /// ```
    /// GET /api/v1/buildingassignments/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "assignment-guid",
    ///   "buildingId": "building-guid",
    ///   "buildingCode": "01010100300200001",
    ///   "buildingAddress": "شارع الجمهورية",
    ///   "fieldCollectorId": "collector-guid",
    ///   "fieldCollectorName": "أحمد محمد",
    ///   "assignedByUserName": "مدير البيانات",
    ///   "assignedDate": "2026-01-30T10:00:00Z",
    ///   "targetCompletionDate": "2026-02-15T00:00:00Z",
    ///   "transferStatus": "Transferred",
    ///   "transferStatusName": "تم النقل",
    ///   "totalPropertyUnits": 10,
    ///   "completedPropertyUnits": 3,
    ///   "completionPercentage": 30.0,
    ///   "priority": "Normal",
    ///   "isRevisit": false,
    ///   "isActive": true,
    ///   "isOverdue": false
    /// }
    /// ```
    /// </remarks>
    /// <param name="assignmentId">Assignment GUID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assignment details</returns>
    /// <response code="200">Assignment found</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_View (4000)</response>
    /// <response code="404">Assignment not found</response>
    [HttpGet("{assignmentId:guid}")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(BuildingAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingAssignmentDto>> GetAssignmentById(
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAssignmentByIdQuery { AssignmentId = assignmentId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { error = $"Assignment with ID {assignmentId} not found" });

        return Ok(result);
    }

    // ==================== ASSIGN BUILDINGS ====================

    /// <summary>
    /// Assign buildings to a field collector
    /// تعيين مباني لجامع بيانات ميداني
    /// </summary>
    /// <remarks>
    /// Assigns one or more buildings to a field collector for survey.
    /// UC-012: S06-S07 - Assign selected buildings
    /// 
    /// **Use Case**: UC-012 Assign Buildings - Create Assignment
    /// 
    /// **Required Permission**: Buildings_Assign (4003) - CanAssignBuildings policy
    /// 
    /// **Supports:**
    /// - Bulk assignment of multiple buildings
    /// - Regular assignments (whole building)
    /// - Revisit assignments (specific property units)
    /// - Priority setting (Normal, High, Urgent)
    /// - Assignment notes/instructions
    /// 
    /// **Regular Assignment Example:**
    /// ```json
    /// {
    ///   "fieldCollectorId": "collector-guid",
    ///   "buildings": [
    ///     { "buildingId": "building-guid-1" },
    ///     { "buildingId": "building-guid-2" }
    ///   ],
    ///   "targetCompletionDate": "2026-02-15T00:00:00Z",
    ///   "priority": "Normal",
    ///   "assignmentNotes": "مباني منطقة العزيزية"
    /// }
    /// ```
    /// 
    /// **Revisit Assignment Example:**
    /// ```json
    /// {
    ///   "fieldCollectorId": "collector-guid",
    ///   "buildings": [
    ///     {
    ///       "buildingId": "building-guid",
    ///       "propertyUnitIdsForRevisit": ["unit-guid-1", "unit-guid-2"],
    ///       "revisitReason": "بيانات ناقصة - إعادة جمع معلومات الأسرة"
    ///     }
    ///   ],
    ///   "priority": "High"
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Successfully assigned 2 building(s) to أحمد محمد",
    ///   "assignedCount": 2,
    ///   "failedCount": 0,
    ///   "createdAssignmentIds": ["assignment-guid-1", "assignment-guid-2"],
    ///   "assignments": [...],
    ///   "errors": []
    /// }
    /// ```
    /// 
    /// **Note:** Transfer to tablet happens during synchronization (separate use case),
    /// not immediately after assignment. Initial status is "Pending".
    /// </remarks>
    /// <param name="command">Assignment details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assignment result with created assignment IDs</returns>
    /// <response code="200">Buildings assigned successfully (may include partial success)</response>
    /// <response code="400">Invalid request or all assignments failed</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_Assign (4003)</response>
    /// <response code="404">Field collector or building not found</response>
    [HttpPost("assign")]
    [Authorize(Policy = "CanAssignBuildings")]
    [ProducesResponseType(typeof(AssignBuildingsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AssignBuildingsResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignBuildingsResult>> AssignBuildings(
        [FromBody] AssignBuildingsCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "User assigning {BuildingCount} buildings to field collector {FieldCollectorId}",
            command.Buildings.Count, command.FieldCollectorId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success && result.AssignedCount == 0)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    // ==================== UNASSIGN BUILDING ====================

    /// <summary>
    /// Cancel/unassign a building assignment
    /// إلغاء تعيين مبنى
    /// </summary>
    /// <remarks>
    /// Cancels an active building assignment. 
    /// Cannot cancel assignments that have already been synchronized (data collected).
    /// 
    /// **Use Case**: UC-012 Assign Buildings - Cancel Assignment
    /// 
    /// **Required Permission**: Buildings_Assign (4003) - CanAssignBuildings policy
    /// 
    /// **Restrictions:**
    /// - Cannot cancel if TransferStatus = Synchronized (data already collected)
    /// - Cannot cancel already inactive assignments
    /// - Cancellation reason is required
    /// 
    /// **What happens:**
    /// - Sets IsActive = false
    /// - Sets TransferStatus = Cancelled
    /// - Records cancellation reason in AssignmentNotes
    /// - Audit log created
    /// 
    /// **Example Request:**
    /// ```
    /// POST /api/v1/buildingassignments/3fa85f64-5717-4562-b3fc-2c963f66afa6/unassign
    /// Content-Type: application/json
    /// 
    /// {
    ///   "cancellationReason": "تم نقل جامع البيانات إلى منطقة أخرى"
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Successfully cancelled assignment for building 01010100300200001",
    ///   "assignmentId": "assignment-guid",
    ///   "buildingCode": "01010100300200001",
    ///   "fieldCollectorName": "أحمد محمد"
    /// }
    /// ```
    /// </remarks>
    /// <param name="assignmentId">Assignment GUID to cancel</param>
    /// <param name="request">Cancellation details including reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Unassignment result</returns>
    /// <response code="200">Assignment cancelled successfully</response>
    /// <response code="400">Cannot cancel - already synchronized or inactive</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission - requires Buildings_Assign (4003)</response>
    /// <response code="404">Assignment not found</response>
    [HttpPost("{assignmentId:guid}/unassign")]
    [Authorize(Policy = "CanAssignBuildings")]
    [ProducesResponseType(typeof(UnassignBuildingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UnassignBuildingResult>> UnassignBuilding(
        Guid assignmentId,
        [FromBody] UnassignBuildingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CancellationReason))
        {
            return BadRequest(new { error = "Cancellation reason is required (سبب الإلغاء مطلوب)" });
        }

        var command = new UnassignBuildingCommand
        {
            AssignmentId = assignmentId,
            CancellationReason = request.CancellationReason
        };

        _logger.LogInformation(
            "User cancelling assignment {AssignmentId}. Reason: {Reason}",
            assignmentId, request.CancellationReason);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

// ==================== REQUEST MODELS ====================

/// <summary>
/// Request body for building polygon search
/// طلب البحث عن المباني داخل مضلع
/// </summary>
public class BuildingSearchRequest
{
    // ==================== ADMINISTRATIVE HIERARCHY FILTERS ====================

    /// <summary>
    /// Filter by governorate code (محافظة)
    /// </summary>
    /// <example>01</example>
    public string? GovernorateCode { get; set; }

    /// <summary>
    /// Filter by district code (مدينة)
    /// </summary>
    /// <example>01</example>
    public string? DistrictCode { get; set; }

    /// <summary>
    /// Filter by sub-district code (بلدة)
    /// </summary>
    /// <example>01</example>
    public string? SubDistrictCode { get; set; }

    /// <summary>
    /// Filter by community code (قرية)
    /// </summary>
    /// <example>001</example>
    public string? CommunityCode { get; set; }

    /// <summary>
    /// Filter by neighborhood code (حي)
    /// </summary>
    /// <example>001</example>
    public string? NeighborhoodCode { get; set; }

    // ==================== BUILDING FILTERS ====================

    /// <summary>
    /// Search by building code - partial match (رمز البناء)
    /// </summary>
    /// <example>0101010030</example>
    public string? BuildingCode { get; set; }

    /// <summary>
    /// Search by address - partial match (العنوان)
    /// </summary>
    /// <example>شارع الجمهورية</example>
    public string? Address { get; set; }

    /// <summary>
    /// Filter by building type (نوع البناء)
    /// 1=Residential, 2=Commercial, 3=MixedUse, 4=Industrial
    /// </summary>
    public BuildingType? BuildingType { get; set; }

    /// <summary>
    /// Filter by building status (حالة البناء)
    /// 1=Existing, 2=UnderConstruction, 3=Damaged, 4=Destroyed, 5=Demolished
    /// </summary>
    public BuildingStatus? BuildingStatus { get; set; }

    // ==================== ASSIGNMENT FILTER ====================

    /// <summary>
    /// Filter by assignment status (حالة التعيين)
    /// null=all, true=assigned only, false=unassigned only
    /// </summary>
    public bool? HasActiveAssignment { get; set; }

    // ==================== SPATIAL FILTER: RADIUS ====================

    /// <summary>
    /// Center latitude for radius search
    /// </summary>
    /// <example>36.2021</example>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Center longitude for radius search
    /// </summary>
    /// <example>37.1343</example>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Radius in meters for radius search
    /// </summary>
    /// <example>1000</example>
    public int? RadiusMeters { get; set; }

    // ==================== SPATIAL FILTER: POLYGON ====================

    /// <summary>
    /// Polygon in WKT format for polygon search (البحث بالمضلع)
    /// Coordinates are in longitude-latitude order. First and last coordinate must be identical.
    /// </summary>
    /// <example>POLYGON((37.13 36.20, 37.14 36.20, 37.14 36.21, 37.13 36.21, 37.13 36.20))</example>
    public string? PolygonWkt { get; set; }

    /// <summary>
    /// Polygon as coordinate array (easier for frontend)
    /// Format: [[lng1, lat1], [lng2, lat2], ...]
    /// Auto-closed if first != last coordinate
    /// </summary>
    /// <example>[[37.13, 36.20], [37.14, 36.20], [37.14, 36.21], [37.13, 36.21]]</example>
    public double[][]? Coordinates { get; set; }

    // ==================== PAGINATION ====================

    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    /// <example>1</example>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Items per page (default: 20, max: 1000 for polygon search)
    /// </summary>
    /// <example>20</example>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort field (e.g., "buildingCode", "address", "createdDate")
    /// </summary>
    /// <example>buildingCode</example>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (default: false = ascending)
    /// </summary>
    /// <example>false</example>
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// Request body for unassign operation
/// طلب إلغاء التعيين
/// </summary>
public class UnassignBuildingRequest
{
    /// <summary>
    /// Reason for cancellation (required)
    /// سبب الإلغاء (مطلوب)
    /// </summary>
    /// <example>تم نقل جامع البيانات إلى منطقة أخرى</example>
    public string CancellationReason { get; set; } = string.Empty;
}