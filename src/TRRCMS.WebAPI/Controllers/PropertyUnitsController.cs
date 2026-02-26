using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.PropertyUnits.Commands.CreatePropertyUnit;
using TRRCMS.Application.PropertyUnits.Commands.DeletePropertyUnit;
using TRRCMS.Application.PropertyUnits.Commands.UpdatePropertyUnit;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Application.PropertyUnits.Queries.GetAllPropertyUnits;
using TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnit;
using TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnitsByBuilding;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Property Units management API
/// </summary>
/// <remarks>
/// Manages individual property units (apartments, shops, offices) within buildings.
/// وحدات العقار - إدارة المقاسم
/// 
/// **What is a Property Unit?**
/// A Property Unit is an individual occupiable space within a building, such as:
/// - Residential apartment (شقة سكنية)
/// - Commercial shop (محل تجاري)
/// - Office space (مكتب)
/// - Warehouse (مستودع)
/// 
/// **Hierarchy:**
/// ```
/// Building (بناء)
///   └── Property Unit (وحدة/مقسم)
///         ├── Household (أسرة)
///         └── PersonPropertyRelation (علاقة الشخص بالعقار)
/// ```
/// 
/// **Unit Identifier Format:**
/// - Simple: "1", "2", "3" (apartment numbers)
/// - Floor-based: "1A", "1B", "2A" (floor + unit letter)
/// - Descriptive: "Ground-Left", "Basement-1" (position-based)
/// 
/// **PropertyUnitType Values (نوع الوحدة):**
/// 
/// | Value | Name | Arabic | Description |
/// |-------|------|--------|-------------|
/// | 1 | Apartment | شقة سكنية | Residential apartment |
/// | 2 | Shop | محل تجاري | Commercial retail space |
/// | 3 | Office | مكتب | Office space |
/// | 4 | Warehouse | مستودع | Storage/warehouse |
/// | 5 | Other | أخرى | Other unit type |
/// 
/// **PropertyUnitStatus Values (حالة الوحدة):**
/// 
/// | Value | Name | Arabic | Description |
/// |-------|------|--------|-------------|
/// | 1 | Occupied | مشغول | Currently occupied |
/// | 2 | Vacant | شاغر | Empty/unoccupied |
/// | 3 | Damaged | متضرر | Damaged but repairable |
/// | 4 | UnderRenovation | قيد الترميم | Being renovated |
/// | 5 | Uninhabitable | غير صالح للسكن | Cannot be occupied |
/// | 6 | Locked | مغلق | Locked/sealed by authorities |
/// | 99 | Unknown | غير معروف | Status unknown |
/// 
/// **Permissions:**
/// - View: PropertyUnits_View (6000)
/// - Create: PropertyUnits_Create (6001)
/// - Update: PropertyUnits_Update (6002)
/// 
/// **Alternative Endpoints:**
/// For survey context:
/// - `POST /api/v1/Surveys/{surveyId}/property-units` - Create during survey
/// - `GET /api/v1/Buildings/{id}/property-units` - Units by building
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class PropertyUnitsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertyUnitsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ==================== CREATE ====================

    /// <summary>
    /// Create a new property unit
    /// </summary>
    /// <remarks>
    /// Creates a new property unit within a building.
    /// إضافة وحدة عقارية جديدة
    /// 
    /// **Use Case**: UC-001 Field Survey - Register property units in a building
    /// 
    /// **Required Permission**: PropertyUnits_Create (6001) - CanCreatePropertyUnits policy
    /// 
    /// **Required Fields:**
    /// - `buildingId`: Parent building (must exist)
    /// - `unitIdentifier`: Unique identifier within building (max 50 chars)
    /// - `unitType`: Unit type (1-5)
    /// - `status`: Current status (1-6 or 99)
    /// 
    /// **Optional Fields:**
    /// - `floorNumber`: Floor number (-5 to 200, 0 = Ground, -1 = Basement)
    /// - `areaSquareMeters`: Unit area (must be > 0)
    /// - `numberOfRooms`: Room count (0-100)
    /// - `description`: Additional notes (max 2000 chars)
    /// 
    /// **Validation Rules:**
    /// - `unitIdentifier` must be unique within the building
    /// - `unitType` must be 1-5
    /// - `status` must be 1-6 or 99
    /// - `floorNumber` must be between -5 and 200
    /// - `areaSquareMeters` must be greater than 0
    /// - `numberOfRooms` must be between 0 and 100
    /// 
    /// **Example Request - Apartment:**
    /// ```json
    /// {
    ///   "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "unitIdentifier": "1A",
    ///   "floorNumber": 1,
    ///   "unitType": 1,
    ///   "status": 1,
    ///   "areaSquareMeters": 85.5,
    ///   "numberOfRooms": 3,
    ///   "description": "شقة سكنية بإطلالة على الحديقة"
    /// }
    /// ```
    /// 
    /// **Example Request - Shop:**
    /// ```json
    /// {
    ///   "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "unitIdentifier": "G-1",
    ///   "floorNumber": 0,
    ///   "unitType": 2,
    ///   "status": 1,
    ///   "areaSquareMeters": 45.0,
    ///   "description": "محل تجاري في الطابق الأرضي"
    /// }
    /// ```
    /// 
    /// **Example Request - Damaged Unit:**
    /// ```json
    /// {
    ///   "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "unitIdentifier": "2B",
    ///   "floorNumber": 2,
    ///   "unitType": 1,
    ///   "status": 3,
    ///   "areaSquareMeters": 90.0,
    ///   "numberOfRooms": 4,
    ///   "description": "شقة متضررة بسبب القصف - تحتاج إلى ترميم"
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "buildingNumber": "00001",
    ///   "unitIdentifier": "1A",
    ///   "floorNumber": 1,
    ///   "unitType": 1,
    ///   "status": 1,
    ///   "areaSquareMeters": 85.5,
    ///   "numberOfRooms": 3,
    ///   "description": "شقة سكنية بإطلالة على الحديقة",
    ///   "createdAtUtc": "2026-01-31T12:00:00Z",
    ///   "lastModifiedAtUtc": null
    /// }
    /// ```
    ///
    /// **Note:** Both requests and responses use integer values for `unitType` and `status`.
    /// See enum tables above for mappings.
    /// </remarks>
    /// <param name="command">Property unit creation data</param>
    /// <returns>Created property unit with generated ID</returns>
    /// <response code="201">Property unit created successfully</response>
    /// <response code="400">Validation error - check required fields and value ranges</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires PropertyUnits_Create (6001) permission</response>
    /// <response code="404">Building not found</response>
    /// <response code="409">Unit identifier already exists in this building</response>
    [HttpPost]
    [Authorize(Policy = "CanCreatePropertyUnits")]
    [ProducesResponseType(typeof(PropertyUnitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropertyUnitDto>> CreatePropertyUnit([FromBody] CreatePropertyUnitCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPropertyUnit), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // ==================== UPDATE ====================

    /// <summary>
    /// Update an existing property unit
    /// </summary>
    /// <remarks>
    /// Updates property unit details. Only provided fields will be updated (partial update).
    /// تعديل بيانات الوحدة العقارية
    /// 
    /// **Use Case**: Update unit status, correct data, add details
    /// 
    /// **Required Permission**: PropertyUnits_Update (6002) - CanUpdatePropertyUnits policy
    /// 
    /// **Updatable Fields:**
    /// - `floorNumber` (رقم الطابق)
    /// - `unitType` (نوع الوحدة) - rarely changed after creation
    /// - `status` (حالة الوحدة) - most common update
    /// - `areaSquareMeters` (مساحة القسم)
    /// - `numberOfRooms` (عدد الغرف)
    /// - `description` (وصف مفصل)
    /// 
    /// **Cannot Change:**
    /// - `buildingId` - Unit cannot be moved to another building
    /// - `unitIdentifier` - Identifier is permanent
    /// 
    /// **Example Request - Update status:**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "status": 2,
    ///   "description": "الساكن انتقل - الوحدة شاغرة الآن"
    /// }
    /// ```
    /// 
    /// **Example Request - After renovation:**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "status": 1,
    ///   "numberOfRooms": 4,
    ///   "areaSquareMeters": 95.0,
    ///   "description": "تم إضافة غرفة جديدة بعد الترميم"
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "buildingNumber": "00001",
    ///   "unitIdentifier": "1A",
    ///   "floorNumber": 1,
    ///   "unitType": 1,
    ///   "status": 1,
    ///   "areaSquareMeters": 95.0,
    ///   "numberOfRooms": 4,
    ///   "description": "تم إضافة غرفة جديدة بعد الترميم",
    ///   "createdAtUtc": "2026-01-29T12:00:00Z",
    ///   "lastModifiedAtUtc": "2026-01-31T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Property unit ID to update (must match ID in body)</param>
    /// <param name="command">Property unit update data (only include fields to change)</param>
    /// <returns>Updated property unit details</returns>
    /// <response code="200">Property unit updated successfully</response>
    /// <response code="400">Validation error or ID mismatch between URL and body</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires PropertyUnits_Update (6002) permission</response>
    /// <response code="404">Property unit not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanCreatePropertyUnits")]
    [ProducesResponseType(typeof(PropertyUnitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyUnitDto>> UpdatePropertyUnit(Guid id, [FromBody] UpdatePropertyUnitCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "ID in URL does not match ID in request body" });
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==================== GET BY ID ====================

    /// <summary>
    /// Get property unit by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about a specific property unit.
    /// عرض تفاصيل الوحدة العقارية
    /// 
    /// **Use Case**: View unit details, verify data
    /// 
    /// **Required Permission**: PropertyUnits_View (6000) - CanViewPropertyUnits policy
    /// 
    /// **Response includes:**
    /// - Unit identification (ID, building, identifier)
    /// - Physical characteristics (floor, area, rooms)
    /// - Current status and type
    /// - Description and notes
    /// - Audit timestamps
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "buildingNumber": "00001",
    ///   "unitIdentifier": "1A",
    ///   "floorNumber": 1,
    ///   "unitType": 1,
    ///   "status": 1,
    ///   "areaSquareMeters": 85.5,
    ///   "numberOfRooms": 3,
    ///   "description": "شقة سكنية بإطلالة على الحديقة",
    ///   "createdAtUtc": "2026-01-29T12:00:00Z",
    ///   "lastModifiedAtUtc": "2026-01-29T14:30:00Z"
    /// }
    /// ```
    /// 
    /// **UnitType Values:** 1=Apartment, 2=Shop, 3=Office, 4=Warehouse, 5=Other
    /// 
    /// **Status Values:** 1=Occupied, 2=Vacant, 3=Damaged, 4=UnderRenovation, 5=Uninhabitable, 6=Locked, 99=Unknown
    /// </remarks>
    /// <param name="id">Property unit ID (GUID)</param>
    /// <returns>Property unit details</returns>
    /// <response code="200">Property unit found and returned</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires PropertyUnits_View (6000) permission</response>
    /// <response code="404">Property unit not found</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewPropertyUnits")]
    [ProducesResponseType(typeof(PropertyUnitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyUnitDto>> GetPropertyUnit(Guid id)
    {
        var query = new GetPropertyUnitQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = $"Property unit with ID {id} not found" });
        }

        return Ok(result);
    }

    // ==================== GET ALL ====================

    /// <summary>
    /// Get all property units with optional filtering and grouping
    /// </summary>
    /// <remarks>
    /// Retrieves property units with optional filtering by building, type, and status.
    /// Results are grouped by building by default.
    /// عرض جميع الوحدات العقارية مع خيارات التصفية والتجميع
    ///
    /// **Use Case**: Building inventory, reporting, administrative review, filtered dashboards
    ///
    /// **Required Permission**: PropertyUnits_View (6000) - CanViewPropertyUnits policy
    ///
    /// **NEW FEATURES:**
    /// - **Filter by Building**: Get units from specific building only
    /// - **Filter by Type**: Get only apartments, shops, offices, etc.
    /// - **Filter by Status**: Get only occupied, vacant, damaged units, etc.
    /// - **Grouping**: Results grouped by building with statistics (default)
    /// - **Flat List**: Option to get ungrouped flat list of all units
    ///
    /// **Filter Parameters (all optional, AND-combined):**
    /// - `buildingId`: Filter by specific building (GUID)
    /// - `unitType`: 1=Apartment, 2=Shop, 3=Office, 4=Warehouse, 5=Other
    /// - `status`: 1=Occupied, 2=Vacant, 3=Damaged, 4=UnderRenovation, 5=Uninhabitable, 6=Locked, 99=Unknown
    /// - `groupByBuilding`: true (default) = grouped, false = flat list
    ///
    /// **Query Examples:**
    /// ```
    /// # All units grouped by building (default)
    /// GET /api/v1/PropertyUnits
    ///
    /// # All apartments across all buildings
    /// GET /api/v1/PropertyUnits?unitType=1
    ///
    /// # All vacant units
    /// GET /api/v1/PropertyUnits?status=2
    ///
    /// # All shops in specific building
    /// GET /api/v1/PropertyUnits?buildingId={guid}&amp;unitType=2
    ///
    /// # All damaged units (ungrouped flat list)
    /// GET /api/v1/PropertyUnits?status=3&amp;groupByBuilding=false
    ///
    /// # All occupied apartments in specific building
    /// GET /api/v1/PropertyUnits?buildingId={guid}&amp;unitType=1&amp;status=1
    /// ```
    ///
    /// **Example Response (Grouped - Default):**
    /// ```json
    /// {
    ///   "groupedByBuilding": [
    ///     {
    ///       "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "buildingNumber": "01-02-03-001-002-00001",
    ///       "unitCount": 3,
    ///       "propertyUnits": [
    ///         {
    ///           "id": "unit-guid-1",
    ///           "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///           "buildingNumber": "01-02-03-001-002-00001",
    ///           "unitIdentifier": "1A",
    ///           "floorNumber": 1,
    ///           "unitType": 1,
    ///           "status": 1,
    ///           "areaSquareMeters": 85.5,
    ///           "numberOfRooms": 3,
    ///           "description": "شقة سكنية",
    ///           "createdAtUtc": "2026-01-29T12:00:00Z"
    ///         },
    ///         {
    ///           "id": "unit-guid-2",
    ///           "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///           "buildingNumber": "01-02-03-001-002-00001",
    ///           "unitIdentifier": "2A",
    ///           "floorNumber": 2,
    ///           "unitType": 1,
    ///           "status": 2,
    ///           "areaSquareMeters": 90.0,
    ///           "numberOfRooms": 3,
    ///           "description": "شقة شاغرة",
    ///           "createdAtUtc": "2026-01-29T12:00:00Z"
    ///         }
    ///       ]
    ///     },
    ///     {
    ///       "buildingId": "4gb96f75-6828-5c73-d4e4-487220g75d08",
    ///       "buildingNumber": "01-02-03-001-002-00002",
    ///       "unitCount": 2,
    ///       "propertyUnits": [
    ///         {
    ///           "id": "unit-guid-3",
    ///           "buildingId": "4gb96f75-6828-5c73-d4e4-487220g75d08",
    ///           "buildingNumber": "01-02-03-001-002-00002",
    ///           "unitIdentifier": "G-1",
    ///           "floorNumber": 0,
    ///           "unitType": 2,
    ///           "status": 1,
    ///           "areaSquareMeters": 45.0,
    ///           "description": "محل تجاري",
    ///           "createdAtUtc": "2026-01-29T12:00:00Z"
    ///         }
    ///       ]
    ///     }
    ///   ],
    ///   "totalUnits": 5,
    ///   "totalBuildings": 2
    /// }
    /// ```
    ///
    /// **Response Fields:**
    /// - `groupedByBuilding[]`: Array of buildings with their units
    ///   - `buildingId`: Building unique identifier
    ///   - `buildingNumber`: Building code (17-digit formatted)
    ///   - `unitCount`: Number of units in this building (matching filters)
    ///   - `propertyUnits[]`: Array of property units (ordered by floor, then identifier)
    /// - `totalUnits`: Total count of units across all buildings (matching filters)
    /// - `totalBuildings`: Total count of buildings containing units (matching filters)
    ///
    /// **Performance Notes:**
    /// - Single database query with filters applied at database level
    /// - Building data fetched in batch (no N+1 queries)
    /// - Results ordered by building number, floor, and unit identifier
    ///
    /// **Validation:**
    /// - Invalid `unitType` values (not 1-5) return 400 Bad Request
    /// - Invalid `status` values (not 1-6 or 99) return 400 Bad Request
    /// - Invalid `buildingId` GUID format returns 400 Bad Request
    /// </remarks>
    /// <param name="buildingId">Filter by building ID (optional)</param>
    /// <param name="unitType">Filter by unit type: Apartment=1, Shop=2, Office=3, Warehouse=4, Other=5 (optional)</param>
    /// <param name="status">Filter by status: Occupied=1, Vacant=2, Damaged=3, UnderRenovation=4, Uninhabitable=5, Locked=6, Unknown=99 (optional)</param>
    /// <param name="groupByBuilding">Group results by building (default: true)</param>
    /// <returns>Property units grouped by building with statistics</returns>
    /// <response code="200">Success - returns grouped property units response</response>
    /// <response code="400">Bad request - invalid filter values</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires PropertyUnits_View (6000) permission</response>
    [HttpGet]
    [Authorize(Policy = "CanViewPropertyUnits")]
    [ProducesResponseType(typeof(GroupedPropertyUnitsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GroupedPropertyUnitsResponseDto>> GetAllPropertyUnits(
        [FromQuery] Guid? buildingId,
        [FromQuery] int? unitType,
        [FromQuery] int? status,
        [FromQuery] bool groupByBuilding = true)
    {
        var query = new GetAllPropertyUnitsQuery
        {
            BuildingId = buildingId,
            UnitType = unitType,
            Status = status,
            GroupByBuilding = groupByBuilding
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ==================== GET BY BUILDING ====================

    /// <summary>
    /// Get all property units for a specific building
    /// </summary>
    /// <remarks>
    /// Retrieves all property units within a specific building.
    /// عرض وحدات البناء
    /// 
    /// **Use Case**: Building management, survey preparation, unit listing
    /// 
    /// **Required Permission**: PropertyUnits_View (6000) - CanViewPropertyUnits policy
    /// 
    /// **Common Uses:**
    /// - Display units when selecting a building in UI
    /// - Building management dashboard
    /// - Survey preparation - view existing units before survey
    /// - Generate building occupancy report
    /// 
    /// **Response**: Array of property units ordered by floor and identifier.
    /// 
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "unit-guid-1",
    ///     "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "buildingNumber": "00001",
    ///     "unitIdentifier": "G-1",
    ///     "floorNumber": 0,
    ///     "unitType": 2,
    ///     "status": 1,
    ///     "areaSquareMeters": 50.0,
    ///     "numberOfRooms": null,
    ///     "description": "محل بقالة"
    ///   },
    ///   {
    ///     "id": "unit-guid-2",
    ///     "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "buildingNumber": "00001",
    ///     "unitIdentifier": "1A",
    ///     "floorNumber": 1,
    ///     "unitType": 1,
    ///     "status": 1,
    ///     "areaSquareMeters": 85.5,
    ///     "numberOfRooms": 3,
    ///     "description": "شقة سكنية"
    ///   },
    ///   {
    ///     "id": "unit-guid-3",
    ///     "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "buildingNumber": "00001",
    ///     "unitIdentifier": "1B",
    ///     "floorNumber": 1,
    ///     "unitType": 1,
    ///     "status": 2,
    ///     "areaSquareMeters": 90.0,
    ///     "numberOfRooms": 3,
    ///     "description": null
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="buildingId">Building ID to get units for</param>
    /// <returns>List of property units in the building</returns>
    /// <response code="200">Success - returns array of property units (may be empty)</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires PropertyUnits_View (6000) permission</response>
    /// <response code="404">Building not found</response>
    [HttpGet("building/{buildingId}")]
    [Authorize(Policy = "CanViewPropertyUnits")]
    [ProducesResponseType(typeof(List<PropertyUnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PropertyUnitDto>>> GetPropertyUnitsByBuilding(Guid buildingId)
    {
        try
        {
            var query = new GetPropertyUnitsByBuildingQuery(buildingId);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ==================== DELETE ====================

    /// <summary>
    /// Soft delete a property unit and all related data
    /// حذف الوحدة العقارية مع جميع البيانات المرتبطة
    /// </summary>
    /// <remarks>
    /// **Use Case**: Remove a property unit that was added by mistake or is no longer relevant
    ///
    /// **Required Permission**: PropertyUnits_Create (6001) - CanCreatePropertyUnits policy
    ///
    /// **Cascade Delete Behavior**:
    /// This operation will soft delete:
    /// - The PropertyUnit itself
    /// - All Households in this unit
    /// - All Persons in those households
    /// - All PersonPropertyRelations for those persons and this unit
    /// - All Evidences linked to those relations and persons
    ///
    /// **Important**: Only works when the related survey is in **Draft** status.
    /// If the survey is Finalized or Completed, the delete will be rejected.
    ///
    /// **Example Request**:
    /// ```
    /// DELETE /api/v1/PropertyUnits/7e439aab-5dd1-4a8a-b6c4-265008e53b86
    /// ```
    ///
    /// **Example Response**:
    /// ```json
    /// {
    ///   "primaryEntityId": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "primaryEntityType": "PropertyUnit",
    ///   "affectedEntities": [
    ///     { "entityId": "7e439aab-...", "entityType": "PropertyUnit", "entityIdentifier": "1A" },
    ///     { "entityId": "aaa11111-...", "entityType": "Household", "entityIdentifier": "أحمد محمد" },
    ///     { "entityId": "bbb22222-...", "entityType": "Person", "entityIdentifier": "محمد أحمد الخالد" },
    ///     { "entityId": "ccc33333-...", "entityType": "PersonPropertyRelation", "entityIdentifier": "Relation Owner" },
    ///     { "entityId": "ddd44444-...", "entityType": "Evidence", "entityIdentifier": "national_id.pdf" }
    ///   ],
    ///   "totalAffected": 5,
    ///   "deletedAtUtc": "2026-02-14T10:00:00Z",
    ///   "message": "PropertyUnit deleted successfully along with 1 household(s), 1 person(s), 1 relation(s), and 1 evidence(s)"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Property unit ID (GUID) to delete</param>
    /// <returns>Delete result with all affected entity IDs</returns>
    /// <response code="200">Property unit and related data deleted successfully</response>
    /// <response code="400">Survey is not in Draft status or property unit is already deleted</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires PropertyUnits_Create (6001) permission</response>
    /// <response code="404">Property unit not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanCreatePropertyUnits")]
    [ProducesResponseType(typeof(DeleteResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResultDto>> DeletePropertyUnit(Guid id)
    {
        var command = new DeletePropertyUnitCommand { PropertyUnitId = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}