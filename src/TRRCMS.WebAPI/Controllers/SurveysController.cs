using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Application.Surveys.Commands.AddPersonToHousehold;
using TRRCMS.Application.Surveys.Commands.CreateFieldSurvey;
using TRRCMS.Application.Surveys.Commands.CreateHouseholdInSurvey;
using TRRCMS.Application.Surveys.Commands.CreateOfficeSurvey;
using TRRCMS.Application.Surveys.Commands.CreatePropertyUnitInSurvey;
using TRRCMS.Application.Surveys.Commands.DeleteEvidence;
using TRRCMS.Application.Surveys.Commands.DeletePersonPropertyRelation;
using TRRCMS.Application.Surveys.Commands.FinalizeFieldSurvey;
using TRRCMS.Application.Surveys.Commands.FinalizeOfficeSurvey;
using TRRCMS.Application.Surveys.Commands.LinkPersonToPropertyUnit;
using TRRCMS.Application.Surveys.Commands.LinkPropertyUnitToSurvey;
using TRRCMS.Application.Surveys.Commands.SaveDraftSurvey;
using TRRCMS.Application.Surveys.Commands.SetHouseholdHead;
using TRRCMS.Application.Surveys.Commands.UpdateHouseholdInSurvey;
using TRRCMS.Application.Surveys.Commands.UpdateOfficeSurvey;
using TRRCMS.Application.Surveys.Commands.UpdatePersonPropertyRelation;
using TRRCMS.Application.Surveys.Commands.UpdatePropertyUnitInSurvey;
using TRRCMS.Application.Surveys.Commands.UploadIdentificationDocument;
using TRRCMS.Application.Surveys.Commands.UploadPropertyPhoto;
using TRRCMS.Application.Surveys.Commands.UploadTenureDocument;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Application.Surveys.Queries.DownloadEvidence;
using TRRCMS.Application.Surveys.Queries.GetDraftSurvey;
using TRRCMS.Application.Surveys.Queries.GetEvidenceById;
using TRRCMS.Application.Surveys.Queries.GetEvidencesByRelation;
using TRRCMS.Application.Surveys.Queries.GetFieldDraftSurveys;
using TRRCMS.Application.Surveys.Queries.GetFieldSurveyById;
using TRRCMS.Application.Surveys.Queries.GetFieldSurveys;
using TRRCMS.Application.Surveys.Queries.GetHouseholdInSurvey;
using TRRCMS.Application.Surveys.Queries.GetHouseholdPersons;
using TRRCMS.Application.Surveys.Queries.GetHouseholdsForSurvey;
using TRRCMS.Application.Surveys.Queries.GetOfficeDraftSurveys;
using TRRCMS.Application.Surveys.Queries.GetOfficeSurveyById;
using TRRCMS.Application.Surveys.Queries.GetOfficeSurveys;
using TRRCMS.Application.Surveys.Queries.GetPropertyUnitsForSurvey;
using TRRCMS.Application.Surveys.Queries.GetSurveyEvidence;
using TRRCMS.Domain.Enums;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Surveys controller for field and office survey operations
/// </summary>
/// <remarks>
/// Supports UC-001 (Field Survey), UC-004 (Office Survey), and UC-005 (Draft Survey Management)
/// 
/// **Permissions:**
/// - Surveys_Create (7000) - CanCreateSurveys
/// - Surveys_ViewOwn (7001) - CanViewOwnSurveys
/// - Surveys_EditOwn (7002) - CanEditOwnSurveys
/// - Surveys_ViewAll (7004) - CanViewSurveys
/// - Surveys_Finalize (7008) - CanFinalizeSurveys
/// </remarks>
[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class SurveysController : ControllerBase
{
    private readonly IMediator _mediator;

    public SurveysController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    // ==================== FIELD SURVEY MANAGEMENT ====================

    /// <summary>
    /// Create a new field survey
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 1 - Building Identification
    /// 
    /// **Purpose**: Initiates a new field survey for a specific building. This is the first step in the field survey workflow.
    /// 
    /// **What it does**:
    /// - Creates a new survey record in Draft status
    /// - Generates a unique reference code (format: ALG-YYYY-NNNNN)
    /// - Links the survey to a building
    /// - Records GPS coordinates of the survey location
    /// - Records interviewee information
    /// - Automatically assigns the survey to the current user (field collector)
    /// 
    /// **Required permissions**: CanCreateSurveys
    /// </remarks>
    /// <param name="command">Field survey creation data</param>
    /// <returns>Created survey with reference code and status</returns>
    [HttpPost("field")]
    [Authorize(Policy = "CanCreateSurveys")]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDto>> CreateFieldSurvey(
        [FromBody] CreateFieldSurveyCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetSurvey), new { id = result.Id }, result);
    }

    // ==================== OFFICE SURVEY MANAGEMENT (UC-004/UC-005) ====================

    /// <summary>
    /// Create a new office survey
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-004 - Office Survey for Walk-in Claimants
    /// 
    /// **Purpose**: Initiates a new office survey when a claimant visits the office to submit a claim. This is the desktop alternative to field surveys.
    /// 
    /// **What it does**:
    /// - Creates a new survey record in Draft status with Type=Office
    /// - Generates a unique reference code (format: OFC-YYYY-NNNNN)
    /// - Records office location and registration details
    /// - Links the survey to a building
    /// - Records contact information for follow-up
    /// - Automatically assigns the survey to the current user (office clerk)
    /// 
    /// **When to use**:
    /// - When a claimant visits the office in person
    /// - When processing walk-in claims
    /// - When receiving claims remotely via documentation
    /// 
    /// **Required permissions**: CanCreateSurveys
    /// 
    /// **Example**:
    /// ```json
    /// {
    ///   "buildingId": "12345678-1234-1234-1234-123456789012",
    ///   "surveyDate": "2026-01-24T10:00:00Z",
    ///   "officeLocation": "UN-Habitat Aleppo Office",
    ///   "registrationNumber": "REG-2026-001234",
    ///   "inPersonVisit": true,
    ///   "intervieweeName": "محمد أحمد السيد",
    ///   "intervieweeRelationship": "مالك العقار",
    ///   "contactPhone": "+963912345678",
    ///   "contactEmail": "example@email.com",
    ///   "notes": "Walk-in claimant with property deed documentation"
    /// }
    /// ```
    /// 
    /// **Response**: Returns the created survey with auto-generated reference code (OFC-YYYY-NNNNN)
    /// 
    /// **Next steps**: 
    /// 1. Link or create property unit using POST /api/v1/surveys/{surveyId}/property-units
    /// 2. Add household and person data
    /// 3. Upload evidence documents
    /// 4. Finalize to create claim using POST /api/v1/surveys/office/{id}/finalize
    /// </remarks>
    /// <param name="command">Office survey creation data</param>
    /// <returns>Created survey with reference code and status</returns>
    /// <response code="201">Survey created successfully. Returns survey details with reference code.</response>
    /// <response code="400">Invalid input. Check building ID, phone format, email format, or date validity.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. User lacks CanCreateSurveys permission.</response>
    /// <response code="404">Building not found. Verify the building ID exists.</response>
    [HttpPost("office")]
    [Authorize(Policy = "CanCreateSurveys")]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDto>> CreateOfficeSurvey(
        [FromBody] CreateOfficeSurveyCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetOfficeSurveyById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get all office surveys with filtering and pagination
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-004/UC-005 - List and search office surveys
    /// 
    /// **Purpose**: Lists office surveys with flexible filtering options for clerks and supervisors.
    /// 
    /// **What it does**:
    /// - Returns paginated list of office surveys only (excludes field surveys)
    /// - Supports multiple filter criteria
    /// - Allows sorting by various fields
    /// - Includes related building and property unit information
    /// 
    /// **Available Filters**:
    /// - status: Filter by status (Draft, Completed, Finalized)
    /// - buildingId: Filter by specific building
    /// - clerkId: Filter by office clerk who created the survey
    /// - fromDate/toDate: Date range filter for survey date
    /// - referenceCode: Search by reference code (partial match)
    /// - intervieweeName: Search by interviewee name (partial match)
    /// 
    /// **Sorting Options**:
    /// - SurveyDate (default), ReferenceCode, Status, CreatedAtUtc
    /// - Direction: asc or desc (default: desc)
    /// 
    /// **Required permissions**: CanViewOwnSurveys
    /// 
    /// **Example Request**:
    /// ```
    /// GET /api/surveys/office?status=Draft&amp;fromDate=2026-01-01&amp;page=1&amp;pageSize=20&amp;sortBy=SurveyDate&amp;sortDirection=desc
    /// ```
    /// 
    /// **Response**: Paginated list with survey summaries and pagination metadata
    /// </remarks>
    /// <param name="status">Filter by survey status (Draft, Completed, Finalized)</param>
    /// <param name="buildingId">Filter by building ID</param>
    /// <param name="clerkId">Filter by office clerk ID</param>
    /// <param name="fromDate">Filter surveys from this date</param>
    /// <param name="toDate">Filter surveys up to this date</param>
    /// <param name="referenceCode">Search by reference code (partial match)</param>
    /// <param name="intervieweeName">Search by interviewee name (partial match)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field (SurveyDate, ReferenceCode, Status, CreatedAtUtc)</param>
    /// <param name="sortDirection">Sort direction (asc or desc)</param>
    /// <returns>Paginated list of office surveys</returns>
    /// <response code="200">Success. Returns paginated list of surveys.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. User lacks CanViewOwnSurveys permission.</response>
    [HttpGet("office")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(GetOfficeSurveysResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetOfficeSurveysResponse>> GetOfficeSurveys(
        [FromQuery] string? status = null,
        [FromQuery] Guid? buildingId = null,
        [FromQuery] Guid? clerkId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? referenceCode = null,
        [FromQuery] string? intervieweeName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "SurveyDate",
        [FromQuery] string sortDirection = "desc")
    {
        var query = new GetOfficeSurveysQuery
        {
            Status = status,
            BuildingId = buildingId,
            ClerkId = clerkId,
            FromDate = fromDate,
            ToDate = toDate,
            ReferenceCode = referenceCode,
            IntervieweeName = intervieweeName,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get current clerk's draft office surveys
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-005 - Resume draft office survey
    /// 
    /// **Purpose**: Retrieves all draft office surveys created by the current user, allowing them to resume incomplete work.
    /// 
    /// **What it does**:
    /// - Filters surveys by current user ID
    /// - Only returns surveys with Type=Office and Status=Draft
    /// - Sorted by last modified date (most recent first)
    /// - Includes building and property unit information
    /// 
    /// **When to use**:
    /// - When logging in to continue previous work
    /// - To view incomplete surveys that need attention
    /// - For the "My Drafts" view in the desktop application
    /// 
    /// **Required permissions**: CanViewOwnSurveys
    /// 
    /// **Response**: List of draft surveys owned by the current user (may be empty)
    /// </remarks>
    /// <returns>List of draft office surveys</returns>
    [HttpGet("office/drafts")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(List<SurveyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<SurveyDto>>> GetOfficeDraftSurveys()
    {
        var query = new GetOfficeDraftSurveysQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get office survey details by ID
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-004/UC-005 - View full office survey details
    /// 
    /// **Purpose**: Retrieves complete office survey with all related data.
    /// 
    /// **What you get**:
    /// - Survey details including office-specific fields
    /// - Building and property unit info
    /// - Linked households and persons
    /// - Person-property relations
    /// - Uploaded evidence
    /// - Linked claim info (if any)
    /// - Data summary counts
    /// 
    /// **Required permissions**: CanViewOwnSurveys
    /// </remarks>
    /// <param name="id">Office survey ID</param>
    /// <returns>Complete office survey details</returns>
    [HttpGet("office/{id}")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(OfficeSurveyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OfficeSurveyDetailDto>> GetOfficeSurveyById(Guid id)
    {
        var query = new GetOfficeSurveyByIdQuery { SurveyId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update office survey
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-004/UC-005 - Update office survey details
    /// 
    /// **Purpose**: Updates office survey fields (only for Draft surveys).
    /// 
    /// **Updateable fields**:
    /// - PropertyUnitId, IntervieweeName, IntervieweeRelationship, Notes, DurationMinutes
    /// - Office-specific: OfficeLocation, RegistrationNumber, AppointmentReference, ContactPhone, ContactEmail, InPersonVisit
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// </remarks>
    /// <param name="id">Office survey ID</param>
    /// <param name="command">Update data</param>
    /// <returns>Updated survey</returns>
    [HttpPut("office/{id}")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDto>> UpdateOfficeSurvey(
        Guid id,
        [FromBody] UpdateOfficeSurveyCommand command)
    {
        command.SurveyId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Finalize office survey and optionally create claim
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-004 S21 / UC-005 - Finalize office survey
    /// 
    /// **Purpose**: Marks survey as finalized and optionally creates a claim.
    /// 
    /// **What it does**:
    /// - Validates survey has required data (property unit linked)
    /// - Collects data summary (households, persons, relations, evidence)
    /// - Marks survey as Finalized
    /// - If AutoCreateClaim=true AND ownership relations exist:
    ///   - Generates claim number (CLM-YYYY-NNNNNNNNN)
    ///   - Creates claim with ClaimSource=OfficeSubmission
    ///   - Links claim to survey
    /// - Returns finalization result with summary
    /// 
    /// **Required permissions**: CanFinalizeSurveys
    /// 
    /// **Example**:
    /// ```json
    /// {
    ///   "finalNotes": "Survey completed successfully",
    ///   "durationMinutes": 45,
    ///   "autoCreateClaim": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Office survey ID</param>
    /// <param name="command">Finalization options</param>
    /// <returns>Finalization result with claim info</returns>
    [HttpPost("office/{id}/finalize")]
    [Authorize(Policy = "CanFinalizeSurveys")]
    [ProducesResponseType(typeof(OfficeSurveyFinalizationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OfficeSurveyFinalizationResultDto>> FinalizeOfficeSurvey(
        Guid id,
        [FromBody] FinalizeOfficeSurveyCommand command)
    {
        command.SurveyId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==================== COMMON SURVEY OPERATIONS ====================

    /// <summary>
    /// Save survey progress as draft (works for both field and office surveys)
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-002/UC-005 - Save draft and exit safely
    /// 
    /// **Purpose**: Saves current survey progress without finalizing.
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// </remarks>
    /// <param name="id">Survey ID to update</param>
    /// <param name="command">Draft update data (all fields optional)</param>
    /// <returns>Updated survey details</returns>
    [HttpPut("{id}/draft")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDto>> SaveDraft(
        Guid id,
        [FromBody] SaveDraftSurveyCommand command)
    {
        command.SurveyId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get survey by ID (works for both field and office surveys)
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-002 - Resume draft survey
    /// 
    /// **Purpose**: Retrieves complete survey details to resume work or view progress.
    /// 
    /// **Required permissions**: CanViewOwnSurveys
    /// </remarks>
    /// <param name="id">Survey ID to retrieve</param>
    /// <returns>Complete survey details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDto>> GetSurvey(Guid id)
    {
        var query = new GetDraftSurveyQuery { SurveyId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ==================== PROPERTY UNIT MANAGEMENT ====================
    // Add these endpoints to your existing SurveysController.cs
    // Replace the old property unit endpoints with these updated versions

    /// <summary>
    /// Get all property units for survey's building
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 2 - View available property units
    /// 
    /// **Purpose**: Lists all property units in the building being surveyed.
    /// Field collectors can see existing units to select or create new ones.
    /// 
    /// **Required Permission**: CanViewOwnSurveys
    /// 
    /// **Response includes**:
    /// - Unit identifier (رقم الوحدة)
    /// - Unit type (نوع الوحدة)
    /// - Status (حالة الوحدة)
    /// - Floor number (رقم الطابق)
    /// - Area in m² (مساحة القسم)
    /// - Number of rooms (عدد الغرف)
    /// - Description (وصف مفصل)
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
    ///     "areaSquareMeters": 45.0,
    ///     "numberOfRooms": null,
    ///     "description": "محل تجاري"
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
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="surveyId">Survey ID to get property units for</param>
    /// <returns>List of property units in the building</returns>
    /// <response code="200">Success. Returns array of property units (may be empty).</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only view units for your own surveys.</response>
    /// <response code="404">Survey not found. Verify the survey ID.</response>
    [HttpGet("{surveyId}/property-units")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(List<PropertyUnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PropertyUnitDto>>> GetPropertyUnitsForSurvey(Guid surveyId)
    {
        var query = new GetPropertyUnitsForSurveyQuery { SurveyId = surveyId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create new property unit in survey context
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 2 - Create new property unit during field survey
    /// 
    /// **Purpose**: Creates a new property unit and automatically links it to the survey.
    /// Use this when the unit doesn't exist in the system yet.
    /// 
    /// **Required Permission**: CanEditOwnSurveys
    /// 
    /// **What it does**:
    /// 1. Creates property unit record in survey's building
    /// 2. Links the unit to the survey automatically
    /// 3. Validates unit identifier is unique within building
    /// 
    /// **Required Fields**:
    /// - unitIdentifier: رقم الوحدة (e.g., "1A", "G-1", "الطابق الأول-يمين")
    /// - unitType: نوع الوحدة (1=Apartment, 2=Shop, 3=Office, 4=Warehouse, 5=Other)
    /// - status: حالة الوحدة (1=Occupied, 2=Vacant, 3=Damaged, 4=UnderRenovation, 5=Uninhabitable, 6=Locked, 99=Unknown)
    /// 
    /// **Optional Fields**:
    /// - floorNumber: رقم الطابق (0=Ground, 1=First, -1=Basement)
    /// - areaSquareMeters: مساحة القسم
    /// - numberOfRooms: عدد الغرف
    /// - description: وصف مفصل
    /// 
    /// **Example Request - Apartment**:
    /// ```json
    /// {
    ///   "unitIdentifier": "1A",
    ///   "floorNumber": 1,
    ///   "unitType": 1,
    ///   "status": 1,
    ///   "areaSquareMeters": 85.5,
    ///   "numberOfRooms": 3,
    ///   "description": "شقة سكنية مؤلفة من 3 غرف وصالة"
    /// }
    /// ```
    /// 
    /// **Example Request - Shop**:
    /// ```json
    /// {
    ///   "unitIdentifier": "G-1",
    ///   "floorNumber": 0,
    ///   "unitType": 2,
    ///   "status": 1,
    ///   "areaSquareMeters": 45.0,
    ///   "numberOfRooms": null,
    ///   "description": "محل تجاري في الطابق الأرضي"
    /// }
    /// ```
    /// 
    /// **Example Request - Vacant Unit**:
    /// ```json
    /// {
    ///   "unitIdentifier": "2B",
    ///   "floorNumber": 2,
    ///   "unitType": 1,
    ///   "status": 2,
    ///   "areaSquareMeters": 90.0,
    ///   "numberOfRooms": 4,
    ///   "description": "شقة شاغرة"
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
    ///   "description": "شقة سكنية مؤلفة من 3 غرف وصالة",
    ///   "createdAtUtc": "2026-01-29T12:00:00Z",
    ///   "lastModifiedAtUtc": null
    /// }
    /// ```
    /// </remarks>
    /// <param name="surveyId">Survey ID to create the property unit for</param>
    /// <param name="command">Property unit creation data</param>
    /// <returns>Created property unit details</returns>
    /// <response code="201">Property unit created successfully and linked to survey.</response>
    /// <response code="400">Invalid input. Check validation rules and duplicate identifiers.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only create units for your own surveys.</response>
    /// <response code="404">Survey or building not found.</response>
    /// <response code="409">Property unit with same identifier already exists in building.</response>
    [HttpPost("{surveyId}/property-units")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(PropertyUnitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PropertyUnitDto>> CreatePropertyUnitInSurvey(
        Guid surveyId,
        [FromBody] CreatePropertyUnitInSurveyCommand command)
    {
        command.SurveyId = surveyId;
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetSurvey), new { id = surveyId }, result);
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            if (ex.Message.Contains("already exists"))
            {
                return Conflict(new { message = ex.Message });
            }
            return BadRequest(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update property unit in survey context
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 2 - Update property unit details during field survey
    /// 
    /// **Purpose**: Updates existing property unit details. Only provided fields will be updated.
    /// 
    /// **Required Permission**: CanEditOwnSurveys
    /// 
    /// **What it does**:
    /// - Validates unit belongs to survey's building
    /// - Updates only the fields you provide
    /// - Records changes in audit trail
    /// 
    /// **Updateable Fields** (all optional):
    /// - floorNumber: رقم الطابق
    /// - unitType: نوع الوحدة (1-5)
    /// - status: حالة الوحدة (1-6 or 99)
    /// - areaSquareMeters: مساحة القسم
    /// - numberOfRooms: عدد الغرف
    /// - description: وصف مفصل
    /// 
    /// **Example Request - Update Status Only**:
    /// ```json
    /// {
    ///   "status": 3
    /// }
    /// ```
    /// 
    /// **Example Request - Update Multiple Fields**:
    /// ```json
    /// {
    ///   "status": 1,
    ///   "numberOfRooms": 4,
    ///   "areaSquareMeters": 95.0,
    ///   "description": "تم تجديد الشقة وإضافة غرفة"
    /// }
    /// ```
    /// 
    /// **Example Request - Mark as Damaged**:
    /// ```json
    /// {
    ///   "status": 3,
    ///   "description": "أضرار في السقف والجدران بسبب تسرب المياه"
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
    ///   "status": "Damaged",
    ///   "areaSquareMeters": 85.5,
    ///   "numberOfRooms": 3,
    ///   "description": "أضرار في السقف والجدران بسبب تسرب المياه",
    ///   "createdAtUtc": "2026-01-29T12:00:00Z",
    ///   "lastModifiedAtUtc": "2026-01-29T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="unitId">Property unit ID to update</param>
    /// <param name="command">Property unit update data (all fields optional)</param>
    /// <returns>Updated property unit details</returns>
    /// <response code="200">Property unit updated successfully.</response>
    /// <response code="400">Invalid input or unit doesn't belong to survey's building.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only update units for your own surveys.</response>
    /// <response code="404">Survey or property unit not found.</response>
    [HttpPut("{surveyId}/property-units/{unitId}")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(PropertyUnitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyUnitDto>> UpdatePropertyUnitInSurvey(
        Guid surveyId,
        Guid unitId,
        [FromBody] UpdatePropertyUnitInSurveyCommand command)
    {
        command.SurveyId = surveyId;
        command.PropertyUnitId = unitId;
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Link existing property unit to survey
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 2 - Select existing property unit
    /// 
    /// **Purpose**: Links an existing property unit to the survey without creating a new one.
    /// Use this when the unit already exists in the system.
    /// 
    /// **Required Permission**: CanEditOwnSurveys
    /// 
    /// **What it does**:
    /// 1. Validates unit belongs to survey's building
    /// 2. Links property unit to survey
    /// 3. Updates survey's PropertyUnitId
    /// 
    /// **When to use**:
    /// - When selecting from list of existing units
    /// - For office surveys selecting previously created units
    /// - When another field collector already created the unit
    /// 
    /// **Example**: Link unit "1A" to a survey
    /// ```
    /// POST /api/Surveys/{surveyId}/property-units/{unitId}/link
    /// ```
    /// 
    /// **Example Response** (returns updated survey):
    /// ```json
    /// {
    ///   "id": "survey-guid",
    ///   "referenceCode": "SRV-20260129-0001",
    ///   "buildingId": "building-guid",
    ///   "propertyUnitId": "unit-guid",
    ///   "status": "Draft",
    ///   ...
    /// }
    /// ```
    /// </remarks>
    /// <param name="surveyId">Survey ID to link to</param>
    /// <param name="unitId">Property unit ID to link</param>
    /// <returns>Updated survey details</returns>
    /// <response code="200">Property unit linked successfully to survey.</response>
    /// <response code="400">Invalid input or unit doesn't belong to survey's building.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only link units to your own surveys.</response>
    /// <response code="404">Survey or property unit not found.</response>
    [HttpPost("{surveyId}/property-units/{unitId}/link")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDto>> LinkPropertyUnitToSurvey(
        Guid surveyId,
        Guid unitId)
    {
        var command = new LinkPropertyUnitToSurveyCommand
        {
            SurveyId = surveyId,
            PropertyUnitId = unitId
        };
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Application.Common.Exceptions.NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    // ==================== HOUSEHOLD MANAGEMENT ====================

    /// <summary>
    /// Get all households for survey
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 3 - View all households during survey
    /// 
    /// **Purpose**: Retrieves all households linked to the survey's property unit.
    /// 
    /// **Required Permission**: CanViewOwnSurveys
    /// 
    /// **Response**: List of households (may be empty if none created yet)
    /// </remarks>
    /// <param name="surveyId">Survey ID</param>
    /// <returns>List of households for this survey</returns>
    /// <response code="200">Success. Returns array of households.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only view households for your own surveys.</response>
    /// <response code="404">Survey not found.</response>
    [HttpGet("{surveyId}/households")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(List<HouseholdDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<HouseholdDto>>> GetHouseholdsForSurvey(Guid surveyId)
    {
        var query = new GetHouseholdsForSurveyQuery { SurveyId = surveyId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create household in survey context
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 3 - Household Registration
    /// تسجيل الأسرة - تسجيل تفاصيل الإشغال
    /// 
    /// **Purpose**: Creates a new household and links it to the survey's property unit.
    /// 
    /// **Required Permission**: CanEditOwnSurveys
    /// 
    /// **Form Fields (Arabic)**:
    /// - رب الأسرة/العميل: HeadOfHouseholdName (required)
    /// - عدد الأفراد: HouseholdSize (required)
    /// - ادخل ملاحظاتك: Notes (optional)
    /// 
    /// **تكوين الأسرة (Family Composition)**:
    /// - عدد البالغين الذكور: MaleCount
    /// - عدد البالغين الإناث: FemaleCount  
    /// - عدد الأطفال الذكور (أقل من 18): MaleChildCount
    /// - عدد الأطفال الإناث (أقل من 18): FemaleChildCount
    /// - عدد كبار السن الذكور (أكثر من 65): MaleElderlyCount
    /// - عدد كبار السن الإناث (أكثر من 65): FemaleElderlyCount
    /// - عدد المعاقين الذكور: MaleDisabledCount
    /// - عدد المعاقين الإناث: FemaleDisabledCount
    /// 
    /// **Example Request**:
    /// ```json
    /// {
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
    /// <param name="surveyId">Survey ID to create household for</param>
    /// <param name="command">Household creation data</param>
    /// <returns>Created household details</returns>
    /// <response code="201">Household created successfully.</response>
    /// <response code="400">Validation error. Survey must have linked property unit.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only create households for your own surveys.</response>
    /// <response code="404">Survey or property unit not found.</response>
    [HttpPost("{surveyId}/households")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(HouseholdDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdDto>> CreateHouseholdInSurvey(
        Guid surveyId,
        [FromBody] CreateHouseholdInSurveyCommand command)
    {
        command.SurveyId = surveyId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(
            nameof(GetHouseholdInSurvey),
            new { surveyId, householdId = result.Id },
            result);
    }

    /// <summary>
    /// Get household details
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 3 - View household information
    /// 
    /// **Purpose**: Retrieves complete household details.
    /// 
    /// **Required Permission**: CanViewOwnSurveys
    /// 
    /// **Response Fields**:
    /// - Identifiers (id, propertyUnitId)
    /// - Basic info (headOfHouseholdName, householdSize, notes)
    /// - Adults composition (maleCount, femaleCount)
    /// - Children composition (maleChildCount, femaleChildCount)
    /// - Elderly composition (maleElderlyCount, femaleElderlyCount)
    /// - Disabled composition (maleDisabledCount, femaleDisabledCount)
    /// - Audit fields (createdAtUtc, lastModifiedAtUtc)
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="householdId">Household ID to retrieve</param>
    /// <returns>Complete household details</returns>
    /// <response code="200">Household found. Returns complete details.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only view households for your own surveys.</response>
    /// <response code="404">Survey or household not found.</response>
    [HttpGet("{surveyId}/households/{householdId}")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(HouseholdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdDto>> GetHouseholdInSurvey(
        Guid surveyId,
        Guid householdId)
    {
        var query = new GetHouseholdInSurveyQuery
        {
            SurveyId = surveyId,
            HouseholdId = householdId
        };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = $"Household with ID {householdId} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Update household in survey context
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 3 - Update household during survey
    /// 
    /// **Purpose**: Updates existing household details. Only provided fields will be updated.
    /// 
    /// **Required Permission**: CanEditOwnSurveys
    /// 
    /// **Example Request** (partial update - only updates provided fields):
    /// ```json
    /// {
    ///   "householdSize": 6,
    ///   "maleChildCount": 3,
    ///   "notes": "ولد طفل ذكر جديد"
    /// }
    /// ```
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="householdId">Household ID to update</param>
    /// <param name="command">Household update data (all fields optional)</param>
    /// <returns>Updated household details</returns>
    /// <response code="200">Household updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only update households for your own surveys.</response>
    /// <response code="404">Survey or household not found.</response>
    [HttpPut("{surveyId}/households/{householdId}")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(HouseholdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdDto>> UpdateHouseholdInSurvey(
        Guid surveyId,
        Guid householdId,
        [FromBody] UpdateHouseholdInSurveyCommand command)
    {
        command.SurveyId = surveyId;
        command.HouseholdId = householdId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }


    // ==================== PERSON MANAGEMENT ====================

    /// <summary>
    /// Add person to household in survey context
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 3 - Person Registration
    /// إضافة شخص جديد
    /// 
    /// **Purpose**: Creates a new person and assigns them to a household.
    /// 
    /// **Required Permission**: Surveys_EditOwn (CanEditOwnSurveys)
    /// 
    /// **Step 1 - Personal Info (الخطوة الأولى)**:
    /// - الكنية: FamilyNameArabic (required)
    /// - الاسم الأول: FirstNameArabic (required)
    /// - اسم الأب: FatherNameArabic (required)
    /// - الاسم الأم: MotherNameArabic (optional)
    /// - الرقم الوطني: NationalId (optional)
    /// - تاريخ الميلاد: YearOfBirth (optional, year only)
    /// 
    /// **Step 2 - Contact Info (الخطوة الثانية)**:
    /// - البريد الالكتروني: Email (optional)
    /// - رقم الموبايل: MobileNumber (optional)
    /// - رقم الهاتف: PhoneNumber (optional)
    /// 
    /// **Household Relationship**:
    /// - RelationshipToHead: (optional, defaults to "Member")
    /// 
    /// **Example Request**:
    /// ```json
    /// {
    ///   "familyNameArabic": "الأحمد",
    ///   "firstNameArabic": "محمد",
    ///   "fatherNameArabic": "محمد",
    ///   "motherNameArabic": "فاطمة",
    ///   "nationalId": "00000000000",
    ///   "yearOfBirth": 1985,
    ///   "email": "*****@gmail.com",
    ///   "mobileNumber": "+963 09",
    ///   "phoneNumber": "0000000",
    ///   "relationshipToHead": "Spouse"
    /// }
    /// ```
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="householdId">Household ID to add person to</param>
    /// <param name="command">Person creation data</param>
    /// <returns>Created person details</returns>
    /// <response code="201">Person added to household successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only add persons to your own surveys.</response>
    /// <response code="404">Survey or household not found.</response>
    [HttpPost("{surveyId}/households/{householdId}/persons")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonDto>> AddPersonToHousehold(
        Guid surveyId,
        Guid householdId,
        [FromBody] AddPersonToHouseholdCommand command)
    {
        command.SurveyId = surveyId;
        command.HouseholdId = householdId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(
            nameof(GetHouseholdPersons),
            new { surveyId, householdId },
            result);
    }

    /// <summary>
    /// Get all persons in household
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 3 - View household members
    /// عرض أفراد الأسرة
    /// 
    /// **Purpose**: Lists all registered persons/members in a household.
    /// 
    /// **Required Permission**: Surveys_ViewOwn (CanViewOwnSurveys)
    /// 
    /// **Response**: Array of persons ordered by creation date
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="householdId">Household ID to get persons for</param>
    /// <returns>List of persons in the household</returns>
    /// <response code="200">Success. Returns array of persons (may be empty).</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only view persons in your own surveys.</response>
    /// <response code="404">Survey or household not found.</response>
    [HttpGet("{surveyId}/households/{householdId}/persons")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(List<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PersonDto>>> GetHouseholdPersons(
        Guid surveyId,
        Guid householdId)
    {
        var query = new GetHouseholdPersonsQuery
        {
            SurveyId = surveyId,
            HouseholdId = householdId
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Set household head
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 3 - Designate household head
    /// 
    /// **Purpose**: Links a Person entity as the official head of household.
    /// 
    /// **What it does**:
    /// - Designates a person as head of household
    /// - Updates household's HeadOfHouseholdPersonId
    /// - Creates audit trail
    /// 
    /// **When to use**:
    /// - After adding persons to household
    /// - To officially designate the household head
    /// - Person must already be member of this household
    /// 
    /// **Important**: Person must belong to this household (HouseholdId must match)
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// 
    /// **Response**: Returns updated household with linked head
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="householdId">Household ID</param>
    /// <param name="personId">Person ID to set as head</param>
    /// <returns>Updated household details</returns>
    /// <response code="200">Household head set successfully.</response>
    /// <response code="400">Person doesn't belong to this household.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only modify your own surveys.</response>
    /// <response code="404">Survey, household, or person not found.</response>
    [HttpPut("{surveyId}/households/{householdId}/head/{personId}")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(HouseholdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdDto>> SetHouseholdHead(
        Guid surveyId,
        Guid householdId,
        Guid personId)
    {
        var command = new SetHouseholdHeadCommand
        {
            SurveyId = surveyId,
            HouseholdId = householdId,
            PersonId = personId
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==================== PERSON-PROPERTY RELATIONS ====================

    /// <summary>
    /// Link person to property unit with relation type
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 3 - Establish person-property relationship
    /// 
    /// **Purpose**: Creates relationship between person and property unit for ownership/tenancy tracking.
    /// 
    /// **What it does**:
    /// - Creates PersonPropertyRelation record
    /// - Links person to property unit
    /// - Records relationship type (Owner, Tenant, Occupant, etc.)
    /// - Validates person and unit belong to same survey building
    /// 
    /// **When to use**:
    /// - To establish ownership claims
    /// - To document tenancy arrangements
    /// - To link occupants to units
    /// - For tenure rights documentation
    /// 
    /// **Relationship types**:
    /// - Owner: Legal owner of the property
    /// - Heirs: Inherited ownership rights
    /// - Tenant: Renter/tenant with lease
    /// - Occupant: Current occupant without formal tenure
    /// - Claimant: Claiming ownership/rights
    /// - Co-owner: Shared ownership
    /// 
    /// **Important**: 
    /// - Person and property unit must be in same building
    /// - Used for later claims processing
    /// - Ownership relations (Owner, Heirs) qualify for automatic claim creation
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// 
    /// **Example**:
    /// ```json
    /// {
    ///   "personId": "person-guid-here",
    ///   "relationType": "Owner",
    ///   "startDate": "2010-01-01",
    ///   "notes": "Primary owner with deed documentation"
    /// }
    /// ```
    /// 
    /// **Response**: Created relation record with details
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="unitId">Property unit ID</param>
    /// <param name="command">Link command with personId and relationType</param>
    /// <returns>Created relation details</returns>
    /// <response code="201">Person linked to property unit successfully.</response>
    /// <response code="400">Person already linked to this unit or unit not in survey building.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only link persons in your own surveys.</response>
    /// <response code="404">Survey, person, or property unit not found.</response>
    [HttpPost("{surveyId}/property-units/{unitId}/relations")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(Application.PersonPropertyRelations.Dtos.PersonPropertyRelationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Application.PersonPropertyRelations.Dtos.PersonPropertyRelationDto>> LinkPersonToPropertyUnit(
        Guid surveyId,
        Guid unitId,
        [FromBody] LinkPersonToPropertyUnitCommand command)
    {
        command.SurveyId = surveyId;
        command.PropertyUnitId = unitId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetSurvey), new { id = surveyId }, result);
    }

    // ==================== EVIDENCE MANAGEMENT ====================

    /// <summary>
    /// Upload property photo
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 4 - Evidence Collection (Property Photos)
    /// 
    /// **Purpose**: Upload photos of property (exterior, interior, damage documentation).
    /// 
    /// **What it does**:
    /// - Uploads photo file to storage
    /// - Creates Evidence record
    /// - Links to property unit or person-property relation (optional)
    /// - Calculates file hash for integrity
    /// - Validates file type and size
    /// 
    /// **When to use**:
    /// - Document property condition
    /// - Capture damage or improvements
    /// - Record exterior and interior views
    /// - Support tenure claims with visual evidence
    /// 
    /// **File Requirements**:
    /// - Format: JPG, JPEG, PNG, GIF
    /// - Max size: 10MB
    /// - Required: Photo file, description
    /// 
    /// **Optional Linking**:
    /// - propertyUnitId: Link to specific unit
    /// - personPropertyRelationId: Link to ownership/tenancy record
    /// - Can upload first, link later
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// 
    /// **Example** (multipart/form-data):
    /// ```
    /// File: property-front.jpg (binary)
    /// Description: Front facade of building
    /// PropertyUnitId: unit-guid-here (optional)
    /// Notes: Photo taken during field visit
    /// ```
    /// 
    /// **Response**: Evidence record with file details and download path
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="command">Upload command with file and metadata (from form)</param>
    /// <returns>Created evidence record</returns>
    /// <response code="201">Photo uploaded successfully.</response>
    /// <response code="400">Invalid file type, size exceeded, or validation error.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only upload to your own surveys.</response>
    /// <response code="404">Survey or property unit not found.</response>
    [HttpPost("{surveyId}/evidence/photos")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(EvidenceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvidenceDto>> UploadPropertyPhoto(
        Guid surveyId,
        [FromForm] UploadPropertyPhotoCommand command)
    {
        command.SurveyId = surveyId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetEvidenceById), new { evidenceId = result.Id }, result);
    }

    /// <summary>
    /// Upload identification document for person
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 4 - Evidence Collection (ID Documents)
    /// 
    /// **Purpose**: Upload identification documents for registered persons (ID cards, passports, birth certificates).
    /// 
    /// **What it does**:
    /// - Uploads ID document to storage
    /// - Links directly to Person entity
    /// - Records document metadata (issue date, expiry, authority)
    /// - Marks person as having identification document
    /// - Validates file type and size
    /// 
    /// **When to use**:
    /// - Document person identity
    /// - Support claim verification
    /// - Comply with identification requirements
    /// - Establish person legitimacy
    /// 
    /// **File Requirements**:
    /// - Format: PDF, JPG, JPEG, PNG
    /// - Max size: 10MB
    /// - Required: File, description, person ID
    /// 
    /// **Document Metadata** (optional):
    /// - documentIssuedDate: When document was issued
    /// - documentExpiryDate: When document expires
    /// - issuingAuthority: Who issued (e.g., "Ministry of Interior")
    /// - documentReferenceNumber: Official document number
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// 
    /// **Example** (multipart/form-data):
    /// ```
    /// File: national-id.pdf (binary)
    /// PersonId: person-guid-here
    /// Description: National ID Card
    /// DocumentIssuedDate: 2020-01-15
    /// DocumentExpiryDate: 2030-01-15
    /// IssuingAuthority: Ministry of Interior
    /// DocumentReferenceNumber: 123456789
    /// ```
    /// 
    /// **Response**: Evidence record with file details
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="command">Upload command with file and metadata (from form)</param>
    /// <returns>Created evidence record</returns>
    /// <response code="201">Document uploaded successfully.</response>
    /// <response code="400">Invalid file type, size exceeded, or person not in survey.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only upload to your own surveys.</response>
    /// <response code="404">Survey or person not found.</response>
    [HttpPost("{surveyId}/evidence/identification")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(EvidenceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvidenceDto>> UploadIdentificationDocument(
        Guid surveyId,
        [FromForm] UploadIdentificationDocumentCommand command)
    {
        command.SurveyId = surveyId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetEvidenceById), new { evidenceId = result.Id }, result);
    }

    /// <summary>
    /// Upload tenure/ownership document
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 4 - Evidence Collection (Tenure Documents)
    /// 
    /// **Purpose**: Upload documents proving ownership, tenancy, or other property rights.
    /// 
    /// **What it does**:
    /// - Uploads tenure document to storage
    /// - Links to PersonPropertyRelation (ownership/tenancy record)
    /// - Records document metadata
    /// - Validates file type and size
    /// - Supports future claims processing
    /// 
    /// **When to use**:
    /// - Document property ownership
    /// - Prove tenancy arrangements
    /// - Support inheritance claims
    /// - Establish tenure rights
    /// 
    /// **Document Types**:
    /// - Property deeds (ownership)
    /// - Rental contracts (tenancy)
    /// - Inheritance documents
    /// - Court orders
    /// - Municipal records
    /// - Utility bills (as proof of residence)
    /// 
    /// **File Requirements**:
    /// - Format: PDF, JPG, JPEG, PNG
    /// - Max size: 10MB
    /// - Required: File, description, person-property relation ID
    /// 
    /// **Document Metadata** (optional):
    /// - documentIssuedDate: When document was issued
    /// - documentExpiryDate: When document expires (for temporary rights)
    /// - issuingAuthority: Who issued (e.g., "Real Estate Registry", "Court")
    /// - documentReferenceNumber: Official registration/deed number
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// 
    /// **Example** (multipart/form-data):
    /// ```
    /// File: property-deed.pdf (binary)
    /// PersonPropertyRelationId: relation-guid-here
    /// Description: Property Ownership Deed
    /// DocumentIssuedDate: 2015-06-20
    /// IssuingAuthority: Real Estate Registry Office
    /// DocumentReferenceNumber: DEED-2015-123456
    /// ```
    /// 
    /// **Response**: Evidence record with file details
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="command">Upload command with file and metadata (from form)</param>
    /// <returns>Created evidence record</returns>
    /// <response code="201">Document uploaded successfully.</response>
    /// <response code="400">Invalid file type, size exceeded, or relation not in survey.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only upload to your own surveys.</response>
    /// <response code="404">Survey or person-property relation not found.</response>
    [HttpPost("{surveyId}/evidence/tenure")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(EvidenceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvidenceDto>> UploadTenureDocument(
        Guid surveyId,
        [FromForm] UploadTenureDocumentCommand command)
    {
        command.SurveyId = surveyId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetEvidenceById), new { evidenceId = result.Id }, result);
    }

    /// <summary>
    /// Get all evidence for a survey
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 4 - View survey evidence
    /// 
    /// **Purpose**: Retrieves all evidence (documents, photos) uploaded during survey.
    /// 
    /// **What you get**:
    /// - List of evidence with type, description, file info
    /// - Expiration status for documents
    /// - Links to related persons and property units
    /// 
    /// **Filter options**:
    /// - evidenceType: Filter by type (1=IdentificationDocument, 2=OwnershipDeed, 3=RentalContract, 4=UtilityBill, 5=Photo, etc.)
    /// 
    /// **Required permissions**: Field collector can only view their own surveys.
    /// </remarks>
    /// <param name="surveyId">Survey ID</param>
    /// <param name="evidenceType">Optional filter by evidence type (int or name: 1, "Photo", "OwnershipDeed", etc.)</param>
    /// <response code="200">Success. Returns array of evidence (may be empty).</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only view evidence for your own surveys.</response>
    /// <response code="404">Survey not found.</response>
    [HttpGet("{surveyId}/evidence")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(List<EvidenceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<EvidenceDto>>> GetSurveyEvidence(
        Guid surveyId,
        [FromQuery] string? evidenceType = null)
    {
        // Parse string to EvidenceType enum (supports both int and name)
        EvidenceType? parsedType = null;
        if (!string.IsNullOrEmpty(evidenceType))
        {
            // Try parse as int first (e.g., "1", "5")
            if (int.TryParse(evidenceType, out var intValue) && Enum.IsDefined(typeof(EvidenceType), intValue))
            {
                parsedType = (EvidenceType)intValue;
            }
            // Try parse as name (e.g., "Photo", "OwnershipDeed")
            else if (Enum.TryParse<EvidenceType>(evidenceType, ignoreCase: true, out var namedValue))
            {
                parsedType = namedValue;
            }
            // Invalid value - you could throw an error here, or just ignore it
        }

        var query = new GetSurveyEvidenceQuery
        {
            SurveyId = surveyId,
            EvidenceType = parsedType
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get evidence details by ID
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 4 - View evidence details
    /// 
    /// **Purpose**: Retrieves complete metadata for a specific evidence file.
    /// 
    /// **What you get**:
    /// - Evidence type and description
    /// - File details (name, size, MIME type)
    /// - File hash for integrity verification
    /// - Upload timestamp
    /// - Links to survey, person, property unit
    /// - Document metadata (issue date, expiry, authority)
    /// - Expiration status
    /// 
    /// **Required permissions**: Authenticated users
    /// </remarks>
    /// <param name="evidenceId">Evidence ID to retrieve</param>
    /// <returns>Evidence details</returns>
    /// <response code="200">Evidence found. Returns complete details.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="404">Evidence not found.</response>
    [HttpGet("evidence/{evidenceId}")]
    [Authorize]
    [ProducesResponseType(typeof(EvidenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvidenceDto>> GetEvidenceById(Guid evidenceId)
    {
        var query = new GetEvidenceByIdQuery { EvidenceId = evidenceId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Download evidence file
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 4 - Download evidence file
    /// 
    /// **Purpose**: Downloads the actual file (photo or document).
    /// 
    /// **What it does**:
    /// - Retrieves file from storage
    /// - Returns file stream with correct MIME type
    /// - Sets download filename
    /// 
    /// **Required permissions**: Authenticated users
    /// 
    /// **Response**: Binary file stream (image or PDF)
    /// </remarks>
    /// <param name="evidenceId">Evidence ID to download</param>
    /// <returns>File stream</returns>
    /// <response code="200">File found. Returns binary stream.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="404">Evidence or file not found.</response>
    [HttpGet("evidence/{evidenceId}/download")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadEvidence(Guid evidenceId)
    {
        var query = new DownloadEvidenceQuery { EvidenceId = evidenceId };
        var result = await _mediator.Send(query);
        return File(result.FileStream, result.MimeType, result.FileName);
    }

    /// <summary>
    /// Delete evidence
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 4 - Remove evidence
    /// 
    /// **Purpose**: Deletes evidence record and physical file (soft delete).
    /// 
    /// **What it does**:
    /// - Marks evidence as deleted (soft delete)
    /// - Removes physical file from storage
    /// - Creates audit trail
    /// 
    /// **When to use**:
    /// - Remove incorrect uploads
    /// - Delete duplicate files
    /// - Remove sensitive information
    /// 
    /// **Important**: Only works for Draft surveys
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// 
    /// **Response**: 204 No Content (success)
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="evidenceId">Evidence ID to delete</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Evidence deleted successfully.</response>
    /// <response code="400">Survey not in Draft status.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only delete from your own surveys.</response>
    /// <response code="404">Survey or evidence not found.</response>
    [HttpDelete("{surveyId}/evidence/{evidenceId}")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvidence(
        Guid surveyId,
        Guid evidenceId)
    {
        var command = new DeleteEvidenceCommand
        {
            SurveyId = surveyId,
            EvidenceId = evidenceId
        };
        await _mediator.Send(command);
        return NoContent();
    }
    // ==================== FIELD SURVEY LIST & QUERY ENDPOINTS ====================

    /// <summary>
    /// Get all field surveys with filtering and pagination
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 - Field Survey listing
    /// 
    /// Returns field surveys only (Type = Field) with optional filtering.
    /// </remarks>
    /// <param name="status">Filter by status (Draft, Completed, Finalized)</param>
    /// <param name="buildingId">Filter by building ID</param>
    /// <param name="fieldCollectorId">Filter by field collector ID</param>
    /// <param name="propertyUnitId">Filter by property unit ID</param>
    /// <param name="fromDate">Filter surveys from this date</param>
    /// <param name="toDate">Filter surveys until this date</param>
    /// <param name="referenceCode">Search by reference code (partial match)</param>
    /// <param name="intervieweeName">Search by interviewee name (partial match)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field (default: SurveyDate)</param>
    /// <param name="sortDirection">Sort direction (default: desc)</param>
    /// <returns>Paginated list of field surveys</returns>
    [HttpGet("field")]
    [Authorize(Policy = "CanViewSurveys")]
    [ProducesResponseType(typeof(GetFieldSurveysResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetFieldSurveysResponse>> GetFieldSurveys(
        [FromQuery] string? status = null,
        [FromQuery] Guid? buildingId = null,
        [FromQuery] Guid? fieldCollectorId = null,
        [FromQuery] Guid? propertyUnitId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? referenceCode = null,
        [FromQuery] string? intervieweeName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "SurveyDate",
        [FromQuery] string sortDirection = "desc")
    {
        var query = new GetFieldSurveysQuery
        {
            Status = status,
            BuildingId = buildingId,
            FieldCollectorId = fieldCollectorId,
            PropertyUnitId = propertyUnitId,
            FromDate = fromDate,
            ToDate = toDate,
            ReferenceCode = referenceCode,
            IntervieweeName = intervieweeName,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get current field collector's draft surveys
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-002 - Resume draft field survey
    /// 
    /// Returns only Draft status surveys belonging to the current user.
    /// </remarks>
    /// <param name="buildingId">Optional: Filter by building</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 50)</param>
    /// <param name="sortBy">Sort field (default: LastModifiedAtUtc)</param>
    /// <param name="sortDirection">Sort direction (default: desc)</param>
    /// <returns>List of draft field surveys</returns>
    [HttpGet("field/drafts")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(GetFieldDraftSurveysResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetFieldDraftSurveysResponse>> GetFieldDraftSurveys(
        [FromQuery] Guid? buildingId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "LastModifiedAtUtc",
        [FromQuery] string sortDirection = "desc")
    {
        var query = new GetFieldDraftSurveysQuery
        {
            BuildingId = buildingId,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get field survey by ID with full details
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001/UC-002 - View or resume field survey
    /// 
    /// Returns complete survey with households, persons, relations, and evidence.
    /// </remarks>
    /// <param name="id">Field survey ID</param>
    /// <param name="includeHouseholds">Include households (default: true)</param>
    /// <param name="includePersons">Include persons (default: true)</param>
    /// <param name="includeRelations">Include relations (default: true)</param>
    /// <param name="includeEvidence">Include evidence (default: true)</param>
    /// <returns>Complete field survey details</returns>
    [HttpGet("field/{id}")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(FieldSurveyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FieldSurveyDetailDto>> GetFieldSurveyById(
        Guid id,
        [FromQuery] bool includeHouseholds = true,
        [FromQuery] bool includePersons = true,
        [FromQuery] bool includeRelations = true,
        [FromQuery] bool includeEvidence = true)
    {
        var query = new GetFieldSurveyByIdQuery
        {
            SurveyId = id,
            IncludeHouseholds = includeHouseholds,
            IncludePersons = includePersons,
            IncludeRelations = includeRelations,
            IncludeEvidence = includeEvidence
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ==================== FIELD SURVEY FINALIZATION ====================

    /// <summary>
    /// Finalize a field survey
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Final Stage - Complete field survey for export
    /// 
    /// Marks survey as Finalized, making it ready for export to .uhc container.
    /// Unlike office surveys, field surveys don't auto-create claims.
    /// </remarks>
    /// <param name="id">Survey ID to finalize</param>
    /// <param name="command">Finalization options</param>
    [HttpPost("field/{id}/finalize")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(FieldSurveyFinalizationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FieldSurveyFinalizationResultDto>> FinalizeFieldSurvey(
        Guid id,
        [FromBody] FinalizeFieldSurveyCommand command)
    {
        command.SurveyId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    /// <summary>
    /// Update person-property relation (partial update - PATCH)
    /// </summary>
    /// <remarks>
    /// Only provided fields are updated. Use Clear* flags to set fields to null.
    /// 
    /// **Enum Values**:
    /// - RelationType: Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99
    /// - ContractType: FullOwnership=1, SharedOwnership=2, LongTermRental=3, ShortTermRental=4, 
    ///                 InformalTenure=5, UnauthorizedOccupation=6, CustomaryRights=7, InheritanceBased=8,
    ///                 HostedGuest=9, TemporaryShelter=10, GovernmentAllocation=11, Usufruct=12, Other=99
    /// 
    /// **Example Request**:
    /// ```json
    /// {
    ///   "contractType": 2,
    ///   "ownershipShare": 0.5,
    ///   "clearNotes": true
    /// }
    /// ```
    /// </remarks>
    [HttpPatch("{surveyId}/relations/{relationId}")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(PersonPropertyRelationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonPropertyRelationDto>> UpdatePersonPropertyRelation(
        Guid surveyId,
        Guid relationId,
        [FromBody] UpdatePersonPropertyRelationCommand command)
    {
        command.SurveyId = surveyId;
        command.RelationId = relationId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete person-property relation
    /// </summary>
    /// <param name="surveyId">Survey ID</param>
    /// <param name="relationId">Relation ID to delete</param>
    /// <param name="deleteEvidenceFiles">Also delete evidence files from storage (default: true)</param>
    [HttpDelete("{surveyId}/relations/{relationId}")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePersonPropertyRelation(
        Guid surveyId,
        Guid relationId,
        [FromQuery] bool deleteEvidenceFiles = true)
    {
        var command = new DeletePersonPropertyRelationCommand
        {
            SurveyId = surveyId,
            RelationId = relationId,
            DeleteEvidenceFiles = deleteEvidenceFiles
        };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Get evidences for a relation (صور المستندات)
    /// </summary>
    /// <remarks>
    /// **EvidenceType Enum Values**:
    /// - IdentificationDocument=1, OwnershipDeed=2, RentalContract=3, UtilityBill=4,
    ///   Photo=5, OfficialLetter=6, CourtOrder=7, InheritanceDocument=8, TaxReceipt=9, Other=99
    /// </remarks>
    /// <param name="surveyId">Survey ID</param>
    /// <param name="relationId">Relation ID</param>
    /// <param name="evidenceType">Optional: Filter by evidence type (integer enum value)</param>
    /// <param name="onlyCurrentVersions">Only current versions (default: true)</param>
    [HttpGet("{surveyId}/relations/{relationId}/evidences")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(List<EvidenceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<EvidenceDto>>> GetEvidencesByRelation(
        Guid surveyId,
        Guid relationId,
        [FromQuery] EvidenceType? evidenceType = null,
        [FromQuery] bool onlyCurrentVersions = true)
    {
        var query = new GetEvidencesByRelationQuery
        {
            SurveyId = surveyId,
            RelationId = relationId,
            EvidenceType = evidenceType,
            OnlyCurrentVersions = onlyCurrentVersions
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

