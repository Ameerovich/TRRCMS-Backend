using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Users.Commands.ActivateUser;
using TRRCMS.Application.Users.Commands.CreateUser;
using TRRCMS.Application.Users.Commands.DeactivateUser;
using TRRCMS.Application.Users.Commands.GrantPermissions;
using TRRCMS.Application.Users.Commands.RevokePermission;
using TRRCMS.Application.Users.Commands.UnlockUser;
using TRRCMS.Application.Users.Commands.UpdateUser;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Application.Users.Queries.GetAllUsers;
using TRRCMS.Application.Users.Queries.GetUser;
using TRRCMS.Application.Users.Queries.GetUserAuditLog;
using TRRCMS.Domain.Enums;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// User management API for system administration
/// </summary>
/// <remarks>
/// Manages system users, roles, and permissions for TRRCMS.
/// إدارة المستخدمين - UC-009 User &amp; Role Management
/// 
/// **What is a User?**
/// A User represents a system account that can authenticate and access TRRCMS.
/// Users have roles that determine their base permissions, plus additional
/// permissions can be granted individually.
/// 
/// **User Lifecycle:**
/// 1. Create user with role and initial password
/// 2. User logs in and changes password
/// 3. Grant/revoke permissions as needed
/// 4. Deactivate when no longer needed
/// 5. Reactivate if needed later
/// 
/// **UserRole Values (دور المستخدم):**
/// 
/// | Value | Name | Arabic | Access | Description |
/// |-------|------|--------|--------|-------------|
/// | 1 | FieldCollector | جامع بيانات | Mobile only | Field data collection |
/// | 2 | FieldSupervisor | مشرف ميداني | Desktop (read) | Supervise field teams |
/// | 3 | OfficeClerk | موظف مكتب | Desktop (full) | Claim registration |
/// | 4 | DataManager | مدير بيانات | Desktop (full) | Data validation/import |
/// | 5 | Analyst | محلل | Desktop (read) | Reporting/analytics |
/// | 6 | Administrator | مدير النظام | Full | Full system access |
/// 
/// **Account Status:**
/// - `isActive`: Whether the account can be used
/// - `isLockedOut`: Temporarily locked due to failed login attempts
/// - `mustChangePassword`: User must change password on next login
/// 
/// **Permissions:**
/// - View: Users_View (8000)
/// - Create: Users_Create (8001)
/// - Update: Users_Update (8002)
/// - Deactivate: Users_Deactivate (8003)
/// 
/// **Security Notes:**
/// - Passwords are hashed before storage
/// - Failed logins are tracked and can trigger lockout
/// - All changes are logged for audit trail
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ==================== CREATE ====================

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <remarks>
    /// Creates a new system user with specified role and permissions.
    /// إنشاء مستخدم جديد
    /// 
    /// **Use Case**: UC-009 User Management - Create new user accounts
    /// 
    /// **Required Permission**: Users_Create (8001) - CanCreateUsers policy
    /// 
    /// **Required Fields:**
    /// - `username`: Login username (unique)
    /// - `email`: Email address (unique)
    /// - `fullNameArabic`: Full name in Arabic
    /// - `role`: User role (1-6)
    /// - `password`: Initial password (will be hashed)
    /// 
    /// **Optional Fields:**
    /// - `fullNameEnglish`: Full name in English
    /// - `phoneNumber`: Contact phone
    /// - `organization`: Organization/department
    /// - `jobTitle`: Job title
    /// - `employeeId`: Employee ID
    /// - `hasMobileAccess`: Allow mobile app access
    /// - `hasDesktopAccess`: Allow desktop app access
    /// 
    /// **Password Requirements:**
    /// - Minimum 8 characters
    /// - At least one uppercase, one lowercase, one digit, one special character
    /// 
    /// **Example Request - Field Collector:**
    /// ```json
    /// {
    ///   "username": "fc001",
    ///   "email": "fc001@trrcms.org",
    ///   "fullNameArabic": "أحمد محمد علي",
    ///   "fullNameEnglish": "Ahmed Mohammed Ali",
    ///   "phoneNumber": "+963 991 234 567",
    ///   "role": 1,
    ///   "password": "TempPass123!",
    ///   "hasMobileAccess": true,
    ///   "hasDesktopAccess": false,
    ///   "organization": "فريق حلب الميداني",
    ///   "jobTitle": "جامع بيانات"
    /// }
    /// ```
    /// 
    /// **Example Request - Administrator:**
    /// ```json
    /// {
    ///   "username": "admin.ali",
    ///   "email": "ali.admin@trrcms.org",
    ///   "fullNameArabic": "علي حسن أحمد",
    ///   "role": 6,
    ///   "password": "SecureAdmin123!",
    ///   "hasMobileAccess": true,
    ///   "hasDesktopAccess": true,
    ///   "organization": "إدارة النظام",
    ///   "jobTitle": "مدير النظام"
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "username": "fc001",
    ///   "email": "fc001@trrcms.org",
    ///   "fullNameArabic": "أحمد محمد علي",
    ///   "fullNameEnglish": "Ahmed Mohammed Ali",
    ///   "phoneNumber": "+963 991 234 567",
    ///   "role": 1,
    ///   "roleName": "FieldCollector",
    ///   "hasMobileAccess": true,
    ///   "hasDesktopAccess": false,
    ///   "isActive": true,
    ///   "isLockedOut": false,
    ///   "mustChangePassword": true,
    ///   "createdAtUtc": "2026-01-31T10:00:00Z"
    /// }
    /// ```
    /// 
    /// **Note:** New users have `mustChangePassword: true` by default.
    /// </remarks>
    /// <param name="command">User creation data</param>
    /// <returns>Created user with generated ID</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Validation error - check required fields, password requirements</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Users_Create (8001) permission</response>
    /// <response code="409">Username or email already exists</response>
    [HttpPost]
    [Authorize(Policy = "CanCreateUsers")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
    }

    // ==================== GET BY ID ====================

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <remarks>
    /// Retrieves detailed user information including permissions.
    /// عرض تفاصيل المستخدم
    /// 
    /// **Use Case**: UC-009 - View user details, verify permissions
    /// 
    /// **Required Permission**: Users_View (8000) - CanViewAllUsers policy
    /// 
    /// **Response includes:**
    /// - User profile (names, contact info)
    /// - Role and permissions list
    /// - Account status (active, locked, must change password)
    /// - Login history (last login, failed attempts)
    /// - Assignment info (tablet, supervisor, team)
    /// - Audit trail (created/modified timestamps)
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "username": "fc001",
    ///   "email": "fc001@trrcms.org",
    ///   "fullNameArabic": "أحمد محمد علي",
    ///   "fullNameEnglish": "Ahmed Mohammed Ali",
    ///   "phoneNumber": "+963 991 234 567",
    ///   "role": 1,
    ///   "roleName": "FieldCollector",
    ///   "hasMobileAccess": true,
    ///   "hasDesktopAccess": false,
    ///   "isActive": true,
    ///   "isLockedOut": false,
    ///   "lockoutEndDate": null,
    ///   "failedLoginAttempts": 0,
    ///   "lastLoginDate": "2026-01-31T08:00:00Z",
    ///   "lastPasswordChangeDate": "2026-01-15T10:00:00Z",
    ///   "mustChangePassword": false,
    ///   "assignedTabletId": "TABLET-FC-001",
    ///   "tabletAssignedDate": "2026-01-01T00:00:00Z",
    ///   "supervisorUserId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "supervisorName": "محمد خالد",
    ///   "teamName": "فريق حلب",
    ///   "permissions": [
    ///     "Surveys_Create",
    ///     "Surveys_ViewOwn",
    ///     "Surveys_EditOwn",
    ///     "Evidence_Upload",
    ///     "Evidence_View"
    ///   ],
    ///   "activePermissionsCount": 5,
    ///   "createdAtUtc": "2026-01-01T10:00:00Z",
    ///   "lastModifiedAtUtc": "2026-01-15T10:00:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">User ID (GUID)</param>
    /// <returns>User details with permissions</returns>
    /// <response code="200">User found and returned</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Users_View (8000) permission</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewAllUsers")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailDto>> GetUser(Guid id)
    {
        var result = await _mediator.Send(new GetUserQuery { UserId = id });
        return Ok(result);
    }

    // ==================== GET ALL ====================

    /// <summary>
    /// Get all users with filters and pagination
    /// </summary>
    /// <remarks>
    /// Retrieves paginated list of users with optional filters.
    /// عرض قائمة المستخدمين
    /// 
    /// **Use Case**: UC-009 - User listing, search, administration
    /// 
    /// **Required Permission**: Users_View (8000) - CanViewAllUsers policy
    /// 
    /// **Filter Options:**
    /// - `role`: Filter by role (1-6)
    /// - `isActive`: Filter by active status (true/false)
    /// - `searchTerm`: Search in username, name, email
    /// 
    /// **Pagination:**
    /// - `page`: Page number (default: 1)
    /// - `pageSize`: Items per page (default: 20)
    /// 
    /// **Example Request:**
    /// ```
    /// GET /api/v1/Users?role=1&amp;isActive=true&amp;searchTerm=أحمد&amp;page=1&amp;pageSize=10
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "users": [
    ///     {
    ///       "id": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///       "username": "fc001",
    ///       "fullNameArabic": "أحمد محمد علي",
    ///       "role": 1,
    ///       "roleName": "FieldCollector",
    ///       "isActive": true,
    ///       "lastLoginDate": "2026-01-31T08:00:00Z"
    ///     }
    ///   ],
    ///   "totalCount": 45,
    ///   "page": 1,
    ///   "pageSize": 10,
    ///   "totalPages": 5
    /// }
    /// ```
    /// </remarks>
    /// <param name="query">Filter and pagination parameters</param>
    /// <returns>Paginated list of users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Users_View (8000) permission</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllUsers")]
    [ProducesResponseType(typeof(PagedResult<UserListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<UserListDto>>> GetAllUsers([FromQuery] GetAllUsersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ==================== UPDATE ====================

    /// <summary>
    /// Update user details
    /// </summary>
    /// <remarks>
    /// Updates user profile information. Does not change password or permissions.
    /// تحديث بيانات المستخدم
    /// 
    /// **Use Case**: UC-009 - Update user profile, contact info, access settings
    /// 
    /// **Required Permission**: Users_Update (8002) - CanEditUsers policy
    /// 
    /// **Updatable Fields:**
    /// - Contact: email, phoneNumber
    /// - Names: fullNameArabic, fullNameEnglish
    /// - Organization: organization, jobTitle, employeeId
    /// - Access: hasMobileAccess, hasDesktopAccess
    /// - Assignment: supervisorUserId, teamName, assignedTabletId
    /// 
    /// **Cannot Change:**
    /// - username (immutable)
    /// - role (use separate endpoint or recreate user)
    /// - password (use change password endpoint)
    /// - permissions (use grant/revoke endpoints)
    /// 
    /// **Example Request:**
    /// ```json
    /// {
    ///   "email": "ahmed.new@trrcms.org",
    ///   "phoneNumber": "+963 992 345 678",
    ///   "organization": "فريق دمشق الميداني",
    ///   "supervisorUserId": "8cd03f62-9345-5234-b2cd-0e963g44bgc8"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">User ID to update</param>
    /// <param name="command">Update data (only include fields to change)</param>
    /// <returns>Updated user</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Validation error</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Users_Update (8002) permission</response>
    /// <response code="404">User not found</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "CanEditUsers")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        command.UserId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==================== ACTIVATE ====================

    /// <summary>
    /// Activate a user account
    /// </summary>
    /// <remarks>
    /// Reactivates a previously deactivated user account.
    /// تفعيل حساب المستخدم
    /// 
    /// **Use Case**: UC-009 - Reactivate user who was deactivated
    /// 
    /// **Required Permission**: Users_Update (8002) - CanEditUsers policy
    /// 
    /// **When to use:**
    /// - User returns after leave
    /// - Account was deactivated by mistake
    /// - Temporary deactivation period ended
    /// 
    /// **Effect:**
    /// - Sets `isActive = true`
    /// - User can login again
    /// - All permissions remain as before deactivation
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "username": "fc001",
    ///   "isActive": true,
    ///   "lastModifiedAtUtc": "2026-01-31T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">User ID to activate</param>
    /// <returns>Activated user</returns>
    /// <response code="200">User activated successfully</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Users_Update (8002) permission</response>
    /// <response code="404">User not found</response>
    [HttpPut("{id}/activate")]
    [Authorize(Policy = "CanEditUsers")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> ActivateUser(Guid id)
    {
        var result = await _mediator.Send(new ActivateUserCommand { UserId = id });
        return Ok(result);
    }

    // ==================== DEACTIVATE ====================

    /// <summary>
    /// Deactivate a user account
    /// </summary>
    /// <remarks>
    /// Deactivates a user account preventing login.
    /// إلغاء تفعيل حساب المستخدم
    /// 
    /// **Use Case**: UC-009 - Disable user access without deleting account
    /// 
    /// **Required Permission**: Users_Deactivate (8003) - CanEditUsers policy
    /// 
    /// **When to use:**
    /// - Employee leaves organization
    /// - Security concern
    /// - Extended leave of absence
    /// - Contract ends
    /// 
    /// **Effect:**
    /// - Sets `isActive = false`
    /// - User cannot login
    /// - Existing sessions are invalidated
    /// - Permissions remain but are not usable
    /// 
    /// **Deactivation Reason Required:**
    /// A reason must be provided for audit trail.
    /// 
    /// **Example Request:**
    /// ```json
    /// {
    ///   "reason": "انتهاء عقد العمل - End of employment contract"
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "username": "fc001",
    ///   "isActive": false,
    ///   "lastModifiedAtUtc": "2026-01-31T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">User ID to deactivate</param>
    /// <param name="command">Deactivation reason</param>
    /// <returns>Deactivated user</returns>
    /// <response code="200">User deactivated successfully</response>
    /// <response code="400">Reason is required</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Users_Deactivate (8003) permission</response>
    /// <response code="404">User not found</response>
    [HttpPut("{id}/deactivate")]
    [Authorize(Policy = "CanEditUsers")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> DeactivateUser(Guid id, [FromBody] DeactivateUserCommand command)
    {
        command.UserId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==================== UNLOCK ====================

    /// <summary>
    /// Unlock a locked user account
    /// </summary>
    /// <remarks>
    /// Unlocks a user account that was locked due to failed login attempts.
    /// فتح حساب المستخدم المقفل
    /// 
    /// **Use Case**: UC-009 - Unlock user locked by failed password attempts
    /// 
    /// **Required Permission**: Users_Update (8002) - CanEditUsers policy
    /// 
    /// **When to use:**
    /// - User forgot password and got locked out
    /// - Lockout period should be ended early
    /// 
    /// **Effect:**
    /// - Sets `isLockedOut = false`
    /// - Clears `lockoutEndDate`
    /// - Resets `failedLoginAttempts = 0`
    /// - User can attempt login again
    /// 
    /// **Note:** This does NOT reset the password. User still needs correct password.
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "username": "fc001",
    ///   "isLockedOut": false,
    ///   "lockoutEndDate": null,
    ///   "failedLoginAttempts": 0,
    ///   "lastModifiedAtUtc": "2026-01-31T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">User ID to unlock</param>
    /// <returns>Unlocked user</returns>
    /// <response code="200">User unlocked successfully</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Users_Update (8002) permission</response>
    /// <response code="404">User not found</response>
    [HttpPut("{id}/unlock")]
    [Authorize(Policy = "CanEditUsers")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UnlockUser(Guid id)
    {
        var result = await _mediator.Send(new UnlockUserCommand { UserId = id });
        return Ok(result);
    }

    // ==================== GRANT PERMISSIONS ====================

    /// <summary>
    /// Grant permissions to a user
    /// </summary>
    /// <remarks>
    /// Grants one or more permissions to a user in addition to their role-based permissions.
    /// منح صلاحيات للمستخدم
    /// 
    /// **Use Case**: UC-009 - Add specific permissions beyond role defaults
    /// 
    /// **Required Permission**: CanManageUserRoles policy (Administrator only)
    /// 
    /// **When to use:**
    /// - User needs temporary elevated access
    /// - Role doesn't include a needed permission
    /// - Special project requirements
    /// 
    /// **Common Permission Groups:**
    /// - Claims: Claims_ViewAll (1000), Claims_Create (1002), Claims_Approve (1009)
    /// - Evidence: Evidence_View (2000), Evidence_Upload (2001), Evidence_Verify (2002)
    /// - Documents: Documents_ViewSensitive (3000), Documents_Upload (3002)
    /// - Buildings: Buildings_View (4000), Buildings_Create (4001)
    /// - PropertyUnits: PropertyUnits_View (6000), PropertyUnits_Create (6001)
    /// - Surveys: Surveys_Create (7000), Surveys_ViewAll (7004)
    /// - Users: Users_View (8000), Users_Create (8001)
    /// 
    /// **Example Request:**
    /// ```json
    /// {
    ///   "permissions": [2000, 2001, 2002],
    ///   "grantReason": "مطلوب للمشروع الجديد - Required for new project"
    /// }
    /// ```
    /// 
    /// **Example Response:**
    /// ```json
    /// {
    ///   "id": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "username": "fc001",
    ///   "lastModifiedAtUtc": "2026-01-31T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">User ID to grant permissions to</param>
    /// <param name="command">Permissions to grant with reason</param>
    /// <returns>Updated user</returns>
    /// <response code="200">Permissions granted successfully</response>
    /// <response code="400">Invalid permission values or missing reason</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires CanManageUserRoles (Administrator)</response>
    /// <response code="404">User not found</response>
    [HttpPost("{id}/permissions")]
    [Authorize(Policy = "CanManageUserRoles")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GrantPermissions(Guid id, [FromBody] GrantPermissionsCommand command)
    {
        command.UserId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ==================== REVOKE PERMISSION ====================

    /// <summary>
    /// Revoke a permission from a user
    /// </summary>
    /// <remarks>
    /// Removes a specific permission that was previously granted to a user.
    /// إلغاء صلاحية من المستخدم
    /// 
    /// **Use Case**: UC-009 - Remove specific permission from user
    /// 
    /// **Required Permission**: CanManageUserRoles policy (Administrator only)
    /// 
    /// **When to use:**
    /// - Temporary access period ended
    /// - User changed responsibilities
    /// - Security review
    /// 
    /// **Permission Format:**
    /// Pass permission name in URL: `Evidence_Upload`, `Claims_Approve`, etc.
    /// 
    /// **Example Request:**
    /// ```
    /// DELETE /api/v1/Users/{id}/permissions/Evidence_Upload
    /// Body: { "reason": "انتهاء المشروع - Project completed" }
    /// ```
    /// 
    /// **Note:** Role-based permissions cannot be revoked - only additionally granted ones.
    /// </remarks>
    /// <param name="id">User ID</param>
    /// <param name="permission">Permission name (e.g., "Evidence_Upload")</param>
    /// <param name="command">Revocation reason</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Permission revoked successfully</response>
    /// <response code="400">Invalid permission name</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires CanManageUserRoles (Administrator)</response>
    /// <response code="404">User not found or permission not granted</response>
    [HttpDelete("{id}/permissions/{permission}")]
    [Authorize(Policy = "CanManageUserRoles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokePermission(
        Guid id,
        string permission,
        [FromBody] RevokePermissionCommand command)
    {
        if (!Enum.TryParse<Permission>(permission, ignoreCase: true, out var parsedPermission))
        {
            return BadRequest(new { message = $"Invalid permission value: '{permission}'." });
        }

        command.UserId = id;
        command.Permission = parsedPermission;

        await _mediator.Send(command);
        return NoContent();
    }

    // ==================== AUDIT LOG ====================

    /// <summary>
    /// Get user audit log
    /// </summary>
    /// <remarks>
    /// Retrieves the audit log for a specific user showing all changes made to their account.
    /// عرض سجل التدقيق للمستخدم
    /// 
    /// **Use Case**: UC-009 - Review user account history, security audit
    /// 
    /// **Required Permission**: CanViewAuditLogs policy
    /// 
    /// **Logged Actions:**
    /// - Account created
    /// - Profile updated
    /// - Password changed
    /// - Account activated/deactivated
    /// - Account locked/unlocked
    /// - Permissions granted/revoked
    /// - Login success/failure
    /// 
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "timestamp": "2026-01-31T14:30:00Z",
    ///     "action": "PermissionGranted",
    ///     "userName": "admin.ali",
    ///     "changes": "Granted: Evidence_Upload, Evidence_View",
    ///     "reason": "مطلوب للمشروع الجديد"
    ///   },
    ///   {
    ///     "timestamp": "2026-01-31T10:00:00Z",
    ///     "action": "AccountUnlocked",
    ///     "userName": "admin.ali",
    ///     "changes": "isLockedOut: true → false",
    ///     "reason": null
    ///   },
    ///   {
    ///     "timestamp": "2026-01-31T09:45:00Z",
    ///     "action": "LoginFailed",
    ///     "userName": "fc001",
    ///     "changes": "failedLoginAttempts: 4 → 5, isLockedOut: false → true",
    ///     "reason": null
    ///   },
    ///   {
    ///     "timestamp": "2026-01-01T10:00:00Z",
    ///     "action": "AccountCreated",
    ///     "userName": "admin.ali",
    ///     "changes": "Created with role: FieldCollector",
    ///     "reason": null
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="id">User ID to get audit log for</param>
    /// <returns>List of audit log entries</returns>
    /// <response code="200">Audit log retrieved successfully</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires CanViewAuditLogs permission</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id}/audit-log")]
    [Authorize(Policy = "CanViewAuditLogs")]
    [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AuditLogDto>>> GetUserAuditLog(Guid id)
    {
        var result = await _mediator.Send(new GetUserAuditLogQuery { UserId = id });
        return Ok(result);
    }
}