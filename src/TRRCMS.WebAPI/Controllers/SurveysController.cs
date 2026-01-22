using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Application.Surveys.Commands.AddPersonToHousehold;
using TRRCMS.Application.Surveys.Commands.CreateFieldSurvey;
using TRRCMS.Application.Surveys.Commands.CreateHouseholdInSurvey;
using TRRCMS.Application.Surveys.Commands.CreatePropertyUnitInSurvey;
using TRRCMS.Application.Surveys.Commands.DeleteEvidence;
using TRRCMS.Application.Surveys.Commands.LinkPersonToPropertyUnit;
using TRRCMS.Application.Surveys.Commands.LinkPropertyUnitToSurvey;
using TRRCMS.Application.Surveys.Commands.SaveDraftSurvey;
using TRRCMS.Application.Surveys.Commands.SetHouseholdHead;
using TRRCMS.Application.Surveys.Commands.UpdatePropertyUnitInSurvey;
using TRRCMS.Application.Surveys.Commands.UploadIdentificationDocument;
using TRRCMS.Application.Surveys.Commands.UploadPropertyPhoto;
using TRRCMS.Application.Surveys.Commands.UploadTenureDocument;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Application.Surveys.Queries.DownloadEvidence;
using TRRCMS.Application.Surveys.Queries.GetDraftSurvey;
using TRRCMS.Application.Surveys.Queries.GetEvidenceById;
using TRRCMS.Application.Surveys.Queries.GetHouseholdInSurvey;
using TRRCMS.Application.Surveys.Queries.GetHouseholdPersons;
using TRRCMS.Application.Surveys.Queries.GetPropertyUnitsForSurvey;
using TRRCMS.Application.Surveys.Queries.GetSurveyEvidence;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Surveys controller for field and office survey operations
/// Supports UC-001 (Field Survey) and UC-004 (Office Survey)
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SurveysController : ControllerBase
{
    private readonly IMediator _mediator;

    public SurveysController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    // ==================== SURVEY MANAGEMENT ====================

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
    /// **When to use**:
    /// - When starting a new field survey for a building
    /// - At the beginning of the field data collection process
    /// 
    /// **Required permissions**: CanCreateSurveys
    /// 
    /// **Example**:
    /// ```json
    /// {
    ///   "buildingId": "12345678-1234-1234-1234-123456789012",
    ///   "surveyDate": "2026-01-20T10:00:00Z",
    ///   "gpsCoordinates": "36.2021,37.1343",
    ///   "intervieweeName": "محمد أحمد السيد",
    ///   "intervieweeRelationship": "مالك العقار",
    ///   "notes": "First field survey for this building"
    /// }
    /// ```
    /// 
    /// **Response**: Returns the created survey with auto-generated reference code
    /// 
    /// **Next steps**: 
    /// 1. Use the returned survey ID to create/link property units
    /// 2. Continue with household and person data collection
    /// 3. Save draft periodically to preserve work
    /// </remarks>
    /// <param name="command">Field survey creation data</param>
    /// <returns>Created survey with reference code and status</returns>
    /// <response code="201">Survey created successfully. Returns survey details with reference code.</response>
    /// <response code="400">Invalid input. Check building ID, GPS format, or date validity.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. User lacks CanCreateSurveys permission.</response>
    /// <response code="404">Building not found. Verify the building ID exists.</response>
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

    /// <summary>
    /// Save survey progress as draft
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-002 - Save draft and exit safely
    /// 
    /// **Purpose**: Saves current survey progress without finalizing. Allows field collectors to safely exit and resume later.
    /// 
    /// **What it does**:
    /// - Updates survey fields with current progress
    /// - Keeps survey in Draft status
    /// - Records survey duration
    /// - Preserves all entered data
    /// - Updates last modified timestamp
    /// 
    /// **When to use**:
    /// - Periodically during data collection to prevent data loss
    /// - When pausing work to resume later
    /// - When network connectivity is restored after offline work
    /// - Before closing the application
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// 
    /// **Example**:
    /// ```json
    /// {
    ///   "notes": "Updated survey notes after visiting 3 units",
    ///   "durationMinutes": 45,
    ///   "gpsCoordinates": "36.2025,37.1350"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Survey ID to update</param>
    /// <param name="command">Draft update data (all fields optional)</param>
    /// <returns>Updated survey details</returns>
    /// <response code="200">Draft saved successfully. Returns updated survey.</response>
    /// <response code="400">Invalid input or survey not in Draft status.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only edit your own surveys.</response>
    /// <response code="404">Survey not found. Verify the survey ID.</response>
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
    /// Get survey by ID
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-002 - Resume draft survey
    /// 
    /// **Purpose**: Retrieves complete survey details to resume work or view progress.
    /// 
    /// **What it does**:
    /// - Fetches complete survey information
    /// - Includes building details (number, address)
    /// - Includes linked property unit information (if any)
    /// - Shows current survey status
    /// - Displays field collector name
    /// - Shows all timestamps (created, modified)
    /// 
    /// **Required permissions**: CanViewOwnSurveys
    /// </remarks>
    /// <param name="id">Survey ID to retrieve</param>
    /// <returns>Complete survey details</returns>
    /// <response code="200">Survey found. Returns complete survey details.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only view your own surveys.</response>
    /// <response code="404">Survey not found. Verify the survey ID.</response>
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

    /// <summary>
    /// Get all property units for survey's building
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 2 - View available property units
    /// 
    /// **Purpose**: Lists all property units in the building being surveyed.
    /// 
    /// **Required permissions**: CanViewOwnSurveys
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
    /// **Use Case**: UC-001 Stage 2 - Create new property unit during survey
    /// 
    /// **Purpose**: Creates a new property unit and automatically links it to the survey.
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// </remarks>
    /// <param name="surveyId">Survey ID to create the property unit for</param>
    /// <param name="command">Property unit creation data</param>
    /// <returns>Created property unit details</returns>
    /// <response code="201">Property unit created successfully and linked to survey.</response>
    /// <response code="400">Invalid input. Check validation rules and duplicate identifiers.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only create units for your own surveys.</response>
    /// <response code="404">Survey or building not found.</response>
    [HttpPost("{surveyId}/property-units")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(PropertyUnitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyUnitDto>> CreatePropertyUnitInSurvey(
        Guid surveyId,
        [FromBody] CreatePropertyUnitInSurveyCommand command)
    {
        command.SurveyId = surveyId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(
            nameof(GetSurvey),
            new { id = surveyId },
            result);
    }

    /// <summary>
    /// Update property unit in survey context
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 2 - Update property unit details during survey
    /// 
    /// **Purpose**: Updates existing property unit details.
    /// 
    /// **Required permissions**: CanEditOwnSurveys
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
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Link existing property unit to survey
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 2 - Select existing property unit
    /// 
    /// **Purpose**: Links an existing property unit to the survey.
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// </remarks>
    /// <param name="surveyId">Survey ID to link the property unit to</param>
    /// <param name="command">Link command with property unit ID</param>
    /// <returns>Updated survey with linked property unit</returns>
    /// <response code="200">Property unit linked successfully to survey.</response>
    /// <response code="400">Invalid input or unit doesn't belong to survey's building.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only link units to your own surveys.</response>
    /// <response code="404">Survey or property unit not found.</response>
    [HttpPost("{surveyId}/link-property-unit")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDto>> LinkPropertyUnitToSurvey(
        Guid surveyId,
        [FromBody] LinkPropertyUnitToSurveyCommand command)
    {
        command.SurveyId = surveyId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==================== HOUSEHOLD MANAGEMENT (DAY 3) ====================

    /// <summary>
    /// Create household in survey context
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 3 - Household Registration
    /// 
    /// **Purpose**: Creates a household for a property unit and collects demographic composition data.
    /// 
    /// **What it does**:
    /// - Creates household record linked to property unit
    /// - Records head of household name
    /// - Captures household size
    /// - Records demographic composition (gender, age groups)
    /// - Captures vulnerability indicators
    /// - Records economic indicators
    /// - Captures displacement information
    /// 
    /// **When to use**:
    /// - After selecting/creating a property unit
    /// - When registering occupants of a unit
    /// - As part of complete building survey
    /// 
    /// **Required fields**:
    /// - propertyUnitId: Unit this household occupies
    /// - headOfHouseholdName: Name of household head
    /// - householdSize: Total number of members
    /// 
    /// **Optional demographic data**:
    /// - Gender composition (maleCount, femaleCount)
    /// - Age breakdown (infantCount, childCount, minorCount, adultCount, elderlyCount)
    /// - Vulnerability indicators (personsWithDisabilities, isFemaleHeaded, widows, orphans)
    /// - Economic data (employed, unemployed, income source)
    /// - Displacement info (isDisplaced, origin, arrival date)
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// 
    /// **Example**:
    /// ```json
    /// {
    ///   "propertyUnitId": "unit-guid-here",
    ///   "headOfHouseholdName": "أحمد محمد حسن",
    ///   "householdSize": 6,
    ///   "maleCount": 3,
    ///   "femaleCount": 3,
    ///   "adultCount": 2,
    ///   "childCount": 4,
    ///   "isFemaleHeaded": false,
    ///   "employedPersonsCount": 1
    /// }
    /// ```
    /// 
    /// **Response**: Returns created household with all demographic data
    /// 
    /// **Next steps**: 
    /// - Add individual persons: POST /{surveyId}/households/{householdId}/persons
    /// - Set household head: POST /{surveyId}/households/{householdId}/set-head
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="command">Household creation data</param>
    /// <returns>Created household details</returns>
    /// <response code="201">Household created successfully.</response>
    /// <response code="400">Invalid input or property unit doesn't belong to survey's building.</response>
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
    /// **Purpose**: Retrieves complete household details including all demographic composition.
    /// 
    /// **What you get**:
    /// - Household identification (ID, head name, size)
    /// - Gender composition (male/female counts)
    /// - Age breakdown (infants, children, minors, adults, elderly)
    /// - Vulnerability indicators (disabilities, female-headed, widows, orphans)
    /// - Economic indicators (employment, income)
    /// - Displacement information
    /// - Computed metrics (dependency ratio, vulnerability flag)
    /// 
    /// **Required permissions**: CanViewOwnSurveys
    /// 
    /// **Response**: Complete household data with computed properties
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
    /// **Example**:
    /// ```json
    /// {
    ///   "personId": "person-guid-here"
    /// }
    /// ```
    /// 
    /// **Response**: Returns updated household with linked head
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="householdId">Household ID</param>
    /// <param name="command">Command with person ID</param>
    /// <returns>Updated household details</returns>
    /// <response code="200">Household head set successfully.</response>
    /// <response code="400">Person doesn't belong to this household.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only modify your own surveys.</response>
    /// <response code="404">Survey, household, or person not found.</response>
    [HttpPost("{surveyId}/households/{householdId}/set-head")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(HouseholdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdDto>> SetHouseholdHead(
        Guid surveyId,
        Guid householdId,
        [FromBody] SetHouseholdHeadCommand command)
    {
        command.SurveyId = surveyId;
        command.HouseholdId = householdId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==================== PERSON MANAGEMENT (DAY 3) ====================

    /// <summary>
    /// Add person to household
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 3 - Person Registration
    /// 
    /// **Purpose**: Registers an individual person as member of a household.
    /// 
    /// **What it does**:
    /// - Creates person record with Arabic 3-part name
    /// - Links person to household
    /// - Records relationship to household head
    /// - Captures identification and demographics
    /// - Records contact information
    /// 
    /// **When to use**:
    /// - After creating household
    /// - For each household member
    /// - To register household head and family members
    /// 
    /// **Required fields (Arabic names)**:
    /// - firstNameArabic: First name (الاسم الأول)
    /// - fatherNameArabic: Father's name (اسم الأب)
    /// - familyNameArabic: Family name (اسم العائلة)
    /// 
    /// **Optional fields**:
    /// - motherNameArabic: Mother's name (اسم الأم)
    /// - fullNameEnglish: English name
    /// - nationalId: Identification number
    /// - yearOfBirth: Birth year
    /// - gender: M/F or ذكر/أنثى
    /// - nationality: Nationality
    /// - relationshipToHead: "Head", "Spouse", "Son", "Daughter", etc.
    /// - primaryPhoneNumber: Contact number
    /// - isContactPerson: Is main contact (true/false)
    /// 
    /// **Arabic naming system**:
    /// - Format: [First] [Father] [Family] ([Mother])
    /// - Example: أحمد محمد حسن (فاطمة)
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// 
    /// **Example**:
    /// ```json
    /// {
    ///   "firstNameArabic": "أحمد",
    ///   "fatherNameArabic": "محمد",
    ///   "familyNameArabic": "حسن",
    ///   "motherNameArabic": "فاطمة",
    ///   "yearOfBirth": 1985,
    ///   "gender": "M",
    ///   "nationality": "Syrian",
    ///   "relationshipToHead": "Head",
    ///   "primaryPhoneNumber": "+963123456789",
    ///   "isContactPerson": true
    /// }
    /// ```
    /// 
    /// **Response**: Returns created person with computed full name and age
    /// 
    /// **Next steps**: 
    /// - Add more household members
    /// - Set as household head: POST /set-head
    /// - Link to property unit: POST /persons/{personId}/link-to-unit
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="householdId">Household ID to add person to</param>
    /// <param name="command">Person creation data</param>
    /// <returns>Created person details</returns>
    /// <response code="201">Person added to household successfully.</response>
    /// <response code="400">Invalid input. Check required Arabic names.</response>
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
    /// 
    /// **Purpose**: Lists all registered persons/members in a household.
    /// 
    /// **What you get**:
    /// - List of all household members
    /// - Full Arabic names (3-part + mother's name)
    /// - Computed full names and ages
    /// - Demographics (gender, nationality, birth year)
    /// - Relationships to household head
    /// - Contact information
    /// - Identification details
    /// 
    /// **Required permissions**: CanViewOwnSurveys
    /// 
    /// **Response**: Array of persons ordered by creation date
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="householdId">Household ID to get members for</param>
    /// <returns>List of household members</returns>
    /// <response code="200">Success. Returns array of persons (may be empty).</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only view members for your own surveys.</response>
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
    /// Link person to property unit
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
    /// - Owner: Legal owner
    /// - Tenant: Renter/tenant
    /// - Occupant: Current occupant
    /// - Claimant: Claiming ownership/rights
    /// - Co-owner: Shared ownership
    /// 
    /// **Important**: 
    /// - Person and property unit must be in same building
    /// - Each person-unit pair can only have one relation
    /// - Used for later claims processing
    /// 
    /// **Required permissions**: CanEditOwnSurveys
    /// 
    /// **Example**:
    /// ```json
    /// {
    ///   "propertyUnitId": "unit-guid-here",
    ///   "relationType": "Owner"
    /// }
    /// ```
    /// 
    /// **Response**: 204 No Content (success)
    /// </remarks>
    /// <param name="surveyId">Survey ID for authorization</param>
    /// <param name="personId">Person ID to link</param>
    /// <param name="command">Link command with property unit ID and relation type</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Person linked to property unit successfully.</response>
    /// <response code="400">Person already linked to this unit or unit not in survey building.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Can only link persons in your own surveys.</response>
    /// <response code="404">Survey, person, or property unit not found.</response>
    [HttpPost("{surveyId}/persons/{personId}/link-to-unit")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkPersonToPropertyUnit(
        Guid surveyId,
        Guid personId,
        [FromBody] LinkPersonToPropertyUnitCommand command)
    {
        command.SurveyId = surveyId;
        command.PersonId = personId;
        await _mediator.Send(command);
        return NoContent();
    }
    // ==================== EVIDENCE MANAGEMENT (DAY 4) ====================

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
    /// - Format: JPG, JPEG, PNG
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
    [HttpPost("{surveyId}/evidence/property-photo")]
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
        return CreatedAtAction(
            nameof(GetEvidenceById),
            new { evidenceId = result.Id },
            result);
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
    [HttpPost("{surveyId}/evidence/identification/{personId}")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(EvidenceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvidenceDto>> UploadIdentificationDocument(
        Guid surveyId,
        Guid personId,
        [FromForm] UploadIdentificationDocumentCommand command)
    {
        command.SurveyId = surveyId;
        command.PersonId = personId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(
            nameof(GetEvidenceById),
            new { evidenceId = result.Id },
            result);
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
        return CreatedAtAction(
            nameof(GetEvidenceById),
            new { evidenceId = result.Id },
            result);
    }

    /// <summary>
    /// Get all evidence for survey
    /// </summary>
    /// <remarks>
    /// **Use Case**: UC-001 Stage 4 - View collected evidence
    /// 
    /// **Purpose**: Lists all photos and documents uploaded for the survey.
    /// 
    /// **What you get**:
    /// - All property photos
    /// - All identification documents
    /// - All tenure documents
    /// - File metadata (size, type, hash)
    /// - Upload timestamps
    /// - Links to persons/units
    /// 
    /// **Optional Filtering**:
    /// - evidenceType: Filter by type (PropertyPhoto, IdentificationDocument, TenureDocument)
    /// 
    /// **Required permissions**: CanViewOwnSurveys
    /// 
    /// **Response**: Array of evidence records ordered by upload date
    /// </remarks>
    /// <param name="surveyId">Survey ID to get evidence for</param>
    /// <param name="evidenceType">Optional filter by evidence type</param>
    /// <returns>List of evidence records</returns>
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
        var query = new GetSurveyEvidenceQuery
        {
            SurveyId = surveyId,
            EvidenceType = evidenceType
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
}