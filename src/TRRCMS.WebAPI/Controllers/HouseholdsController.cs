using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Households.Commands.CreateHousehold;
using TRRCMS.Application.Households.Commands.UpdateHousehold;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.Households.Queries.GetAllHouseholds;
using TRRCMS.Application.Households.Queries.GetHousehold;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Households Management API (Admin/Data Manager access)
/// For direct household management outside survey context
/// تسجيل الأسرة - إدارة بيانات الأسر
/// 
/// NOTE: Uses Survey permissions - no separate Household permissions needed
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class HouseholdsController : ControllerBase
{
    private readonly IMediator _mediator;

    public HouseholdsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ==================== CREATE ====================

    /// <summary>
    /// Create a new household (Admin/Data Manager)
    /// </summary>
    /// <remarks>
    /// **Purpose**: Creates a new household within a property unit.
    /// تسجيل الأسرة - تسجيل تفاصيل الإشغال
    /// 
    /// **Required Permission**: Surveys_EditAll (CanEditAllSurveys)
    /// 
    /// **Note**: For field collectors, use POST /api/Surveys/{surveyId}/households instead.
    /// 
    /// **Example Request**:
    /// ```json
    /// {
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "headOfHouseholdName": "أحمد محمد علي",
    ///   "householdSize": 5,
    ///   "maleCount": 1,
    ///   "femaleCount": 1,
    ///   "maleChildCount": 2,
    ///   "femaleChildCount": 1,
    ///   "maleElderlyCount": 0,
    ///   "femaleElderlyCount": 0,
    ///   "maleDisabledCount": 0,
    ///   "femaleDisabledCount": 0,
    ///   "notes": "أسرة من خمسة أفراد"
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Household creation data</param>
    /// <returns>Created household with generated ID</returns>
    /// <response code="201">Household created successfully.</response>
    /// <response code="400">Validation error. Check required fields.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires Surveys_EditAll permission.</response>
    /// <response code="404">Property unit not found.</response>
    [HttpPost]
    [Authorize(Policy = "CanEditAllSurveys")]
    [ProducesResponseType(typeof(HouseholdDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdDto>> CreateHousehold([FromBody] CreateHouseholdCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetHousehold), new { id = result.Id }, result);
    }

    // ==================== UPDATE ====================

    /// <summary>
    /// Update an existing household (Admin/Data Manager)
    /// </summary>
    /// <remarks>
    /// **Purpose**: Updates household details. Only provided fields will be updated.
    /// 
    /// **Required Permission**: Surveys_EditAll (CanEditAllSurveys)
    /// 
    /// **Example Request** (partial update):
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "householdSize": 6,
    ///   "maleChildCount": 3,
    ///   "notes": "ولد طفل جديد"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Household ID to update</param>
    /// <param name="command">Household update data (all fields optional)</param>
    /// <returns>Updated household details</returns>
    /// <response code="200">Household updated successfully.</response>
    /// <response code="400">Validation error or ID mismatch.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires Surveys_EditAll permission.</response>
    /// <response code="404">Household not found.</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanEditAllSurveys")]
    [ProducesResponseType(typeof(HouseholdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdDto>> UpdateHousehold(Guid id, [FromBody] UpdateHouseholdCommand command)
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
    /// Get household by ID (Admin/Data Manager/Supervisor)
    /// </summary>
    /// <remarks>
    /// **Purpose**: Retrieves detailed information about a specific household.
    /// 
    /// **Required Permission**: Surveys_ViewAll (CanViewAllSurveys)
    /// 
    /// **Response includes**:
    /// - Household ID and property unit link
    /// - Head of household name
    /// - Full family composition by gender
    /// - Notes and audit timestamps
    /// </remarks>
    /// <param name="id">Household ID (GUID)</param>
    /// <returns>Household details</returns>
    /// <response code="200">Household found and returned.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires Surveys_ViewAll permission.</response>
    /// <response code="404">Household not found.</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewAllSurveys")]
    [ProducesResponseType(typeof(HouseholdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdDto>> GetHousehold(Guid id)
    {
        var query = new GetHouseholdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = $"Household with ID {id} not found" });
        }

        return Ok(result);
    }

    // ==================== GET ALL ====================

    /// <summary>
    /// Get all households (Admin/Data Manager/Supervisor)
    /// </summary>
    /// <remarks>
    /// **Purpose**: Retrieves all households in the system.
    /// 
    /// **Required Permission**: Surveys_ViewAll (CanViewAllSurveys)
    /// 
    /// **Note**: For large datasets, consider using filtered endpoints.
    /// </remarks>
    /// <returns>List of all households</returns>
    /// <response code="200">Success. Returns array of households (may be empty).</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires Surveys_ViewAll permission.</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllSurveys")]
    [ProducesResponseType(typeof(List<HouseholdDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<HouseholdDto>>> GetAllHouseholds()
    {
        var query = new GetAllHouseholdsQuery();
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}