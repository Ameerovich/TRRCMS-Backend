using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.PersonPropertyRelations.Commands.CreatePersonPropertyRelation;
using TRRCMS.Application.PersonPropertyRelations.Commands.DeletePersonPropertyRelation;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Application.PersonPropertyRelations.Queries.GetAllPersonPropertyRelations;
using TRRCMS.Application.PersonPropertyRelations.Queries.GetPersonPropertyRelation;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Person-Property Relations management API
/// </summary>
/// <remarks>
/// Manages the relationships between persons and property units for tenure rights tracking.
/// علاقة الشخص بالعقار - تسجيل حقوق الحيازة
/// 
/// **What is a Person-Property Relation?**
/// A record that links a Person to a PropertyUnit with a specific relationship type
/// (owner, tenant, occupant, etc.) and contract type. This is the core of tenure
/// rights documentation in TRRCMS.
/// 
/// **Key Concepts:**
/// - **RelationType**: The nature of the person's connection to the property
/// - **ContractType**: The legal/formal basis of the tenure
/// - **OwnershipShare**: For owners, the percentage of ownership (0.0-1.0)
/// 
/// **RelationType Values (نوع العلاقة):**
/// 
/// | Value | Name | Arabic | Description |
/// |-------|------|--------|-------------|
/// | 1 | Owner | مالك | Legal property owner |
/// | 2 | Occupant | ساكن | Current occupant without formal tenure |
/// | 3 | Tenant | مستأجر | Renter with lease agreement |
/// | 4 | Guest | ضيف | Temporary guest/visitor |
/// | 5 | Heir | وريث | Inherited ownership rights |
/// | 99 | Other | أخرى | Other (requires `relationTypeOtherDesc`) |
/// 
/// **TenureContractType Values (نوع العقد):**
/// 
/// | Value | Name | Arabic | Description |
/// |-------|------|--------|-------------|
/// | 1 | FullOwnership | ملكية تامة | Complete property ownership |
/// | 2 | SharedOwnership | ملكية مشتركة | Partial ownership with others |
/// | 3 | LongTermRental | إيجار طويل الأمد | Formal long-term lease |
/// | 4 | ShortTermRental | إيجار قصير الأمد | Temporary rental |
/// | 5 | InformalTenure | حيازة غير رسمية | No formal documentation |
/// | 6 | UnauthorizedOccupation | إشغال غير مصرح | Squatter/unauthorized |
/// | 7 | CustomaryRights | حقوق عرفية | Traditional/customary rights |
/// | 8 | InheritanceBased | ميراث | Inherited property |
/// | 9 | HostedGuest | استضافة | Staying as guest |
/// | 10 | TemporaryShelter | مأوى مؤقت | Emergency accommodation |
/// | 11 | GovernmentAllocation | تخصيص حكومي | Government-allocated housing |
/// | 12 | Usufruct | حق الانتفاع | Right to use property |
/// | 99 | Other | أخرى | Other (requires `contractTypeOtherDesc`) |
/// 
/// **Validation Rules:**
/// - `relationType` = 1 (Owner) → `ownershipShare` is required
/// - `relationType` = 99 (Other) → `relationTypeOtherDesc` is required
/// - `contractType` = 99 (Other) → `contractTypeOtherDesc` is required
/// - `ownershipShare` must be between 0.0 and 1.0
/// 
/// **Alternative Endpoint:**
/// For field collectors working within a survey context, use:
/// `POST /api/v1/Surveys/{surveyId}/property-units/{unitId}/relations`
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class PersonPropertyRelationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PersonPropertyRelationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new person-property relation
    /// </summary>
    /// <remarks>
    /// Creates a link between a person and a property unit with specified relationship
    /// and contract type. This establishes the tenure rights record.
    /// 
    /// **Use Case**: UC-001 Field Survey Stage 3 - Establish person-property relationship
    /// 
    /// **Required Permission**: Surveys_EditAll (7006) or survey-specific edit permission
    /// 
    /// **Required Fields:**
    /// - `personId`: The person being linked
    /// - `propertyUnitId`: The property unit
    /// - `relationType`: Type of relationship (1-5, 99)
    /// 
    /// **Conditional Requirements:**
    /// - If `relationType` = 1 (Owner): `ownershipShare` is required
    /// - If `relationType` = 99 (Other): `relationTypeOtherDesc` is required
    /// - If `contractType` = 99 (Other): `contractTypeOtherDesc` is required
    /// 
    /// **Example - Owner with full ownership:**
    /// ```json
    /// {
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "relationType": 1,
    ///   "contractType": 1,
    ///   "ownershipShare": 1.0,
    ///   "startDate": "2015-03-20T00:00:00Z",
    ///   "notes": "مالك العقار الأصلي - طابو أخضر"
    /// }
    /// ```
    /// 
    /// **Example - Owner with shared ownership (50%):**
    /// ```json
    /// {
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "relationType": 1,
    ///   "contractType": 2,
    ///   "ownershipShare": 0.5,
    ///   "startDate": "2020-01-15T00:00:00Z",
    ///   "notes": "ملكية مشتركة مع الأخ - 50%"
    /// }
    /// ```
    /// 
    /// **Example - Tenant with long-term rental:**
    /// ```json
    /// {
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "relationType": 3,
    ///   "contractType": 3,
    ///   "startDate": "2024-01-01T00:00:00Z",
    ///   "endDate": "2025-12-31T00:00:00Z",
    ///   "contractDetails": "عقد إيجار سنوي - رقم العقد: RC-2024-001",
    ///   "notes": "مستأجر بعقد رسمي"
    /// }
    /// ```
    /// 
    /// **Example - Heir with inheritance:**
    /// ```json
    /// {
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "relationType": 5,
    ///   "contractType": 8,
    ///   "ownershipShare": 0.25,
    ///   "startDate": "2023-06-15T00:00:00Z",
    ///   "notes": "ورث 25% من العقار عن الأب"
    /// }
    /// ```
    /// 
    /// **Example - Other relation type:**
    /// ```json
    /// {
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "relationType": 99,
    ///   "relationTypeOtherDesc": "مدير العقار",
    ///   "contractType": 99,
    ///   "contractTypeOtherDesc": "عقد إدارة عقارات",
    ///   "startDate": "2024-01-01T00:00:00Z"
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "d6ad6c6f-9e89-4190-930d-c6d3ab7b8f8d",
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "relationType": 1,
    ///   "relationTypeOtherDesc": null,
    ///   "contractType": 1,
    ///   "contractTypeOtherDesc": null,
    ///   "ownershipShare": 1.0,
    ///   "contractDetails": null,
    ///   "startDate": "2015-03-20T00:00:00Z",
    ///   "endDate": null,
    ///   "notes": "مالك العقار الأصلي - طابو أخضر",
    ///   "isActive": true,
    ///   "createdAtUtc": "2026-01-31T10:00:00Z",
    ///   "createdBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "durationInDays": 3970,
    ///   "isOngoing": true,
    ///   "evidenceCount": 0
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Person-property relation creation data</param>
    /// <returns>Created relation with computed fields</returns>
    /// <response code="201">Relation created successfully</response>
    /// <response code="400">Validation error - check required fields and enum values</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - missing required permission</response>
    /// <response code="404">Person or PropertyUnit not found</response>
    [HttpPost]
    [Authorize(Policy = "CanEditAllSurveys")]
    [ProducesResponseType(typeof(PersonPropertyRelationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonPropertyRelationDto>> Create([FromBody] CreatePersonPropertyRelationCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get all person-property relations
    /// </summary>
    /// <remarks>
    /// Retrieves all person-property relations in the system.
    /// 
    /// **Use Case**: Administrative review, reporting, data export
    /// 
    /// **Required Permission**: Surveys_ViewAll (7004) - CanViewAllSurveys policy
    /// 
    /// **Response includes:**
    /// - Relation details (person, property unit, type, contract)
    /// - Ownership share (for owners)
    /// - Date range (start/end dates)
    /// - Computed fields (durationInDays, isOngoing, evidenceCount)
    /// - Audit trail
    /// 
    /// **Note**: For large datasets, consider using filtered endpoints or
    /// retrieving relations through parent entities.
    /// 
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "d6ad6c6f-9e89-4190-930d-c6d3ab7b8f8d",
    ///     "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///     "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "relationType": 1,
    ///     "contractType": 1,
    ///     "ownershipShare": 1.0,
    ///     "startDate": "2015-03-20T00:00:00Z",
    ///     "isActive": true,
    ///     "durationInDays": 3970,
    ///     "isOngoing": true,
    ///     "evidenceCount": 2
    ///   },
    ///   {
    ///     "id": "e7be7d7g-0f90-5201-a41e-d7e4bc8c9g9e",
    ///     "personId": "8cd03f62-9345-5234-b2cd-0e963g44bgc8",
    ///     "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "relationType": 3,
    ///     "contractType": 3,
    ///     "ownershipShare": null,
    ///     "startDate": "2024-01-01T00:00:00Z",
    ///     "endDate": "2024-12-31T00:00:00Z",
    ///     "isActive": true,
    ///     "durationInDays": 365,
    ///     "isOngoing": false,
    ///     "evidenceCount": 1
    ///   }
    /// ]
    /// ```
    /// 
    /// **RelationType Quick Reference:**
    /// 1=Owner, 2=Occupant, 3=Tenant, 4=Guest, 5=Heir, 99=Other
    /// 
    /// **ContractType Quick Reference:**
    /// 1=FullOwnership, 2=SharedOwnership, 3=LongTermRental, 4=ShortTermRental,
    /// 5=InformalTenure, 6=UnauthorizedOccupation, 7=CustomaryRights,
    /// 8=InheritanceBased, 9=HostedGuest, 10=TemporaryShelter,
    /// 11=GovernmentAllocation, 12=Usufruct, 99=Other
    /// </remarks>
    /// <returns>List of all person-property relations</returns>
    /// <response code="200">Relations retrieved successfully</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - missing required permission</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllSurveys")]
    [ProducesResponseType(typeof(PagedResult<PersonPropertyRelationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<PersonPropertyRelationDto>>> GetAll([FromQuery] GetAllPersonPropertyRelationsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get person-property relation by ID
    /// </summary>
    /// <remarks>
    /// Retrieves a single person-property relation with all details.
    /// 
    /// **Use Case**: View relation details, verify tenure documentation
    /// 
    /// **Required Permission**: Surveys_ViewAll (7004) - CanViewAllSurveys policy
    /// 
    /// **Response includes:**
    /// - Full relation details
    /// - Ownership share (for owners)
    /// - Contract details and date range
    /// - Computed fields:
    ///   - `durationInDays`: Days since start date (or between start and end)
    ///   - `isOngoing`: True if no end date or end date is in future
    ///   - `evidenceCount`: Number of linked evidence records
    /// - Complete audit trail
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "d6ad6c6f-9e89-4190-930d-c6d3ab7b8f8d",
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "propertyUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "relationType": 1,
    ///   "relationTypeOtherDesc": null,
    ///   "contractType": 1,
    ///   "contractTypeOtherDesc": null,
    ///   "ownershipShare": 1.0,
    ///   "contractDetails": "طابو أخضر رقم 123456",
    ///   "startDate": "2015-03-20T00:00:00Z",
    ///   "endDate": null,
    ///   "notes": "مالك العقار الأصلي",
    ///   "isActive": true,
    ///   "createdAtUtc": "2026-01-31T10:00:00Z",
    ///   "createdBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "lastModifiedAtUtc": null,
    ///   "lastModifiedBy": null,
    ///   "isDeleted": false,
    ///   "deletedAtUtc": null,
    ///   "deletedBy": null,
    ///   "durationInDays": 3970,
    ///   "isOngoing": true,
    ///   "evidenceCount": 2
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Relation GUID</param>
    /// <returns>Person-property relation details</returns>
    /// <response code="200">Relation found and returned</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - missing required permission</response>
    /// <response code="404">Relation not found with the specified ID</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewAllSurveys")]
    [ProducesResponseType(typeof(PersonPropertyRelationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonPropertyRelationDto>> GetById(Guid id)
    {
        var query = new GetPersonPropertyRelationQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Person-property relation with ID {id} not found" });

        return Ok(result);
    }

    // ==================== DELETE ====================

    /// <summary>
    /// Soft delete a person-property relation and all related evidences
    /// حذف علاقة الشخص بالعقار مع جميع المستندات المرتبطة
    /// </summary>
    /// <remarks>
    /// **Use Case**: Remove a relation that was created by mistake or is no longer valid
    ///
    /// **Required Permission**: Surveys_EditAll (7006) - CanEditAllSurveys policy
    ///
    /// **Cascade Delete Behavior**:
    /// This operation will soft delete:
    /// - The PersonPropertyRelation itself
    /// - All Evidences linked to this relation (ownership deeds, rental contracts, etc.)
    ///
    /// **Important**: Only works when the related survey is in **Draft** status.
    /// If the survey is Finalized or Completed, the delete will be rejected.
    ///
    /// **Example Request**:
    /// ```
    /// DELETE /api/v1/PersonPropertyRelations/d6ad6c6f-9e89-4190-930d-c6d3ab7b8f8d
    /// ```
    ///
    /// **Example Response**:
    /// ```json
    /// {
    ///   "primaryEntityId": "d6ad6c6f-9e89-4190-930d-c6d3ab7b8f8d",
    ///   "primaryEntityType": "PersonPropertyRelation",
    ///   "affectedEntities": [
    ///     { "entityId": "d6ad6c6f-...", "entityType": "PersonPropertyRelation", "entityIdentifier": "Relation Owner" },
    ///     { "entityId": "e7be7d7g-...", "entityType": "Evidence", "entityIdentifier": "tabu_green.pdf" },
    ///     { "entityId": "f8cf8e8h-...", "entityType": "Evidence", "entityIdentifier": "rental_contract.pdf" }
    ///   ],
    ///   "totalAffected": 3,
    ///   "deletedAtUtc": "2026-02-14T10:00:00Z",
    ///   "message": "PersonPropertyRelation deleted successfully along with 2 evidence(s)"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Relation ID (GUID) to delete</param>
    /// <returns>Delete result with all affected entity IDs</returns>
    /// <response code="200">Relation and related evidences deleted successfully</response>
    /// <response code="400">Survey is not in Draft status or relation is already deleted</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Surveys_EditAll permission</response>
    /// <response code="404">Relation not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanEditAllSurveys")]
    [ProducesResponseType(typeof(DeleteResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResultDto>> DeleteRelation(Guid id)
    {
        var command = new DeletePersonPropertyRelationCommand { RelationId = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}