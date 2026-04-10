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
/// - **Household**: Group demographics (ungendered counts)
/// - **Person**: Individual identity and documents
/// - A household belongs to one PropertyUnit
/// - Individual persons can be registered separately and linked
///
/// **Demographic Composition (تكوين الأسرة) — canonical v1.9 shape:**
///
/// | Field | Description |
/// |-------|-------------|
/// | householdSize | **Required.** Total household members, 1–50 |
/// | maleCount | Total males, all ages |
/// | femaleCount | Total females, all ages |
/// | adultCount | Number of adults |
/// | childCount | Number of children |
/// | elderlyCount | Number of elderly |
/// | disabledCount | Number of persons with disabilities |
/// | occupancyNature | Occupancy nature enum code (LegalFormal/Informal/Customary/…) |
/// | occupancyStartDate | ISO-8601 UTC date the household started occupying this unit |
/// | notes | Free-text notes, ≤ 2000 chars |
///
/// All count fields except `householdSize` are **optional and nullable**.
/// Validation is **upper-bound only** — gaps are allowed:
/// - `maleCount + femaleCount ≤ householdSize`
/// - `adultCount + childCount + elderlyCount ≤ householdSize`
/// - `disabledCount ≤ householdSize`
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
    /// **Use Case**: Record household occupancy during field survey
    ///
    /// **Required Permission**: Surveys_EditAll (7006) - CanEditAllSurveys policy
    ///
    /// **Note**: For field collectors working within a survey context, use:
    /// `POST /api/v1/Surveys/{surveyId}/households` instead.
    ///
    /// **Required Fields:**
    /// - `propertyUnitId`: The property unit this household occupies
    /// - `householdSize`: Total number of people in household, 1–50
    ///
    /// **Optional Fields (all nullable):**
    /// - `maleCount` / `femaleCount`: Total males / females across all ages
    /// - `adultCount`: Number of adults
    /// - `childCount`: Number of children
    /// - `elderlyCount`: Number of elderly
    /// - `disabledCount`: Number of persons with disabilities
    /// - `occupancyNature`: Occupancy nature enum code
    /// - `occupancyStartDate`: ISO-8601 UTC datetime
    /// - `notes`: Free text, ≤ 2000 chars
    ///
    /// **Validation (upper-bound only, gaps allowed):**
    /// - `maleCount + femaleCount ≤ householdSize`
    /// - `adultCount + childCount + elderlyCount ≤ householdSize`
    /// - `disabledCount ≤ householdSize`
    /// - PropertyUnit must exist
    ///
    /// **Example Request:**
    /// ```json
    /// {
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "householdSize": 6,
    ///   "maleCount": 3,
    ///   "femaleCount": 3,
    ///   "adultCount": 2,
    ///   "childCount": 3,
    ///   "elderlyCount": 1,
    ///   "disabledCount": 0,
    ///   "occupancyNature": 1,
    ///   "occupancyStartDate": "2023-09-01T00:00:00Z",
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
    ///   "householdSize": 6,
    ///   "maleCount": 3,
    ///   "femaleCount": 3,
    ///   "adultCount": 2,
    ///   "childCount": 3,
    ///   "elderlyCount": 1,
    ///   "disabledCount": 0,
    ///   "occupancyNature": 1,
    ///   "occupancyStartDate": "2023-09-01T00:00:00Z",
    ///   "notes": "أسرة مكونة من الأب والأم وثلاثة أطفال والجد",
    ///   "createdAtUtc": "2026-04-10T10:00:00Z",
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
    /// **Updatable Fields (all optional, nullable):**
    /// - `householdSize`, `notes`
    /// - `maleCount`, `femaleCount`, `adultCount`, `childCount`, `elderlyCount`, `disabledCount`
    /// - `occupancyNature` (enum code), `occupancyStartDate` (UTC)
    ///
    /// **Note:** `propertyUnitId` cannot be changed after creation.
    /// To move a household, delete and recreate.
    ///
    /// **Example Request - partial update (only occupancy start date):**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "occupancyStartDate": "2024-01-15T00:00:00Z"
    /// }
    /// ```
    ///
    /// **Example Request - full update:**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "householdSize": 5,
    ///   "maleCount": 3,
    ///   "femaleCount": 2,
    ///   "adultCount": 2,
    ///   "childCount": 2,
    ///   "elderlyCount": 1,
    ///   "disabledCount": 0,
    ///   "occupancyNature": 1,
    ///   "occupancyStartDate": "2023-09-01T00:00:00Z",
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
    /// Retrieves detailed information about a specific household.
    ///
    /// **Use Case**: View household details, verify demographic data
    ///
    /// **Required Permission**: Surveys_ViewAll (7004) - CanViewAllSurveys policy
    ///
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "7e439aab-5dd1-4a8a-b6c4-265008e53b86",
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "propertyUnitIdentifier": "Unit-101",
    ///   "householdSize": 6,
    ///   "maleCount": 3,
    ///   "femaleCount": 3,
    ///   "adultCount": 2,
    ///   "childCount": 3,
    ///   "elderlyCount": 1,
    ///   "disabledCount": 0,
    ///   "occupancyNature": 1,
    ///   "occupancyStartDate": "2023-09-01T00:00:00Z",
    ///   "notes": "أسرة مكونة من الأب والأم وثلاثة أطفال والجد",
    ///   "createdAtUtc": "2026-04-10T10:00:00Z",
    ///   "createdBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "lastModifiedAtUtc": "2026-04-10T14:30:00Z",
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
    ///     "householdSize": 6,
    ///     "maleCount": 3,
    ///     "femaleCount": 3,
    ///     "adultCount": 2,
    ///     "childCount": 3,
    ///     "elderlyCount": 1,
    ///     "disabledCount": 0,
    ///     "occupancyNature": 1,
    ///     "occupancyStartDate": "2023-09-01T00:00:00Z",
    ///     "createdAtUtc": "2026-04-10T10:00:00Z"
    ///   },
    ///   {
    ///     "id": "8f540bbc-6ee2-5b9b-c7d5-376119f64c97",
    ///     "propertyUnitId": "4gb96g75-6828-5673-c4gd-3d074g77bgb7",
    ///     "propertyUnitIdentifier": "Unit-102",
    ///     "householdSize": 4,
    ///     "maleCount": 2,
    ///     "femaleCount": 2,
    ///     "adultCount": 2,
    ///     "childCount": 2,
    ///     "elderlyCount": 0,
    ///     "disabledCount": 0,
    ///     "occupancyNature": 2,
    ///     "occupancyStartDate": null,
    ///     "createdAtUtc": "2026-04-10T11:30:00Z"
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