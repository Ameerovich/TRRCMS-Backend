using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Auth.Commands.ChangePassword;
using TRRCMS.Application.Auth.Commands.Login;
using TRRCMS.Application.Auth.Commands.RefreshToken;
using TRRCMS.Application.Auth.Dtos;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Authentication and authorization endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
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
    /// Login with username and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Access token and refresh token</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var command = new LoginCommand(request.Username, request.Password, request.DeviceId);
            var response = await _mediator.Send(command);

            _logger.LogInformation("User {Username} logged in successfully from device {DeviceId}",
                request.Username, request.DeviceId ?? "unknown");

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Failed login attempt for username {Username}: {Message}",
                request.Username, ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username {Username}", request.Username);
            return StatusCode(500, new { message = "An error occurred during login." });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>New access token and refresh token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var command = new RefreshTokenCommand(request.RefreshToken, request.DeviceId);
            var response = await _mediator.Send(command);

            _logger.LogInformation("Token refreshed successfully from device {DeviceId}",
                request.DeviceId ?? "unknown");

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Failed token refresh: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh." });
        }
    }

    /// <summary>
    /// Change password for current user
    /// </summary>
    /// <param name="request">Current and new password</param>
    /// <returns>Success result</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            // Get current user ID from JWT claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            // User can only change their own password
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

            return Ok(new { message = "Password changed successfully. Please login again with your new password." });
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
    /// Logout (client should discard tokens)
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Logout()
    {
        // Note: With JWT, logout is primarily client-side (discard tokens)
        // The server doesn't maintain session state

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            _logger.LogInformation("User {UserId} logged out", userIdClaim.Value);
        }

        return Ok(new { message = "Logged out successfully. Please discard your tokens." });
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<object> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        var usernameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
        var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email);

        if (userIdClaim == null || usernameClaim == null)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            userId = userIdClaim.Value,
            username = usernameClaim.Value,
            role = roleClaim?.Value,
            email = emailClaim?.Value,
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}