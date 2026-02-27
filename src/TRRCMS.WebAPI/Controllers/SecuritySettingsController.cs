using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.SecuritySettings.Commands.UpdateSecuritySettings;
using TRRCMS.Application.SecuritySettings.Dtos;
using TRRCMS.Application.SecuritySettings.Queries.GetCurrentSecuritySettings;
using TRRCMS.Application.SecuritySettings.Queries.GetSecuritySettingsHistory;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Security Settings API — UC-011 (إعدادات الأمان)
/// </summary>
/// <remarks>
/// Manages system-wide security policy configuration including password rules,
/// session/lockout settings, and access control policies.
///
/// **Architecture:**
/// - Security policies are versioned; each "Apply" creates a new immutable version.
/// - Only one policy is active at any time; previous versions are preserved for audit.
/// - All changes are logged per FSD Section 13: Security &amp; Audit requirements.
///
/// **Workflow (UC-011 S01–S08):**
/// 1. Admin opens security settings → `GET /current` loads the active configuration.
/// 2. Admin configures password, session/lockout, and access control sections.
/// 3. Admin submits → `PUT /` validates and applies the new policy atomically.
/// 4. Previous policy is deactivated; new version becomes effective immediately.
///
/// **Endpoints:**
/// | Method | Path                                  | Auth                       | Description                        |
/// |--------|---------------------------------------|----------------------------|------------------------------------|
/// | GET    | /api/v1/security-settings/current     | CanManageSecuritySettings   | Get active security policy         |
/// | GET    | /api/v1/security-settings/history     | CanManageSecuritySettings   | Get all policy versions (audit)    |
/// | PUT    | /api/v1/security-settings             | CanManageSecuritySettings   | Validate and apply new policy      |
/// </remarks>
[ApiController]
[Route("api/v1/security-settings")]
[Produces("application/json")]
public class SecuritySettingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SecuritySettingsController> _logger;

    public SecuritySettingsController(IMediator mediator, ILogger<SecuritySettingsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ==================== READ ENDPOINTS ====================

    /// <summary>
    /// Get the currently active security policy (عرض إعدادات الأمان الحالية) — UC-011 S02
    /// </summary>
    /// <remarks>
    /// Returns the active security policy with all three sections:
    /// password policy, session/lockout policy, and access control policy.
    ///
    /// **Required permission:** `CanManageSecuritySettings` (Security_Settings = 8300)
    ///
    /// **Example response:**
    /// ```json
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "version": 3,
    ///   "isActive": true,
    ///   "effectiveFromUtc": "2025-12-01T10:30:00Z",
    ///   "passwordPolicy": {
    ///     "minLength": 10,
    ///     "requireUppercase": true,
    ///     "requireLowercase": true,
    ///     "requireDigit": true,
    ///     "requireSpecialCharacter": true,
    ///     "expiryDays": 60,
    ///     "reuseHistory": 5
    ///   },
    ///   "sessionLockoutPolicy": {
    ///     "sessionTimeoutMinutes": 20,
    ///     "maxFailedLoginAttempts": 3,
    ///     "lockoutDurationMinutes": 30
    ///   },
    ///   "accessControlPolicy": {
    ///     "allowPasswordAuthentication": true,
    ///     "allowSsoAuthentication": false,
    ///     "allowTokenAuthentication": true,
    ///     "enforceIpAllowlist": false,
    ///     "ipAllowlist": null,
    ///     "ipDenylist": null,
    ///     "restrictByEnvironment": false,
    ///     "allowedEnvironments": null
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The currently active security policy configuration</returns>
    /// <response code="200">Returns the active security policy</response>
    /// <response code="401">Not authenticated — JWT token missing or expired</response>
    /// <response code="403">Not authorized — requires CanManageSecuritySettings permission</response>
    /// <response code="404">No active security policy found (system not seeded)</response>
    [HttpGet("current")]
    [Authorize(Policy = "CanManageSecuritySettings")]
    [ProducesResponseType(typeof(SecurityPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SecurityPolicyDto>> GetCurrentSecuritySettings(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetCurrentSecuritySettingsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get security policy version history (عرض سجل إصدارات سياسة الأمان) — Audit Trail
    /// </summary>
    /// <remarks>
    /// Returns all security policy versions ordered by version descending (newest first).
    /// Each version is a complete snapshot of the policy at that point in time.
    /// Supports FSD Section 13.4: Legal Audit Trail for security configuration changes.
    ///
    /// **Required permission:** `CanManageSecuritySettings` (Security_Settings = 8300)
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all security policy versions</returns>
    /// <response code="200">Returns the version history (may be empty)</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not authorized</response>
    [HttpGet("history")]
    [Authorize(Policy = "CanManageSecuritySettings")]
    [ProducesResponseType(typeof(List<SecurityPolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<SecurityPolicyDto>>> GetSecuritySettingsHistory(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving security policy version history");
        var result = await _mediator.Send(new GetSecuritySettingsHistoryQuery(), cancellationToken);
        return Ok(result);
    }

    // ==================== WRITE ENDPOINTS ====================

    /// <summary>
    /// Validate and apply a new security policy (تطبيق سياسة أمان جديدة) — UC-011 S03–S08
    /// </summary>
    /// <remarks>
    /// Validates the submitted security configuration against safety constraints,
    /// then atomically applies it as a new versioned policy. The previous active policy
    /// is deactivated and preserved in version history.
    ///
    /// **Required permission:** `CanManageSecuritySettings` (Security_Settings = 8300)
    ///
    /// **Validation rules (UC-011 S06):**
    /// - Password min length: 8–128 characters
    /// - If no complexity requirements: min length must be ≥ 12
    /// - Password expiry: 0 (disabled) to 365 days
    /// - Password reuse history: 0 (disabled) to 24
    /// - Session timeout: 5–1440 minutes (5 min to 24 hours)
    /// - Max failed login attempts: 3–20
    /// - Lockout duration: 1–1440 minutes
    /// - At least one authentication method must remain enabled
    /// - IP allowlist cannot be empty when enforcement is enabled
    ///
    /// **Alternative flow S06a:** If validation fails, a 400 response is returned
    /// with details on which parameters are invalid. The current policy remains in force.
    ///
    /// **Example request:**
    /// ```json
    /// {
    ///   "passwordMinLength": 10,
    ///   "passwordRequireUppercase": true,
    ///   "passwordRequireLowercase": true,
    ///   "passwordRequireDigit": true,
    ///   "passwordRequireSpecialCharacter": true,
    ///   "passwordExpiryDays": 60,
    ///   "passwordReuseHistory": 5,
    ///   "sessionTimeoutMinutes": 20,
    ///   "maxFailedLoginAttempts": 3,
    ///   "lockoutDurationMinutes": 30,
    ///   "allowPasswordAuthentication": true,
    ///   "allowSsoAuthentication": false,
    ///   "allowTokenAuthentication": true,
    ///   "enforceIpAllowlist": false,
    ///   "ipAllowlist": null,
    ///   "ipDenylist": null,
    ///   "restrictByEnvironment": false,
    ///   "allowedEnvironments": null,
    ///   "changeDescription": "Strengthened password policy per IT audit recommendations"
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Complete security policy configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly applied security policy with version number and effective timestamp</returns>
    /// <response code="200">Security policy applied successfully — new version is now active</response>
    /// <response code="400">Validation failed (UC-011 S06a) — see error details for specific issues</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not authorized — requires CanManageSecuritySettings permission</response>
    [HttpPut]
    [Authorize(Policy = "CanManageSecuritySettings")]
    [ProducesResponseType(typeof(SecurityPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SecurityPolicyDto>> UpdateSecuritySettings(
        [FromBody] UpdateSecuritySettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Applying new security settings. Change: {ChangeDescription}",
            command.ChangeDescription ?? "(no description)");

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
