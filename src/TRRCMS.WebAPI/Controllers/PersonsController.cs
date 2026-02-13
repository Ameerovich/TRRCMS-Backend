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
/// Person management API for individual identity records
/// </summary>
/// <remarks>
/// Manages individual person records for tenure rights documentation.
/// إضافة شخص جديد - إدارة بيانات الأشخاص
/// 
/// **What is a Person?**
/// A Person represents an individual's identity and contact information.
/// Persons can be linked to:
/// - Households (as head or member)
/// - Property units (via PersonPropertyRelation)
/// - Evidence/Documents (for identity verification)
/// 
/// **Person vs Household:**
/// - **Person**: Individual identity (name, national ID, contact)
/// - **Household**: Group demographics (family composition counts)
/// - A Person can be head of a Household
/// - Multiple Persons can belong to one Household
/// 
/// **Syrian Name Structure:**
/// Names follow the Arabic naming convention:
/// - الاسم الأول (First name): Personal given name
/// - اسم الأب (Father's name): Father's first name
/// - الكنية (Family name): Family/tribal/surname
/// - الاسم الأم (Mother's name): Optional, for disambiguation
/// 
/// **Full Name Format:**
/// `{FirstNameArabic} {FatherNameArabic} {FamilyNameArabic}`
/// Example: محمد أحمد الخالد (Mohammed Ahmed Al-Khaled)
/// 
/// **Permissions:**
/// - This controller uses Survey permissions
/// - View: Surveys_ViewAll (7004)
/// - Edit: Surveys_EditAll (7006)
/// 
/// **Alternative Endpoints:**
/// For field collectors working within a survey context:
/// - `POST /api/v1/Surveys/{surveyId}/persons` - Create person in survey
/// - `GET /api/v1/Households/{id}/persons` - Get persons by household
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
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
    /// Create a new person
    /// </summary>
    /// <remarks>
    /// Creates a new person record with identity and contact information.
    /// إضافة شخص جديد
    /// 
    /// **Use Case**: UC-001 Field Survey - Register individuals for tenure documentation
    /// 
    /// **Required Permission**: Surveys_EditAll (7006) - CanEditAllSurveys policy
    /// 
    /// **Form Steps (matching mobile/desktop UI):**
    /// 
    /// **Step 1 - Personal Info (الخطوة الأولى - المعلومات الشخصية):**
    /// | Field | Arabic | Required | Type | Description |
    /// |-------|--------|----------|------|-------------|
    /// | familyNameArabic | الكنية | ❌ No | string | Family/surname |
    /// | firstNameArabic | الاسم الأول | ❌ No | string | Given name |
    /// | fatherNameArabic | اسم الأب | ❌ No | string | Father's name |
    /// | motherNameArabic | الاسم الأم | ❌ No | string | Mother's name |
    /// | nationalId | الرقم الوطني | ❌ No | string | 11-digit national ID |
    /// | gender | الجنس | ❌ No | enum | 1=Male (ذكر), 2=Female (أنثى) |
    /// | nationality | الجنسية | ❌ No | enum | 1=Syrian (سوري), 2=Palestinian (فلسطيني), 3=Iraqi (عراقي), etc. |
    /// | dateOfBirth | تاريخ الميلاد | ❌ No | DateTime | Full date or year-only (e.g., "1985-01-01T00:00:00Z") |
    ///
    /// **Step 2 - Contact Info (الخطوة الثانية - معلومات الاتصال):**
    /// | Field | Arabic | Required | Type | Description |
    /// |-------|--------|----------|------|-------------|
    /// | email | البريد الالكتروني | ❌ No | string | Email address |
    /// | mobileNumber | رقم الموبايل | ❌ No | string | Mobile phone |
    /// | phoneNumber | رقم الهاتف | ❌ No | string | Landline phone |
    ///
    /// **Example Request - Full data:**
    /// ```json
    /// {
    ///   "familyNameArabic": "الخالد",
    ///   "firstNameArabic": "محمد",
    ///   "fatherNameArabic": "أحمد",
    ///   "motherNameArabic": "فاطمة",
    ///   "nationalId": "01234567890",
    ///   "gender": 1,
    ///   "nationality": 1,
    ///   "dateOfBirth": "1985-06-15T00:00:00Z",
    ///   "email": "mohammed.khaled@gmail.com",
    ///   "mobileNumber": "+963 991 234 567",
    ///   "phoneNumber": "021 234 5678"
    /// }
    /// ```
    ///
    /// **Example Request - Minimal data (all fields optional):**
    /// ```json
    /// {
    ///   "firstNameArabic": "أحمد",
    ///   "gender": 1
    /// }
    /// ```
    ///
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "familyNameArabic": "الخالد",
    ///   "firstNameArabic": "محمد",
    ///   "fatherNameArabic": "أحمد",
    ///   "motherNameArabic": "فاطمة",
    ///   "nationalId": "01234567890",
    ///   "gender": "Male",
    ///   "nationality": "Syrian",
    ///   "dateOfBirth": "1985-06-15T00:00:00Z",
    ///   "email": "mohammed.khaled@gmail.com",
    ///   "mobileNumber": "+963 991 234 567",
    ///   "phoneNumber": "021 234 5678",
    ///   "householdId": null,
    ///   "relationshipToHead": null,
    ///   "fullNameArabic": "محمد أحمد الخالد",
    ///   "age": 40,
    ///   "createdAtUtc": "2026-01-31T10:00:00Z",
    ///   "createdBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "lastModifiedAtUtc": null,
    ///   "lastModifiedBy": null,
    ///   "isDeleted": false
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Person creation data</param>
    /// <returns>Created person with generated ID and computed fields</returns>
    /// <response code="201">Person created successfully</response>
    /// <response code="400">Validation error - check field formats (e.g., nationalId must be 11 digits, dateOfBirth cannot be in future)</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Surveys_EditAll permission</response>
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
    /// Update an existing person
    /// </summary>
    /// <remarks>
    /// Updates person details. Only provided fields will be updated (partial update supported).
    /// تعديل بيانات شخص
    /// 
    /// **Use Case**: Correct personal data, update contact information
    /// 
    /// **Required Permission**: Surveys_EditAll (7006) - CanEditAllSurveys policy
    /// 
    /// **Updatable Fields (all optional):**
    /// - Personal: familyNameArabic, firstNameArabic, fatherNameArabic, motherNameArabic
    /// - Identity: nationalId, gender (enum: 1=Male, 2=Female), nationality (enum: 1=Syrian, etc.), dateOfBirth (DateTime)
    /// - Contact: email, mobileNumber, phoneNumber
    ///
    /// **Note:** `householdId` and `relationshipToHead` are managed through
    /// the Household endpoints, not directly on Person.
    ///
    /// **Example Request - Update contact info only:**
    /// ```json
    /// {
    ///   "id": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "email": "new.email@gmail.com",
    ///   "mobileNumber": "+963 992 345 678"
    /// }
    /// ```
    ///
    /// **Example Request - Correct name spelling:**
    /// ```json
    /// {
    ///   "id": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "firstNameArabic": "محمّد",
    ///   "fatherNameArabic": "أحمد"
    /// }
    /// ```
    ///
    /// **Example Request - Update identity information:**
    /// ```json
    /// {
    ///   "id": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "nationalId": "01234567890",
    ///   "gender": 1,
    ///   "nationality": 1,
    ///   "dateOfBirth": "1985-06-15T00:00:00Z"
    /// }
    /// ```
    ///
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "familyNameArabic": "الخالد",
    ///   "firstNameArabic": "محمّد",
    ///   "fatherNameArabic": "أحمد",
    ///   "motherNameArabic": "فاطمة",
    ///   "nationalId": "01234567890",
    ///   "gender": "Male",
    ///   "nationality": "Syrian",
    ///   "dateOfBirth": "1985-06-15T00:00:00Z",
    ///   "email": "new.email@gmail.com",
    ///   "mobileNumber": "+963 992 345 678",
    ///   "phoneNumber": "021 234 5678",
    ///   "householdId": null,
    ///   "relationshipToHead": null,
    ///   "fullNameArabic": "محمّد أحمد الخالد",
    ///   "age": 40,
    ///   "createdAtUtc": "2026-01-31T10:00:00Z",
    ///   "createdBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "lastModifiedAtUtc": "2026-01-31T14:30:00Z",
    ///   "lastModifiedBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "isDeleted": false
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Person ID to update (must match ID in body)</param>
    /// <param name="command">Person update data (only include fields to change)</param>
    /// <returns>Updated person details with computed fields</returns>
    /// <response code="200">Person updated successfully</response>
    /// <response code="400">Validation error or ID mismatch between URL and body</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Surveys_EditAll permission</response>
    /// <response code="404">Person not found</response>
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
    /// Get person by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed information about a specific person.
    /// عرض المعلومات الشخصية
    /// 
    /// **Use Case**: View person details, verify identity information
    /// 
    /// **Required Permission**: Surveys_ViewAll (7004) - CanViewAllSurveys policy
    /// 
    /// **Response includes:**
    /// - Personal identification (names in Arabic, national ID, gender, nationality)
    /// - Contact information (email, phone numbers)
    /// - Date of birth (full date, not just year)
    /// - Household context (if assigned to a household, includes relationshipToHead enum)
    /// - Computed properties:
    ///   - `fullNameArabic`: Concatenated full name
    ///   - `age`: Calculated from dateOfBirth
    /// - Complete audit trail
    ///
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "familyNameArabic": "الخالد",
    ///   "firstNameArabic": "محمد",
    ///   "fatherNameArabic": "أحمد",
    ///   "motherNameArabic": "فاطمة",
    ///   "nationalId": "01234567890",
    ///   "gender": "Male",
    ///   "nationality": "Syrian",
    ///   "dateOfBirth": "1985-06-15T00:00:00Z",
    ///   "email": "mohammed.khaled@gmail.com",
    ///   "mobileNumber": "+963 991 234 567",
    ///   "phoneNumber": "021 234 5678",
    ///   "householdId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "relationshipToHead": "Head",
    ///   "fullNameArabic": "محمد أحمد الخالد",
    ///   "age": 40,
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
    /// <param name="id">Person ID (GUID)</param>
    /// <returns>Person details with computed properties</returns>
    /// <response code="200">Person found and returned</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Surveys_ViewAll permission</response>
    /// <response code="404">Person not found</response>
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
    /// Get all persons
    /// </summary>
    /// <remarks>
    /// Retrieves all persons in the system.
    /// 
    /// **Use Case**: Reporting, data export, administrative review
    /// 
    /// **Required Permission**: Surveys_ViewAll (7004) - CanViewAllSurveys policy
    /// 
    /// **Note**: For large datasets, consider using:
    /// - `GET /api/v1/Households/{id}/persons` - Persons by household
    /// - `GET /api/v1/Surveys/{surveyId}/persons` - Persons in a survey
    /// - Search/filter endpoints (when available)
    /// 
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///     "familyNameArabic": "الخالد",
    ///     "firstNameArabic": "محمد",
    ///     "fatherNameArabic": "أحمد",
    ///     "nationalId": "01234567890",
    ///     "gender": "Male",
    ///     "nationality": "Syrian",
    ///     "dateOfBirth": "1985-06-15T00:00:00Z",
    ///     "fullNameArabic": "محمد أحمد الخالد",
    ///     "age": 40,
    ///     "householdId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "relationshipToHead": "Head",
    ///     "createdAtUtc": "2026-01-31T10:00:00Z"
    ///   },
    ///   {
    ///     "id": "8cd03f62-9345-5234-b2cd-0e963g44bgc8",
    ///     "familyNameArabic": "العلي",
    ///     "firstNameArabic": "فاطمة",
    ///     "fatherNameArabic": "خالد",
    ///     "nationalId": null,
    ///     "gender": "Female",
    ///     "nationality": "Syrian",
    ///     "dateOfBirth": "1990-03-22T00:00:00Z",
    ///     "fullNameArabic": "فاطمة خالد العلي",
    ///     "age": 35,
    ///     "householdId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "relationshipToHead": "Spouse",
    ///     "createdAtUtc": "2026-01-31T10:30:00Z"
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <returns>List of all persons</returns>
    /// <response code="200">Success - returns array of persons (may be empty)</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Surveys_ViewAll permission</response>
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