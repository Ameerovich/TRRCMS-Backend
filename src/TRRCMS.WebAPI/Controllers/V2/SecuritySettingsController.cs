using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.SecuritySettings.Dtos;
using TRRCMS.Application.SecuritySettings.Queries.GetSecuritySettingsHistory;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Security Settings API v2 — list endpoints with ListResponse wrapper.
/// </summary>
[Route("api/v2/security-settings")]
[ApiController]
[Produces("application/json")]
public class SecuritySettingsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SecuritySettingsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get security policy version history.
    /// </summary>
    [HttpGet("history")]
    [Authorize(Policy = "CanManageSecuritySettings")]
    [ProducesResponseType(typeof(ListResponse<SecurityPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ListResponse<SecurityPolicyDto>>> GetSecuritySettingsHistory(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetSecuritySettingsHistoryQuery(), cancellationToken);
        return Ok(ListResponse<SecurityPolicyDto>.From(result));
    }
}
