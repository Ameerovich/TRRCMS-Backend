using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Auth.Commands.ChangePassword;
using TRRCMS.Application.Auth.Commands.Login;
using TRRCMS.Application.Auth.Commands.RefreshToken;
using TRRCMS.Application.Auth.Dtos;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Authentication and authorization endpoints for TRRCMS
/// </summary>
/// <remarks>
/// Provides JWT-based authentication for the Tenure Rights Registration and Claims Management System.
/// 
/// **Authentication Flow:**
/// 1. Call `/login` with username/password to get access token and refresh token
/// 2. Use access token in `Authorization: Bearer {token}` header for all protected endpoints
/// 3. When access token expires (15 min), call `/refresh` with refresh token to get new tokens
/// 4. Call `/logout` to end session (client discards tokens)
/// 
/// **Token Lifetimes:**
/// - Access Token: 15 minutes
/// - Refresh Token: 7 days
/// 
/// **User Roles:**
/// - Administrator (1): Full system access
/// - DataManager (2): Data management and verification
/// - FieldSupervisor (3): Field team supervision
/// - FieldCollector (4): Mobile field data collection
/// - OfficeClerk (5): Office data entry
/// - Analyst (6): Reports and analytics
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
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
    /// <remarks>
    /// Authenticates user credentials and returns JWT tokens for API access.
    /// 
    /// **Use Case**: All users - Initial authentication
    /// 
    /// **What you get on success:**
    /// - User profile (ID, name, role, permissions)
    /// - Access token (JWT, 15 min lifetime)
    /// - Refresh token (7 days lifetime)
    /// - Platform access flags (mobile/desktop)
    /// - Password change requirement flag
    /// 
    /// **DeviceId Usage:**
    /// The optional `deviceId` field is used for:
    /// - Audit trail (tracking which device/tablet initiated the login)
    /// - Session management across multiple devices
    /// - Security monitoring
    /// 
    /// **Example request:**
    /// ```json
    /// {
    ///   "username": "fieldcollector1",
    ///   "password": "SecurePass123!",
    ///   "deviceId": "TABLET-FC-001"
    /// }
    /// ```
    /// 
    /// **Example success response:**
    /// ```json
    /// {
    ///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "username": "fieldcollector1",
    ///   "fullNameArabic": "أحمد محمد",
    ///   "fullNameEnglish": "Ahmed Mohammed",
    ///   "email": "ahmed@trrcms.com",
    ///   "role": 4,
    ///   "roleName": "FieldCollector",
    ///   "hasMobileAccess": true,
    ///   "hasDesktopAccess": false,
    ///   "preferredLanguage": "ar",
    ///   "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    ///   "refreshToken": "dGhpcyBpcyBhIHJlZnJl...",
    ///   "accessTokenExpiry": "2026-01-31T15:30:00Z",
    ///   "refreshTokenExpiry": "2026-02-07T15:15:00Z",
    ///   "mustChangePassword": false
    /// }
    /// ```
    /// 
    /// **Note:** If `mustChangePassword` is `true`, redirect user to change password before allowing other actions.
    /// </remarks>
    /// <param name="request">Login credentials containing username, password, and optional deviceId</param>
    /// <returns>User profile with access and refresh tokens</returns>
    /// <response code="200">Login successful. Returns user profile and tokens.</response>
    /// <response code="401">Invalid credentials or account locked/inactive.</response>
    /// <response code="500">Server error during authentication.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <remarks>
    /// Exchanges a valid refresh token for new access and refresh tokens.
    /// 
    /// **Use Case**: All authenticated users - Token renewal
    /// 
    /// **When to call:**
    /// - When access token expires (401 response from other endpoints)
    /// - Proactively before expiry (check `accessTokenExpiry` from login)
    /// 
    /// **Security Notes:**
    /// - Refresh tokens are single-use (old token invalidated after refresh)
    /// - If refresh token is expired or invalid, user must login again
    /// - Each refresh generates a new refresh token (rotation)
    /// 
    /// **Example request:**
    /// ```json
    /// {
    ///   "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
    ///   "deviceId": "TABLET-FC-001"
    /// }
    /// ```
    /// 
    /// **Example success response:**
    /// ```json
    /// {
    ///   "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    ///   "refreshToken": "bmV3IHJlZnJlc2ggdG9rZW4...",
    ///   "accessTokenExpiry": "2026-01-31T15:45:00Z",
    ///   "refreshTokenExpiry": "2026-02-07T15:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Refresh token and optional deviceId</param>
    /// <returns>New access token and refresh token</returns>
    /// <response code="200">Token refreshed successfully. Returns new tokens.</response>
    /// <response code="401">Invalid or expired refresh token. User must login again.</response>
    /// <response code="500">Server error during token refresh.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// <remarks>
    /// Allows authenticated users to change their own password.
    /// 
    /// **Use Case**: All authenticated users - Password management
    /// 
    /// **Requirements:**
    /// - User must be authenticated (valid access token)
    /// - User can only change their own password (userId must match token)
    /// - Current password must be correct
    /// - New password must meet complexity requirements
    /// - Confirm password must match new password
    /// 
    /// **Password Policy:**
    /// - Minimum 8 characters
    /// - At least one uppercase letter
    /// - At least one lowercase letter
    /// - At least one digit
    /// - At least one special character
    /// 
    /// **After Success:**
    /// - All existing tokens are invalidated
    /// - User must login again with new password
    /// 
    /// **Example request:**
    /// ```json
    /// {
    ///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "currentPassword": "OldPass123!",
    ///   "newPassword": "NewSecure456!",
    ///   "confirmPassword": "NewSecure456!"
    /// }
    /// ```
    /// 
    /// **Example success response:**
    /// ```json
    /// {
    ///   "message": "Password changed successfully. Please login again with your new password."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Current password, new password, and confirmation</param>
    /// <returns>Success message</returns>
    /// <response code="200">Password changed successfully. User must re-login.</response>
    /// <response code="400">Validation error (password mismatch, weak password, etc.).</response>
    /// <response code="401">Not authenticated or current password incorrect.</response>
    /// <response code="403">Attempting to change another user's password.</response>
    /// <response code="404">User not found.</response>
    /// <response code="500">Server error during password change.</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    /// Logout current user session
    /// </summary>
    /// <remarks>
    /// Ends the current user session.
    /// 
    /// **Use Case**: All authenticated users - Session termination
    /// 
    /// **Important Notes:**
    /// - With JWT authentication, logout is primarily client-side
    /// - Server does not maintain session state
    /// - Client MUST discard both access token and refresh token
    /// - For security, do not store tokens after logout
    /// 
    /// **Client Responsibilities After Logout:**
    /// 1. Delete access token from storage
    /// 2. Delete refresh token from storage
    /// 3. Clear any cached user data
    /// 4. Redirect to login page
    /// 
    /// **Example success response:**
    /// ```json
    /// {
    ///   "message": "Logged out successfully. Please discard your tokens."
    /// }
    /// ```
    /// </remarks>
    /// <returns>Success message</returns>
    /// <response code="200">Logout acknowledged. Client should discard tokens.</response>
    /// <response code="401">Not authenticated (no valid token).</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult Logout()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
        {
            _logger.LogInformation("User {UserId} logged out", userIdClaim.Value);
        }

        return Ok(new { message = "Logged out successfully. Please discard your tokens." });
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <remarks>
    /// Returns information about the currently authenticated user from the JWT token claims.
    /// 
    /// **Use Case**: All authenticated users - Profile/session verification
    /// 
    /// **When to use:**
    /// - Verify user is still authenticated
    /// - Get user details without storing locally
    /// - Debug authentication issues
    /// - Display current user info in UI
    /// 
    /// **Note:** This extracts data from the JWT token, not from the database.
    /// For full user profile, use the Users API.
    /// 
    /// **Example success response:**
    /// ```json
    /// {
    ///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "username": "fieldcollector1",
    ///   "role": "FieldCollector",
    ///   "email": "ahmed@trrcms.com",
    ///   "claims": [
    ///     { "type": "nameid", "value": "3fa85f64-5717-4562-b3fc-2c963f66afa6" },
    ///     { "type": "unique_name", "value": "fieldcollector1" },
    ///     { "type": "role", "value": "FieldCollector" },
    ///     { "type": "email", "value": "ahmed@trrcms.com" },
    ///     { "type": "has_mobile_access", "value": "True" }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <returns>Current user details extracted from JWT token</returns>
    /// <response code="200">Returns current user information.</response>
    /// <response code="401">Not authenticated or invalid token.</response>
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