using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Persons.Commands.CreatePerson;
using TRRCMS.Application.Persons.Commands.UpdatePerson;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.Persons.Queries.GetAllPersons;
using TRRCMS.Application.Persons.Queries.GetPerson;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Persons Management API (Admin/Data Manager access)
/// إضافة شخص جديد - إدارة بيانات الأشخاص
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class PersonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PersonsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ==================== CREATE ====================

    /// <summary>
    /// Create a new person (Admin/Data Manager)
    /// </summary>
    /// <remarks>
    /// **Purpose**: Creates a new person record.
    /// إضافة شخص جديد
    /// 
    /// **Required Permission**: Surveys_EditAll (CanEditAllSurveys)
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
    ///   "phoneNumber": "0000000"
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Person creation data</param>
    /// <returns>Created person with generated ID</returns>
    /// <response code="201">Person created successfully.</response>
    /// <response code="400">Validation error. Check required fields.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires Surveys_EditAll permission.</response>
    [HttpPost]
    [Authorize(Policy = "CanEditAllSurveys")]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PersonDto>> CreatePerson([FromBody] CreatePersonCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPerson), new { id = result.Id }, result);
    }

    // ==================== UPDATE ====================

    /// <summary>
    /// Update an existing person (Admin/Data Manager)
    /// </summary>
    /// <remarks>
    /// **Purpose**: Updates person details. Only provided fields will be updated.
    /// تعديل بيانات شخص
    /// 
    /// **Required Permission**: Surveys_EditAll (CanEditAllSurveys)
    /// 
    /// **Example Request** (partial update):
    /// ```json
    /// {
    ///   "id": "person-guid-here",
    ///   "email": "newemail@gmail.com",
    ///   "mobileNumber": "+963 099"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Person ID to update</param>
    /// <param name="command">Person update data (all fields optional)</param>
    /// <returns>Updated person details</returns>
    /// <response code="200">Person updated successfully.</response>
    /// <response code="400">Validation error or ID mismatch.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires Surveys_EditAll permission.</response>
    /// <response code="404">Person not found.</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanEditAllSurveys")]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonDto>> UpdatePerson(Guid id, [FromBody] UpdatePersonCommand command)
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
    /// Get person by ID (Admin/Data Manager/Supervisor)
    /// </summary>
    /// <remarks>
    /// **Purpose**: Retrieves detailed information about a specific person.
    /// عرض المعلومات الشخصية
    /// 
    /// **Required Permission**: Surveys_ViewAll (CanViewAllSurveys)
    /// 
    /// **Response includes**:
    /// - Personal info (names, national ID, year of birth)
    /// - Contact info (email, mobile, phone)
    /// - Household context (if assigned)
    /// - Computed properties (full name, age)
    /// - Audit timestamps
    /// </remarks>
    /// <param name="id">Person ID (GUID)</param>
    /// <returns>Person details</returns>
    /// <response code="200">Person found and returned.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires Surveys_ViewAll permission.</response>
    /// <response code="404">Person not found.</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewAllSurveys")]
    [ProducesResponseType(typeof(PersonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonDto>> GetPerson(Guid id)
    {
        var query = new GetPersonQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = $"Person with ID {id} not found" });
        }

        return Ok(result);
    }

    // ==================== GET ALL ====================

    /// <summary>
    /// Get all persons (Admin/Data Manager/Supervisor)
    /// </summary>
    /// <remarks>
    /// **Purpose**: Retrieves all persons in the system.
    /// 
    /// **Required Permission**: Surveys_ViewAll (CanViewAllSurveys)
    /// 
    /// **Note**: For large datasets, consider using filtered endpoints.
    /// </remarks>
    /// <returns>List of all persons</returns>
    /// <response code="200">Success. Returns array of persons (may be empty).</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. Requires Surveys_ViewAll permission.</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllSurveys")]
    [ProducesResponseType(typeof(List<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<PersonDto>>> GetAllPersons()
    {
        var query = new GetAllPersonsQuery();
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}