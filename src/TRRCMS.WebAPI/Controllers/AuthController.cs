using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Auth.Commands.ChangePassword;
using TRRCMS.Application.Auth.Commands.Login;
using TRRCMS.Application.Auth.Commands.RefreshToken;
using TRRCMS.Application.Auth.Commands.RequestPasswordReset;
using TRRCMS.Application.Auth.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.WebAPI.Middleware;

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
    private readonly IConfiguration _configuration;
    private readonly IStringLocalizer<ErrorMessages> _errorLocalizer;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(
        IMediator mediator,
        ILogger<AuthController> logger,
        IConfiguration configuration,
        IStringLocalizer<ErrorMessages> errorLocalizer,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _logger = logger;
        _configuration = configuration;
        _errorLocalizer = errorLocalizer;
        _currentUserService = currentUserService;
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
    /// **First Login / Password Change Required:**
    /// When `mustChangePassword` is `true` (first login after account creation):
    /// - `accessToken` is a **restricted token** (10 min lifetime, contains `must_change_password` claim)
    /// - `refreshToken` is `null` — no refresh token is issued
    /// - `refreshTokenExpiry` is `null`
    /// - The restricted token **only works** for `POST /api/v1/auth/change-password` and `POST /api/v1/auth/logout`
    /// - All other API endpoints will return **403 Forbidden** with `"error": "PasswordChangeRequired"`
    /// - After changing password, the user **must login again** to get a full access token
    ///
    /// **Frontend flow for first login:**
    /// 1. Call `POST /api/v1/auth/login`
    /// 2. Check `mustChangePassword` in response
    /// 3. If `true`: redirect to change-password screen, use the restricted `accessToken` for the change-password call
    /// 4. After successful password change: redirect to login screen, user logs in again with new password
    /// 5. If `false`: proceed normally with full access token
    /// 
    /// **DeviceId:** Optional. Only used by field collectors on tablets for device tracking.
    /// Desktop and dashboard clients should omit this field.
    ///
    /// **Example request (desktop/dashboard):**
    /// ```json
    /// {
    ///   "username": "admin",
    ///   "password": "Admin@123"
    /// }
    /// ```
    ///
    /// **Example request (field collector with tablet):**
    /// ```json
    /// {
    ///   "username": "collector",
    ///   "password": "Field@123",
    ///   "deviceId": "TABLET-FC-001"
    /// }
    /// ```
    /// 
    /// **Example success response (normal login):**
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
    /// **Example response (first login - must change password):**
    /// ```json
    /// {
    ///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "username": "newuser",
    ///   "fullNameArabic": "مستخدم جديد",
    ///   "role": 3,
    ///   "roleName": "OfficeClerk",
    ///   "hasMobileAccess": false,
    ///   "hasDesktopAccess": true,
    ///   "preferredLanguage": "ar",
    ///   "accessToken": "eyJhbGciOiJIUzI1NiIs...(restricted)...",
    ///   "refreshToken": null,
    ///   "accessTokenExpiry": "2026-01-31T15:25:00Z",
    ///   "refreshTokenExpiry": null,
    ///   "mustChangePassword": true
    /// }
    /// ```
    ///
    /// **IMPORTANT:** If `mustChangePassword` is `true`:
    /// - The `accessToken` is restricted (10 min, only works for change-password)
    /// - `refreshToken` and `refreshTokenExpiry` are `null`
    /// - Redirect user to change-password screen immediately
    /// - After password change, user must login again for full access
    /// </remarks>
    /// <param name="request">Login credentials containing username, password, and optional deviceId</param>
    /// <returns>User profile with access and refresh tokens</returns>
    /// <response code="200">Login successful. Returns user profile and tokens.</response>
    /// <response code="401">Invalid credentials or account locked/inactive.</response>
    /// <response code="500">Server error during authentication.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Username, request.Password, request.DeviceId);
        var response = await _mediator.Send(command);

        _logger.LogInformation("User {Username} logged in successfully from device {DeviceId}",
            request.Username, request.DeviceId ?? "unknown");

        return Ok(response);
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
    ///
    /// **Note:** Refresh is blocked if the user has `mustChangePassword = true`.
    /// In that case, the user must login and change their password first.
    /// </remarks>
    /// <param name="request">Refresh token and optional deviceId</param>
    /// <returns>New access token and refresh token</returns>
    /// <response code="200">Token refreshed successfully. Returns new tokens.</response>
    /// <response code="401">Invalid or expired refresh token, or user must change password first. User must login again.</response>
    /// <response code="500">Server error during token refresh.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken, request.DeviceId);
        var response = await _mediator.Send(command);

        _logger.LogInformation("Token refreshed successfully from device {DeviceId}",
            request.DeviceId ?? "unknown");

        return Ok(response);
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
    /// **Works with restricted tokens:** This endpoint is accessible with the restricted token
    /// issued when `mustChangePassword` is `true` (first login). After changing the password,
    /// the user must login again to receive a full-access token.
    ///
    /// **After Success:**
    /// - Every OTHER session of the user ends (security stamp rotated, old refresh token revoked)
    /// - The response carries a fresh token pair for THIS session (same fields as `/refresh`):
    ///   replace the stored access/refresh tokens and continue — no re-login needed
    /// - `mustChangePassword` is set to `false` (first-login restricted flow: the returned tokens are full tokens)
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
    ///   "success": true,
    ///   "message": "Password changed successfully. Other sessions were signed out; this session continues with the returned tokens.",
    ///   "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    ///   "refreshToken": "bmV3IHJlZnJlc2ggdG9rZW4...",
    ///   "accessTokenExpiry": "2026-10-17T15:45:00Z",
    ///   "refreshTokenExpiry": "2026-10-24T15:30:00Z",
    ///   "serverTimeUtc": "2026-10-17T15:15:00Z",
    ///   "accessTokenExpiresInSeconds": 1800,
    ///   "sessionExpiryWarningSeconds": 60
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
    [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<ChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        // Get current user ID from JWT claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
        {
            return Unauthorized(new ErrorResponse
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Message = "Invalid user token."
            });
        }

        // User can only change their own password
        // userId in the body is optional; when sent it must be the caller's own id
        if (request.UserId != Guid.Empty && request.UserId != currentUserId)
        {
            return Forbid();
        }

        var command = new ChangePasswordCommand(
            currentUserId,
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmPassword,
            currentUserId)
        {
            DeviceId = User.FindFirst("device_id")?.Value
        };

        var result = await _mediator.Send(command);

        _logger.LogInformation("User {UserId} changed password successfully", currentUserId);

        result.Message = "Password changed successfully. Other sessions were signed out; this session continues with the returned tokens.";
        return Ok(result);
    }

    /// <summary>
    /// Current session timing
    /// </summary>
    /// <remarks>
    /// Returns when the access token used for this call expires, by the **server** clock.
    /// Clients use it to (re)schedule the "session about to expire" warning, e.g. after waking
    /// from sleep. Calling it does not extend the session — use `POST /refresh` to renew.
    ///
    /// **Example response:**
    /// ```json
    /// {
    ///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "username": "clerk01",
    ///   "accessTokenExpiry": "2026-10-17T15:30:00Z",
    ///   "serverTimeUtc": "2026-10-17T15:21:12Z",
    ///   "accessTokenExpiresInSeconds": 528,
    ///   "sessionExpiryWarningSeconds": 60,
    ///   "mustChangePassword": false
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Session is valid</response>
    /// <response code="401">Expired (errorCode TOKEN_EXPIRED) or revoked (SESSION_REVOKED)</response>
    [HttpGet("session")]
    [Authorize]
    [ProducesResponseType(typeof(SessionInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public ActionResult<SessionInfoResponse> GetSession()
    {
        var expClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Exp)?.Value;
        var expiry = long.TryParse(expClaim, out var exp)
            ? DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime
            : DateTime.UtcNow;

        return Ok(new SessionInfoResponse
        {
            UserId = Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : Guid.Empty,
            Username = User.Identity?.Name ?? string.Empty,
            AccessTokenExpiry = expiry,
            SessionExpiryWarningSeconds = TRRCMS.Application.Auth.SessionSettings.GetExpiryWarningSeconds(_configuration),
            MustChangePassword = User.FindFirst("must_change_password")?.Value == "true"
        });
    }

    /// <summary>
    /// Ask an administrator to reset your password ("forgot password")
    /// </summary>
    /// <remarks>
    /// Anonymous. Queues a reset request that administrators see in
    /// `GET /api/v1/users/password-reset-requests`. The administrator then sets a temporary
    /// password and gives it to the user out of band; the user must change it at next login.
    ///
    /// The response is **always** 202 with the same message, whether or not the username exists,
    /// so the endpoint cannot be used to discover accounts. A pending request is never overwritten
    /// by later calls. Rate-limited to 3 requests per 10 minutes per IP.
    ///
    /// **Example request:**
    /// ```json
    /// { "username": "clerk01", "note": "Call me on extension 214" }
    /// ```
    /// </remarks>
    /// <response code="202">Request recorded (or silently ignored for unknown usernames)</response>
    /// <response code="400">Username missing / note too long</response>
    /// <response code="429">Too many requests</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("forgot-password")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _mediator.Send(new RequestPasswordResetCommand { Username = request.Username, Note = request.Note });

        return Accepted(new { message = _errorLocalizer["Message_PasswordResetRequestAccepted"].Value });
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
    /// Returns identity claims and the current effective permissions from the database.
    /// 
    /// **Use Case**: All authenticated users - Profile/session verification
    /// 
    /// **When to use:**
    /// - Verify user is still authenticated
    /// - Get user details without storing locally
    /// - Debug authentication issues
    /// - Display current user info in UI
    /// 
    /// **Note:** Identity data comes from the JWT; effective permissions are loaded
    /// from the database so grants, revocations and expirations are respected.
    /// For full user profile, use the Users API.
    /// 
    /// **Example success response:**
    /// ```json
    /// {
    ///   "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "username": "fieldcollector1",
    ///   "role": "FieldCollector",
    ///   "email": "ahmed@trrcms.com",
    ///   "permissions": [4000, 8500],
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
    public async Task<ActionResult<object>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        var usernameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
        var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email);

        if (userIdClaim == null || usernameClaim == null)
        {
            return Unauthorized();
        }

        var currentUser = await _currentUserService.GetCurrentUserAsync(cancellationToken);
        if (currentUser == null || !currentUser.IsActive)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            userId = userIdClaim.Value,
            username = usernameClaim.Value,
            role = roleClaim?.Value,
            email = emailClaim?.Value,
            permissions = currentUser.GetActivePermissions()
                .Select(permission => (int)permission)
                .OrderBy(permission => permission)
                .ToArray(),
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}