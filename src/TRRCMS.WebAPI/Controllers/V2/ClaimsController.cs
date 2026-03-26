using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Claims.Queries.GetClaimSummaries;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Claims v2 — list endpoints wrapped in ListResponse.
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class ClaimsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClaimsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get claim summaries with optional filtering.
    /// </summary>
    [HttpGet("summaries")]
    [Authorize(Policy = "CanViewAllClaims")]
    [ProducesResponseType(typeof(ListResponse<CreatedClaimSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ListResponse<CreatedClaimSummaryDto>>> GetClaimSummaries(
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
        return Ok(ListResponse<CreatedClaimSummaryDto>.From(summaries));
    }
}
