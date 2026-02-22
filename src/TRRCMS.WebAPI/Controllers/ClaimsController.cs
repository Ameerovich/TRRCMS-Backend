using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Claims.Commands.AssignClaim;
using TRRCMS.Application.Claims.Commands.CreateClaim;
using TRRCMS.Application.Claims.Commands.SubmitClaim;
using TRRCMS.Application.Claims.Commands.UpdateClaim;
using TRRCMS.Application.Claims.Commands.VerifyClaim;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Claims.Queries.GetAllClaims;
using TRRCMS.Application.Claims.Queries.GetClaim;
using TRRCMS.Application.Claims.Queries.GetClaimSummaries;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Claims management API controller
/// Provides endpoints for complete claim lifecycle management
/// All endpoints require authentication and specific permissions
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClaimsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    // ==================== BASIC CRUD OPERATIONS ====================

    /// <summary>
    /// Create a new claim
    /// </summary>
    /// <param name="command">Claim creation details</param>
    /// <returns>Created claim with computed properties</returns>
    /// <response code="201">Claim created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_Create)</response>
    [HttpPost]
    [Authorize(Policy = "CanCreateClaims")]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ClaimDto>> CreateClaim([FromBody] CreateClaimCommand command)
    {
        var claim = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetClaim), new { id = claim.Id }, claim);
    }

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
    /// <param name="claimStatus">Filter by claim status (int). Draft=1, Finalized=2, etc.</param>
    /// <param name="claimSource">Filter by claim source (int). FieldCollection=1, OfficeSubmission=2, etc.</param>
    /// <param name="createdByUserId">Filter by the user who created the claim</param>
    /// <param name="surveyVisitId">Filter by linked survey visit ID</param>
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
        [FromQuery] int? claimStatus = null,
        [FromQuery] int? claimSource = null,
        [FromQuery] Guid? createdByUserId = null,
        [FromQuery] Guid? surveyVisitId = null,
        [FromQuery] string? buildingCode = null)
    {
        var query = new GetClaimSummariesQuery
        {
            ClaimStatus = claimStatus,
            ClaimSource = claimSource,
            CreatedByUserId = createdByUserId,
            SurveyVisitId = surveyVisitId,
            BuildingCode = buildingCode
        };

        var summaries = await _mediator.Send(query);
        return Ok(summaries);
    }

    /// <summary>
    /// Update existing claim
    /// UC-006: Update Existing Claim
    /// Requires: Data Manager or Administrator role
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <param name="command">Update details with reason for modification</param>
    /// <returns>Updated claim with audit trail</returns>
    /// <response code="200">Claim updated successfully</response>
    /// <response code="400">Invalid request or validation failed</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_Update)</response>
    /// <response code="404">Claim not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanEditClaims")]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimDto>> UpdateClaim(
        Guid id,
        [FromBody] UpdateClaimCommand command)
    {
        command.ClaimId = id;
        var claim = await _mediator.Send(command);
        return Ok(claim);
    }

    // ==================== WORKFLOW OPERATIONS ====================
    // NOTE: These endpoints are functional but do NOT validate state transitions yet.
    // State machine implementation is planned for Phase 2.
    // Use with caution until state validation is added.

    /// <summary>
    /// Submit claim for processing
    /// ⚠️ WARNING: Does not validate state transitions yet (requires Phase 2 state machine)
    /// Requires: Office Clerk, Data Manager, Field Supervisor, or Administrator role
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <param name="command">Submission details</param>
    /// <returns>No content</returns>
    /// <response code="204">Claim submitted successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_Submit)</response>
    /// <response code="404">Claim not found</response>
    [HttpPut("{id}/submit")]
    [Authorize(Policy = "CanSubmitClaims")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitClaim(Guid id, [FromBody] SubmitClaimCommand command)
    {
        command.ClaimId = id;
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Assign claim to case officer
    /// Requires: Data Manager or Administrator role
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <param name="command">Assignment details</param>
    /// <returns>No content</returns>
    /// <response code="204">Claim assigned successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_Assign)</response>
    /// <response code="404">Claim not found</response>
    [HttpPut("{id}/assign")]
    [Authorize(Policy = "CanAssignClaims")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignClaim(Guid id, [FromBody] AssignClaimCommand command)
    {
        command.ClaimId = id;
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Verify claim
    /// ⚠️ WARNING: Does not validate state transitions yet (requires Phase 2 state machine)
    /// Requires: Data Manager or Administrator role
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <param name="command">Verification details</param>
    /// <returns>No content</returns>
    /// <response code="204">Claim verified successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_Verify)</response>
    /// <response code="404">Claim not found</response>
    [HttpPut("{id}/verify")]
    [Authorize(Policy = "CanVerifyClaims")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyClaim(Guid id, [FromBody] VerifyClaimCommand command)
    {
        command.ClaimId = id;
        await _mediator.Send(command);
        return NoContent();
    }
}