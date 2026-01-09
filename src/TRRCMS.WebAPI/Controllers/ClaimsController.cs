using MediatR;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Claims.Commands.ApproveClaim;
using TRRCMS.Application.Claims.Commands.AssignClaim;
using TRRCMS.Application.Claims.Commands.CreateClaim;
using TRRCMS.Application.Claims.Commands.RejectClaim;
using TRRCMS.Application.Claims.Commands.SubmitClaim;
using TRRCMS.Application.Claims.Commands.VerifyClaim;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Claims.Queries.GetAllClaims;
using TRRCMS.Application.Claims.Queries.GetClaim;
using TRRCMS.Domain.Enums;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Claims management API controller
/// Provides endpoints for complete claim lifecycle management
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
    [HttpPost]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    /// <response code="404">Claim not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status200OK)]
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
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClaimDto>), StatusCodes.Status200OK)]
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
    
    // ==================== WORKFLOW OPERATIONS ====================
    
    /// <summary>
    /// Submit claim for processing
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <param name="command">Submission details</param>
    /// <returns>No content</returns>
    /// <response code="204">Claim submitted successfully</response>
    /// <response code="404">Claim not found</response>
    [HttpPut("{id}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitClaim(Guid id, [FromBody] SubmitClaimCommand command)
    {
        command.ClaimId = id;
        await _mediator.Send(command);
        return NoContent();
    }
    
    /// <summary>
    /// Assign claim to case officer
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <param name="command">Assignment details</param>
    /// <returns>No content</returns>
    /// <response code="204">Claim assigned successfully</response>
    /// <response code="404">Claim not found</response>
    [HttpPut("{id}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignClaim(Guid id, [FromBody] AssignClaimCommand command)
    {
        command.ClaimId = id;
        await _mediator.Send(command);
        return NoContent();
    }
    
    /// <summary>
    /// Verify claim
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <param name="command">Verification details</param>
    /// <returns>No content</returns>
    /// <response code="204">Claim verified successfully</response>
    /// <response code="404">Claim not found</response>
    [HttpPut("{id}/verify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyClaim(Guid id, [FromBody] VerifyClaimCommand command)
    {
        command.ClaimId = id;
        await _mediator.Send(command);
        return NoContent();
    }
    
    /// <summary>
    /// Approve claim
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <param name="command">Approval details</param>
    /// <returns>No content</returns>
    /// <response code="204">Claim approved successfully</response>
    /// <response code="404">Claim not found</response>
    [HttpPut("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveClaim(Guid id, [FromBody] ApproveClaimCommand command)
    {
        command.ClaimId = id;
        await _mediator.Send(command);
        return NoContent();
    }
    
    /// <summary>
    /// Reject claim
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <param name="command">Rejection details (reason required)</param>
    /// <returns>No content</returns>
    /// <response code="204">Claim rejected successfully</response>
    /// <response code="400">Invalid request (missing reason)</response>
    /// <response code="404">Claim not found</response>
    [HttpPut("{id}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectClaim(Guid id, [FromBody] RejectClaimCommand command)
    {
        command.ClaimId = id;
        await _mediator.Send(command);
        return NoContent();
    }
}
