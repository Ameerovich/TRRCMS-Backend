using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Claims.Commands.DeleteClaim;
using TRRCMS.Application.Claims.Commands.UpdateClaim;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Claims.Queries.GetAllClaims;
using TRRCMS.Application.Claims.Queries.GetClaim;
using TRRCMS.Application.Claims.Queries.GetClaimByNumber;
using TRRCMS.Application.Claims.Queries.GetClaimSummaries;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Claims management API controller
/// Provides endpoints for claim queries and updates
/// All endpoints require authentication and specific permissions
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClaimsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    // ==================== QUERY OPERATIONS ====================

    /// <summary>
    /// Get claim by ID
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <returns>Claim with all details and computed properties</returns>
    /// <response code="200">Claim found</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_ViewAll)</response>
    /// <response code="404">Claim not found</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewAllClaims")]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimDto>> GetClaim(Guid id)
    {
        var query = new GetClaimQuery(id);
        var claim = await _mediator.Send(query);

        if (claim == null)
        {
            return NotFound($"Claim with ID {id} not found.");
        }

        return Ok(claim);
    }

    /// <summary>
    /// Get claim by claim number (e.g. CLM-2026-000000015)
    /// </summary>
    /// <param name="claimNumber">Claim number</param>
    /// <returns>Claim with all details</returns>
    /// <response code="200">Claim found</response>
    /// <response code="404">Claim not found</response>
    [HttpGet("by-number/{claimNumber}")]
    [Authorize(Policy = "CanViewAllClaims")]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimDto>> GetClaimByNumber(string claimNumber)
    {
        var claim = await _mediator.Send(new GetClaimByNumberQuery(claimNumber));

        if (claim == null)
            return NotFound($"Claim with number '{claimNumber}' not found.");

        return Ok(claim);
    }

    /// <summary>
    /// Get all claims with optional filtering and pagination
    /// </summary>
    /// <param name="query">Filter and pagination parameters</param>
    /// <returns>Paginated list of claims matching filter criteria</returns>
    /// <response code="200">Claims retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_ViewAll)</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllClaims")]
    [ProducesResponseType(typeof(PagedResult<ClaimDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ClaimDto>>> GetAllClaims([FromQuery] GetAllClaimsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get claim summaries with optional filtering.
    /// Returns lightweight DTOs suitable for the claims overview / case list UI.
    /// All enum filters accept integer codes matching the Vocabulary API.
    /// </summary>
    /// <param name="caseStatus">Filter by case status (int). Open=1, Closed=2.</param>
    /// <param name="claimSource">Filter by claim source (int). FieldCollection=1, OfficeSubmission=2, etc.</param>
    /// <param name="createdByUserId">Filter by the user who created the claim</param>
    /// <param name="surveyVisitId">Filter by originating survey ID. Returns only claims created during that survey.</param>
    /// <param name="propertyUnitId">Filter by property unit ID. Returns all claims for that property unit.</param>
    /// <param name="buildingCode">Filter by building code (17-digit GGDDSSCCNCNNBBBBB)</param>
    /// <returns>List of claim summaries matching filter criteria</returns>
    /// <response code="200">Claim summaries retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_ViewAll)</response>
    [HttpGet("summaries")]
    [Authorize(Policy = "CanViewAllClaims")]
    [ProducesResponseType(typeof(List<CreatedClaimSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<CreatedClaimSummaryDto>>> GetClaimSummaries(
        [FromQuery] int? caseStatus = null,
        [FromQuery] int? claimSource = null,
        [FromQuery] Guid? createdByUserId = null,
        [FromQuery] Guid? surveyVisitId = null,
        [FromQuery] Guid? propertyUnitId = null,
        [FromQuery] string? buildingCode = null)
    {
        var query = new GetClaimSummariesQuery
        {
            CaseStatus = caseStatus,
            ClaimSource = claimSource,
            CreatedByUserId = createdByUserId,
            SurveyVisitId = surveyVisitId,
            PropertyUnitId = propertyUnitId,
            BuildingCode = buildingCode
        };

        var summaries = await _mediator.Send(query);
        return Ok(summaries);
    }

    /// <summary>
    /// Composite claim update — updates the source PersonPropertyRelation, manages evidence links,
    /// and re-derives claim state (ClaimType, CaseStatus) from the updated relation.
    /// تحديث مركب للمطالبة — يحدّث علاقة الشخص بالعقار المصدر، ويدير روابط الأدلة، ويعيد اشتقاق حالة المطالبة.
    /// </summary>
    /// <remarks>
    /// **How it works:**
    /// Claims are never modified directly. Instead, this endpoint updates the underlying
    /// PersonPropertyRelation that created the claim, optionally manages evidence (tenure documents),
    /// and then re-derives the claim's ClaimType and CaseStatus automatically.
    ///
    /// **Auto-derivation rules (from RelationType):**
    /// - Owner (1) or Heir (5) → ClaimType = OwnershipClaim (1), CaseStatus = Closed (2)
    /// - Occupant (2), Tenant (3), Guest (4), Other (99) → ClaimType = OccupancyClaim (2), CaseStatus = Open (1)
    ///
    /// **All fields are optional** except ReasonForModification. Send only what you want to change:
    /// - Relation fields only (e.g., change RelationType from Tenant to Owner)
    /// - Evidence operations only (e.g., add new tenure documents)
    /// - TenureContractType only (claim-level legal classification)
    /// - Any combination of the above
    ///
    /// **Source relation auto-creation:**
    /// If the claim was imported via .uhc and has no existing PersonPropertyRelation,
    /// one is automatically created with RelationType inferred from the current ClaimType.
    ///
    /// **Relation fields (partial update — only provided fields change):**
    /// | Field | Type | Description |
    /// |-------|------|-------------|
    /// | RelationType | int? | Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99. Changes auto-derive ClaimType and CaseStatus. |
    /// | OccupancyType | int? | OwnerOccupied=1, TenantOccupied=2, FamilyOccupied=3, MixedOccupancy=4, Vacant=5, TemporarySeasonal=6, CommercialUse=7, Abandoned=8, Disputed=9, Unknown=99 |
    /// | OwnershipShare | decimal? | Fraction out of 2400 (e.g., 1200 = 50%). For shared ownership. |
    /// | ContractDetails | string? | Free-text contract details. |
    /// | Notes | string? | Free-text notes on the relation. |
    /// | ClearOccupancyType | bool | Set to true to explicitly clear OccupancyType to null. |
    /// | ClearOwnershipShare | bool | Set to true to explicitly clear OwnershipShare to null. |
    /// | ClearContractDetails | bool | Set to true to explicitly clear ContractDetails to null. |
    /// | ClearNotes | bool | Set to true to explicitly clear Notes to null. |
    ///
    /// **Claim-level fields (directly set on claim):**
    /// | Field | Type | Description |
    /// |-------|------|-------------|
    /// | TenureContractType | int? | FullOwnership=1, SharedOwnership=2, LongTermRental=3, ShortTermRental=4, InformalTenure=5, UnauthorizedOccupation=6, CustomaryRights=7, InheritanceBased=8, HostedGuest=9, TemporaryShelter=10, GovernmentAllocation=11, Usufruct=12, Other=99 |
    /// | TenureContractDetails | string? | Additional tenure contract details (max 1000 chars). |
    ///
    /// **Evidence operations (all optional, processed in order):**
    ///
    /// 1. **NewEvidence** (List) — Create new evidence records and link them to the source relation:
    ///    - EvidenceType (int, required): IdentificationDocument=1, OwnershipDeed=2, RentalContract=3, UtilityBill=4, Photo=5, OfficialLetter=6, CourtOrder=7, InheritanceDocument=8, TaxReceipt=9, Other=99
    ///    - Description (string, required), OriginalFileName (string, required), FilePath (string, required)
    ///    - FileSizeBytes (long, required, > 0), MimeType (string, required)
    ///    - Optional: FileHash, LinkReason, DocumentIssuedDate, DocumentExpiryDate, IssuingAuthority, DocumentReferenceNumber
    ///
    /// 2. **LinkExistingEvidenceIds** (List&lt;Guid&gt;) — Link already-existing Evidence records to the source relation.
    ///    Duplicate links are silently skipped. Deactivated links are reactivated.
    ///
    /// 3. **UnlinkEvidenceRelationIds** (List&lt;Guid&gt;) — Deactivate existing EvidenceRelation links by their IDs.
    ///    The EvidenceRelation must belong to this claim's source relation.
    ///
    /// **HasEvidence** is automatically recomputed after all evidence operations.
    ///
    /// **Required Permission:** Claims_Update (CanEditClaims policy)
    ///
    /// **Sample request — change relation type only:**
    /// ```json
    /// {
    ///   "relationType": 1,
    ///   "reasonForModification": "Corrected relation from Tenant to Owner based on deed verification"
    /// }
    /// ```
    ///
    /// **Sample request — add new evidence:**
    /// ```json
    /// {
    ///   "newEvidence": [{
    ///     "evidenceType": 2,
    ///     "description": "Ownership deed scan",
    ///     "originalFileName": "deed_scan.pdf",
    ///     "filePath": "/uploads/evidence/deed_scan.pdf",
    ///     "fileSizeBytes": 245760,
    ///     "mimeType": "application/pdf",
    ///     "linkReason": "Original ownership deed"
    ///   }],
    ///   "reasonForModification": "Added ownership deed document received from claimant"
    /// }
    /// ```
    ///
    /// **Sample request — full update (relation + evidence + tenure):**
    /// ```json
    /// {
    ///   "relationType": 1,
    ///   "ownershipShare": 2400,
    ///   "tenureContractType": 1,
    ///   "tenureContractDetails": "Full ownership with registered deed",
    ///   "newEvidence": [{
    ///     "evidenceType": 2,
    ///     "description": "Ownership deed",
    ///     "originalFileName": "deed.pdf",
    ///     "filePath": "/uploads/evidence/deed.pdf",
    ///     "fileSizeBytes": 102400,
    ///     "mimeType": "application/pdf"
    ///   }],
    ///   "unlinkEvidenceRelationIds": ["3fa85f64-5717-4562-b3fc-2c963f66afa6"],
    ///   "reasonForModification": "Updated claim after field verification confirmed ownership"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Claim ID (GUID)</param>
    /// <param name="command">Relation fields, evidence operations, claim-level fields, and mandatory audit reason</param>
    /// <returns>Updated claim DTO with source relation summary (SourceRelationId, RelationType, HasEvidence, ActiveEvidenceLinkCount)</returns>
    /// <response code="200">Claim updated successfully</response>
    /// <response code="400">Validation failed (invalid enum values, missing reason, evidence not found, etc.)</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_Update)</response>
    /// <response code="404">Claim not found, or referenced Evidence/EvidenceRelation not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanEditClaims")]
    [ProducesResponseType(typeof(UpdateClaimResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateClaimResultDto>> UpdateClaim(
        Guid id,
        [FromBody] UpdateClaimCommand command)
    {
        command.ClaimId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==================== DELETE OPERATIONS ====================

    /// <summary>
    /// Soft delete a claim.
    /// Does NOT delete the source relation or its evidence.
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <param name="deletionReason">Optional reason for deletion (audit trail)</param>
    /// <returns>Delete result with affected entity info</returns>
    /// <response code="200">Claim deleted successfully</response>
    /// <response code="400">Claim is already deleted</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_Update)</response>
    /// <response code="404">Claim not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanEditClaims")]
    [ProducesResponseType(typeof(DeleteResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResultDto>> DeleteClaim(
        Guid id,
        [FromQuery] string? deletionReason = null)
    {
        var command = new DeleteClaimCommand
        {
            ClaimId = id,
            DeletionReason = deletionReason
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

}