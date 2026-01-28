using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.PropertyUnits.Commands.CreatePropertyUnit;
using TRRCMS.Application.PropertyUnits.Commands.UpdatePropertyUnit;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Application.PropertyUnits.Queries.GetAllPropertyUnits;
using TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnit;
using TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnitsByBuilding;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Property Units Management API
/// Provides CRUD operations for property units within buildings
/// </summary>
/// <remarks>
/// Property units represent individual units within a building (apartments, shops, offices, etc.)
/// 
/// **Key Concepts**:
/// - Each property unit belongs to exactly one building
/// - Property units can be created directly or through surveys
/// - Unit identifiers must be unique within a building
/// 
/// **Permissions Required**:
/// - View: PropertyUnits_View
/// - Create: PropertyUnits_Create
/// - Update: PropertyUnits_Update
/// </remarks>
[ApiController]
[Route("api/[controller]")]
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
    /// **Purpose**: Creates a new property unit within a building.
    /// 
    /// **Required Permission**: PropertyUnits_Create
    /// 
    /// **Validation Rules**:
    /// - BuildingId: Required, must exist
    /// - UnitIdentifier: Required, max 50 chars, unique within building
    /// - UnitType: Required, 1-5 (1=Apartment, 2=Shop, 3=Office, 4=Warehouse, 5=Other)
    /// - Status: Required, 1-6 or 99 (1=Occupied, 2=Vacant, 3=Damaged, 4=UnderRenovation, 5=Uninhabitable, 6=Locked, 99=Unknown)
    /// - FloorNumber: Optional, -5 to 200
    /// - AreaSquareMeters: Optional, must be greater than 0
    /// - NumberOfRooms: Optional, 0-100
    /// - Description: Optional, max 2000 chars
    /// 
    /// **Unit Types (نوع الوحدة)**:
    /// | Value | Name | Arabic |
    /// |-------|------|--------|
    /// | 1 | Apartment | شقة سكنية |
    /// | 2 | Shop | محل تجاري |
    /// | 3 | Office | مكتب |
    /// | 4 | Warehouse | مستودع |
    /// | 5 | Other | أخرى |
    /// 
    /// **Status Values (حالة الوحدة)**:
    /// | Value | Name | Arabic |
    /// |-------|------|--------|
    /// | 1 | Occupied | مشغول |
    /// | 2 | Vacant | شاغر |
    /// | 3 | Damaged | متضرر |
    /// | 4 | UnderRenovation | قيد الترميم |
    /// | 5 | Uninhabitable | غير صالح للسكن |
    /// | 6 | Locked | مغلق |
    /// | 99 | Unknown | غير معروف |
    /// 
    /// **Example Request**:
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
    /// **Example Response**:
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "buildingNumber": "00001",
    ///   "unitIdentifier": "1A",
    ///   "floorNumber": 1,
    ///   "unitType": "Apartment",
    ///   "status": "Occupied",
    ///   "areaSquareMeters": 85.5,
    ///   "numberOfRooms": 3,
    ///   "description": "شقة سكنية بإطلالة على الحديقة",
    ///   "createdAtUtc": "2026-01-29T12:00:00Z",
    ///   "lastModifiedAtUtc": null
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Property unit creation data</param>
    /// <returns>Created property unit with generated ID</returns>
    /// <response code="201">Property unit created successfully.</response>
    /// <response code="400">Validation error. Check required fields and value ranges.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires PropertyUnits_Create permission.</response>
    /// <response code="404">Building not found.</response>
    /// <response code="409">Property unit with same identifier already exists in building.</response>
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
    /// **Purpose**: Updates property unit details. Only provided fields will be updated.
    /// 
    /// **Required Permission**: PropertyUnits_Update
    /// 
    /// **Updateable Fields**:
    /// - FloorNumber (رقم الطابق)
    /// - UnitType (نوع الوحدة) - rarely changed after creation
    /// - Status (حالة الوحدة)
    /// - AreaSquareMeters (مساحة القسم)
    /// - NumberOfRooms (عدد الغرف)
    /// - Description (وصف مفصل)
    /// 
    /// **Note**: UnitIdentifier and BuildingId cannot be changed after creation.
    /// 
    /// **Example Request** (partial update):
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "status": 2,
    ///   "numberOfRooms": 4,
    ///   "description": "تم إضافة غرفة جديدة بعد التجديد"
    /// }
    /// ```
    /// 
    /// **Example Request** (full update):
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "floorNumber": 2,
    ///   "unitType": 1,
    ///   "status": 1,
    ///   "areaSquareMeters": 95.0,
    ///   "numberOfRooms": 4,
    ///   "description": "شقة سكنية مجددة"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Property unit ID to update</param>
    /// <param name="command">Property unit update data (all fields optional)</param>
    /// <returns>Updated property unit details</returns>
    /// <response code="200">Property unit updated successfully.</response>
    /// <response code="400">Validation error or ID mismatch.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires PropertyUnits_Update permission.</response>
    /// <response code="404">Property unit not found.</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanUpdatePropertyUnits")]
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

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ==================== GET BY ID ====================

    /// <summary>
    /// Get property unit by ID
    /// </summary>
    /// <remarks>
    /// **Purpose**: Retrieves detailed information about a specific property unit.
    /// 
    /// **Required Permission**: PropertyUnits_View
    /// 
    /// **Response includes**:
    /// - Unit identifier and building info
    /// - Physical characteristics (floor, area, rooms)
    /// - Current status
    /// - Description and notes
    /// - Audit timestamps
    /// 
    /// **Example Response**:
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "buildingNumber": "00001",
    ///   "unitIdentifier": "1A",
    ///   "floorNumber": 1,
    ///   "unitType": "Apartment",
    ///   "status": "Occupied",
    ///   "areaSquareMeters": 85.5,
    ///   "numberOfRooms": 3,
    ///   "description": "شقة سكنية بإطلالة على الحديقة",
    ///   "createdAtUtc": "2026-01-29T12:00:00Z",
    ///   "lastModifiedAtUtc": "2026-01-29T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Property unit ID (GUID)</param>
    /// <returns>Property unit details</returns>
    /// <response code="200">Property unit found and returned.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires PropertyUnits_View permission.</response>
    /// <response code="404">Property unit not found.</response>
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
    /// Get all property units
    /// </summary>
    /// <remarks>
    /// **Purpose**: Retrieves all property units in the system.
    /// 
    /// **Required Permission**: PropertyUnits_View
    /// 
    /// **Note**: For large datasets, consider using the building-specific endpoint instead.
    /// 
    /// **Response**: Array of property units ordered by building and unit identifier.
    /// 
    /// **Example Response**:
    /// ```json
    /// [
    ///   {
    ///     "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///     "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "buildingNumber": "00001",
    ///     "unitIdentifier": "1A",
    ///     "floorNumber": 1,
    ///     "unitType": "Apartment",
    ///     "status": "Occupied",
    ///     "areaSquareMeters": 85.5,
    ///     "numberOfRooms": 3,
    ///     "description": "شقة سكنية",
    ///     "createdAtUtc": "2026-01-29T12:00:00Z",
    ///     "lastModifiedAtUtc": null
    ///   },
    ///   {
    ///     "id": "8f550bbc-6ee2-5b9b-c7d5-376119f64c97",
    ///     "buildingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "buildingNumber": "00001",
    ///     "unitIdentifier": "G-1",
    ///     "floorNumber": 0,
    ///     "unitType": "Shop",
    ///     "status": "Vacant",
    ///     "areaSquareMeters": 45.0,
    ///     "numberOfRooms": null,
    ///     "description": "محل تجاري في الطابق الأرضي",
    ///     "createdAtUtc": "2026-01-29T12:00:00Z",
    ///     "lastModifiedAtUtc": null
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <returns>List of all property units</returns>
    /// <response code="200">Success. Returns array of property units (may be empty).</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires PropertyUnits_View permission.</response>
    [HttpGet]
    [Authorize(Policy = "CanViewPropertyUnits")]
    [ProducesResponseType(typeof(List<PropertyUnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<PropertyUnitDto>>> GetAllPropertyUnits()
    {
        var query = new GetAllPropertyUnitsQuery();
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    // ==================== GET BY BUILDING ====================

    /// <summary>
    /// Get all property units for a specific building
    /// </summary>
    /// <remarks>
    /// **Purpose**: Retrieves all property units within a specific building.
    /// 
    /// **Required Permission**: PropertyUnits_View
    /// 
    /// **Use Cases**:
    /// - Display units when selecting a building
    /// - Building management view
    /// - Survey preparation - see existing units
    /// 
    /// **Response**: Array of property units in the building, ordered by floor and identifier.
    /// 
    /// **Example Response**:
    /// ```json
    /// [
    ///   {
    ///     "id": "unit-guid-1",
    ///     "buildingId": "building-guid",
    ///     "buildingNumber": "00001",
    ///     "unitIdentifier": "G-1",
    ///     "floorNumber": 0,
    ///     "unitType": "Shop",
    ///     "status": "Occupied",
    ///     "areaSquareMeters": 50.0,
    ///     "numberOfRooms": null,
    ///     "description": "محل بقالة"
    ///   },
    ///   {
    ///     "id": "unit-guid-2",
    ///     "buildingId": "building-guid",
    ///     "buildingNumber": "00001",
    ///     "unitIdentifier": "1A",
    ///     "floorNumber": 1,
    ///     "unitType": "Apartment",
    ///     "status": "Occupied",
    ///     "areaSquareMeters": 85.5,
    ///     "numberOfRooms": 3,
    ///     "description": "شقة سكنية"
    ///   },
    ///   {
    ///     "id": "unit-guid-3",
    ///     "buildingId": "building-guid",
    ///     "buildingNumber": "00001",
    ///     "unitIdentifier": "1B",
    ///     "floorNumber": 1,
    ///     "unitType": "Apartment",
    ///     "status": "Vacant",
    ///     "areaSquareMeters": 90.0,
    ///     "numberOfRooms": 3,
    ///     "description": null
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="buildingId">Building ID to get units for</param>
    /// <returns>List of property units in the building</returns>
    /// <response code="200">Success. Returns array of property units (may be empty).</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires PropertyUnits_View permission.</response>
    /// <response code="404">Building not found.</response>
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
}