using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Auth.Commands.ChangePassword;
using TRRCMS.Application.Auth.Dtos;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Auth API v2 — endpoints with MessageResponse wrapper.
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Change password for current user.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MessageResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            if (request.UserId != currentUserId)
            {
                return Forbid();
            }

            var command = new ChangePasswordCommand(
                request.UserId,
                request.CurrentPassword,
                request.NewPassword,
                request.ConfirmPassword,
                currentUserId);

            await _mediator.Send(command);

            _logger.LogInformation("User {UserId} changed password successfully", currentUserId);

            return Ok(MessageResponse.Ok("Password changed successfully. Please login again with your new password."));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Password change validation failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Password change unauthorized: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found during password change: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change");
            return StatusCode(500, new { message = "An error occurred during password change." });
        }
    }

    /// <summary>
    /// Logout current user session.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<MessageResponse> Logout()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            _logger.LogInformation("User {UserId} logged out", userIdClaim.Value);
        }

        return Ok(MessageResponse.Ok("Logged out successfully. Please discard your tokens."));
    }
}
