using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
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
[Route("api/[controller]")]
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
    /// Composite update: modifies the source PersonPropertyRelation + manages evidence links,
    /// then re-derives claim state (ClaimType, CaseStatus).
    /// Requires: Data Manager or Administrator role
    /// </summary>
    /// <param name="id">Claim ID</param>
    /// <param name="command">Relation fields, evidence operations, and audit reason</param>
    /// <returns>Updated claim with source relation summary</returns>
    /// <response code="200">Claim updated successfully</response>
    /// <response code="400">Invalid request or validation failed</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Claims_Update)</response>
    /// <response code="404">Claim not found</response>
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