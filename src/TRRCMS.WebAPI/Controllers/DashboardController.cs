using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Dashboard.Dtos;
using TRRCMS.Application.Dashboard.Queries.GetDashboardSummary;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Dashboard statistics API controller.
/// Provides aggregated data for the desktop application's dashboard page.
///
/// FR-D-12: Dashboard Statistics.
/// </summary>
[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Get aggregated dashboard statistics.
    /// Returns claims, surveys, import pipeline, and building coverage summaries.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var result = await _mediator.Send(new GetDashboardSummaryQuery());
        return Ok(result);
    }
}
