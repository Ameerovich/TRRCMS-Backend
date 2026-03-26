using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Application.Users.Queries.GetUserAuditLog;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Users API v2 — list endpoints with ListResponse wrapper.
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    public UsersController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get user audit log.
    /// </summary>
    [HttpGet("{id}/audit-log")]
    [Authorize(Policy = "CanViewAuditLogs")]
    [ProducesResponseType(typeof(ListResponse<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListResponse<AuditLogDto>>> GetUserAuditLog(Guid id)
    {
        var result = await _mediator.Send(new GetUserAuditLogQuery { UserId = id });
        return Ok(ListResponse<AuditLogDto>.From(result));
    }
}
