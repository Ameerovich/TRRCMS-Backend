using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Households.Commands.CreateHousehold;
using TRRCMS.Application.Households.Commands.UpdateHousehold;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.Households.Queries.GetAllHouseholds;
using TRRCMS.Application.Households.Queries.GetHousehold;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Household management API for demographic data collection
/// </summary>
/// <remarks>
/// Manages household records as part of the tenure rights survey process.
/// تسجيل الأسرة - إدارة بيانات الأسر
/// 
/// **What is a Household?**
/// A household represents a group of people living together in a property unit.
/// It captures demographic composition for humanitarian and planning purposes.
/// 
/// **Household vs Person:**
/// - **Household**: Group demographics (counts by gender, age, disability)
/// - **Person**: Individual identity and documents
/// - A household belongs to one PropertyUnit
/// - Individual persons can be registered separately and linked
/// 
/// **Demographic Breakdown (تكوين الأسرة):**
/// 
/// | Category | Male Field | Female Field | Age Range |
/// |----------|------------|--------------|-----------|
/// | Adults (البالغين) | maleCount | femaleCount | 18-64 years |
/// | Children (الأطفال) | maleChildCount | femaleChildCount | Under 18 |
/// | Elderly (كبار السن) | maleElderlyCount | femaleElderlyCount | 65+ years |
/// | Disabled (المعاقين) | maleDisabledCount | femaleDisabledCount | Any age |
/// 
/// **Note:** Disabled counts are additional flags, not mutually exclusive with age categories.
/// 
/// **Permissions:**
/// - This controller uses Survey permissions (no separate Household permissions)
/// - View: Surveys_ViewAll (7004)
/// - Edit: Surveys_EditAll (7006)
/// 
/// **For Field Collectors:**
/// Use the survey-specific endpoint instead:
/// `POST /api/v1/Surveys/{surveyId}/households`
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
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
    /// Create a new household
    /// </summary>
    /// <remarks>
    /// Creates a new household record within a property unit.
    /// تسجيل الأسرة - تسجيل تفاصيل الإشغال
    /// 
    /// **Use Case**: UC-001 Field Survey Stage 2 - Record household occupancy
    /// 
    /// **Required Permission**: Surveys_EditAll (7006) - CanEditAllSurveys policy
    /// 
    /// **Note**: For field collectors working within a survey context, use:
    /// `POST /api/v1/Surveys/{surveyId}/households` instead.
    /// 
    /// **Required Fields:**
    /// - `propertyUnitId`: The property unit this household occupies
    /// - `headOfHouseholdName`: Name of the head of household (رب الأسرة)
    /// - `householdSize`: Total number of people in household
    /// 
    /// **Optional Demographic Fields:**
    /// - `maleCount` / `femaleCount`: Adult counts (ages 18-64)
    /// - `maleChildCount` / `femaleChildCount`: Children under 18
    /// - `maleElderlyCount` / `femaleElderlyCount`: Elderly 65+
    /// - `maleDisabledCount` / `femaleDisabledCount`: Persons with disabilities
    /// 
    /// **Validation:**
    /// - Sum of demographic counts should equal `householdSize`
    /// - PropertyUnit must exist
    /// 
    /// **Example Request:**
    /// ```json
    /// {
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "headOfHouseholdName": "أحمد محمد علي الخالد",
    ///   "householdSize": 6,
    ///   "maleCount": 1,
    ///   "femaleCount": 1,
    ///   "maleChildCount": 2,
    ///   "femaleChildCount": 1,
    ///   "maleElderlyCount": 1,
    ///   "femaleElderlyCount": 0,
    ///   "maleDisabledCount": 0,
    ///   "femaleDisabledCount": 0,
    ///   "notes": "أسرة مكونة من الأب والأم وثلاثة أطفال والجد"
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "propertyUnitIdentifier": "Unit-101",
    ///   "headOfHouseholdName": "أحمد محمد علي الخالد",
    ///   "headOfHouseholdPersonId": null,
    ///   "householdSize": 6,
    ///   "maleCount": 1,
    ///   "femaleCount": 1,
    ///   "maleChildCount": 2,
    ///   "femaleChildCount": 1,
    ///   "maleElderlyCount": 1,
    ///   "femaleElderlyCount": 0,
    ///   "maleDisabledCount": 0,
    ///   "femaleDisabledCount": 0,
    ///   "notes": "أسرة مكونة من الأب والأم وثلاثة أطفال والجد",
    ///   "createdAtUtc": "2026-01-31T10:00:00Z",
    ///   "createdBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "lastModifiedAtUtc": null,
    ///   "lastModifiedBy": null,
    ///   "isDeleted": false
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Household creation data with demographics</param>
    /// <returns>Created household with generated ID</returns>
    /// <response code="201">Household created successfully</response>
    /// <response code="400">Validation error - check required fields and demographic totals</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Surveys_EditAll permission</response>
    /// <response code="404">Property unit not found</response>
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
    /// Update an existing household
    /// </summary>
    /// <remarks>
    /// Updates household details. Only provided fields will be updated (partial update supported).
    /// 
    /// **Use Case**: Correct demographic data, update after family changes
    /// 
    /// **Required Permission**: Surveys_EditAll (7006) - CanEditAllSurveys policy
    /// 
    /// **Updatable Fields (all optional):**
    /// - `headOfHouseholdName`: Change head of household
    /// - `householdSize`: Update total count
    /// - All demographic counts (male/female × adult/child/elderly/disabled)
    /// - `notes`: Update observations
    /// 
    /// **Note:** `propertyUnitId` cannot be changed after creation.
    /// To move a household, delete and recreate.
    /// 
    /// **Example Request - Update after new baby:**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "householdSize": 7,
    ///   "maleChildCount": 3,
    ///   "notes": "ولد طفل ذكر جديد - تم التحديث"
    /// }
    /// ```
    /// 
    /// **Example Request - Full update:**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "headOfHouseholdName": "محمد أحمد علي الخالد",
    ///   "householdSize": 5,
    ///   "maleCount": 2,
    ///   "femaleCount": 1,
    ///   "maleChildCount": 1,
    ///   "femaleChildCount": 1,
    ///   "maleElderlyCount": 0,
    ///   "femaleElderlyCount": 0,
    ///   "maleDisabledCount": 0,
    ///   "femaleDisabledCount": 0,
    ///   "notes": "انتقل الجد إلى سكن آخر، انضم أخ رب الأسرة"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Household ID to update (must match ID in body)</param>
    /// <param name="command">Household update data (only include fields to change)</param>
    /// <returns>Updated household details</returns>
    /// <response code="200">Household updated successfully</response>
    /// <response code="400">Validation error or ID mismatch between URL and body</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Surveys_EditAll permission</response>
    /// <response code="404">Household not found</response>
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
    /// Get household by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about a specific household including
    /// full demographic breakdown.
    /// 
    /// **Use Case**: View household details, verify demographic data
    /// 
    /// **Required Permission**: Surveys_ViewAll (7004) - CanViewAllSurveys policy
    /// 
    /// **Response includes:**
    /// - Household identification (ID, property unit link)
    /// - Head of household name and optional person link
    /// - Complete demographic breakdown by gender and age
    /// - Notes and full audit trail
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "propertyUnitIdentifier": "Unit-101",
    ///   "headOfHouseholdName": "أحمد محمد علي الخالد",
    ///   "headOfHouseholdPersonId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "householdSize": 6,
    ///   "maleCount": 1,
    ///   "femaleCount": 1,
    ///   "maleChildCount": 2,
    ///   "femaleChildCount": 1,
    ///   "maleElderlyCount": 1,
    ///   "femaleElderlyCount": 0,
    ///   "maleDisabledCount": 0,
    ///   "femaleDisabledCount": 0,
    ///   "notes": "أسرة مكونة من الأب والأم وثلاثة أطفال والجد",
    ///   "createdAtUtc": "2026-01-31T10:00:00Z",
    ///   "createdBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "lastModifiedAtUtc": "2026-01-31T14:30:00Z",
    ///   "lastModifiedBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "isDeleted": false,
    ///   "deletedAtUtc": null,
    ///   "deletedBy": null
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Household ID (GUID)</param>
    /// <returns>Household details with full demographics</returns>
    /// <response code="200">Household found and returned</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Surveys_ViewAll permission</response>
    /// <response code="404">Household not found</response>
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
    /// Get all households
    /// </summary>
    /// <remarks>
    /// Retrieves all households in the system.
    /// 
    /// **Use Case**: Reporting, data export, administrative review
    /// 
    /// **Required Permission**: Surveys_ViewAll (7004) - CanViewAllSurveys policy
    /// 
    /// **Note**: For large datasets, consider:
    /// - Using survey-specific endpoint: `GET /api/v1/Surveys/{surveyId}/households`
    /// - Using property unit endpoint: `GET /api/v1/PropertyUnits/{id}/households`
    /// - Implementing pagination (when available)
    /// 
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///     "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "propertyUnitIdentifier": "Unit-101",
    ///     "headOfHouseholdName": "أحمد محمد علي",
    ///     "householdSize": 6,
    ///     "maleCount": 1,
    ///     "femaleCount": 1,
    ///     "maleChildCount": 2,
    ///     "femaleChildCount": 1,
    ///     "maleElderlyCount": 1,
    ///     "femaleElderlyCount": 0,
    ///     "maleDisabledCount": 0,
    ///     "femaleDisabledCount": 0,
    ///     "createdAtUtc": "2026-01-31T10:00:00Z"
    ///   },
    ///   {
    ///     "id": "8f540bbc-6ee2-5b9b-c7d5-376119f64c97",
    ///     "propertyUnitId": "4gb96g75-6828-5673-c4gd-3d074g77bgb7",
    ///     "propertyUnitIdentifier": "Unit-102",
    ///     "headOfHouseholdName": "محمد خالد أحمد",
    ///     "householdSize": 4,
    ///     "maleCount": 1,
    ///     "femaleCount": 1,
    ///     "maleChildCount": 1,
    ///     "femaleChildCount": 1,
    ///     "maleElderlyCount": 0,
    ///     "femaleElderlyCount": 0,
    ///     "maleDisabledCount": 0,
    ///     "femaleDisabledCount": 0,
    ///     "createdAtUtc": "2026-01-31T11:30:00Z"
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <returns>List of all households</returns>
    /// <response code="200">Success - returns array of households (may be empty)</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Surveys_ViewAll permission</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllSurveys")]
    [ProducesResponseType(typeof(PagedResult<HouseholdDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<HouseholdDto>>> GetAllHouseholds([FromQuery] GetAllHouseholdsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}