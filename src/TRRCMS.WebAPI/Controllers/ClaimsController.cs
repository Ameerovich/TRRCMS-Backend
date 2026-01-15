using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Claims.Commands.AssignClaim;
using TRRCMS.Application.Claims.Commands.CreateClaim;
using TRRCMS.Application.Claims.Commands.SubmitClaim;
using TRRCMS.Application.Claims.Commands.UpdateClaim;
using TRRCMS.Application.Claims.Commands.VerifyClaim;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Claims.Queries.GetAllClaims;
using TRRCMS.Application.Claims.Queries.GetClaim;
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
    /// Get all claims with optional filtering
    /// </summary>
    /// <param name="lifecycleStage">Filter by lifecycle stage</param>
    /// <param name="status">Filter by status</param>
    /// <param name="priority">Filter by priority</param>
    /// <param name="assignedToUserId">Filter by assigned user</param>
    /// <param name="primaryClaimantId">Filter by claimant</param>
    /// <param name="propertyUnitId">Filter by property unit</param>
    /// <param name="verificationStatus">Filter by verification status</param>
    /// <param name="hasConflicts">Filter by conflicts</param>
    /// <param name="isOverdue">Filter by overdue status</param>
    /// <param name="awaitingDocuments">Filter by awaiting documents</param>
    /// <returns>List of claims matching filter criteria</returns>
    /// <response code="200">Claims retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_ViewAll)</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllClaims")]
    [ProducesResponseType(typeof(IEnumerable<ClaimDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ClaimDto>>> GetAllClaims(
        [FromQuery] LifecycleStage? lifecycleStage = null,
        [FromQuery] ClaimStatus? status = null,
        [FromQuery] CasePriority? priority = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] Guid? primaryClaimantId = null,
        [FromQuery] Guid? propertyUnitId = null,
        [FromQuery] VerificationStatus? verificationStatus = null,
        [FromQuery] bool? hasConflicts = null,
        [FromQuery] bool? isOverdue = null,
        [FromQuery] bool? awaitingDocuments = null)
    {
        var query = new GetAllClaimsQuery
        {
            LifecycleStage = lifecycleStage,
            Status = status,
            Priority = priority,
            AssignedToUserId = assignedToUserId,
            PrimaryClaimantId = primaryClaimantId,
            PropertyUnitId = propertyUnitId,
            VerificationStatus = verificationStatus,
            HasConflicts = hasConflicts,
            IsOverdue = isOverdue,
            AwaitingDocuments = awaitingDocuments
        };

        var claims = await _mediator.Send(query);
        return Ok(claims);
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