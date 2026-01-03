using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// User entity - represents system users with authentication and authorization
/// </summary>
public class User : BaseAuditableEntity
{
    // ==================== USER IDENTIFICATION ====================

    /// <summary>
    /// Username for login (unique) (اسم المستخدم)
    /// </summary>
    public string Username { get; private set; }

    /// <summary>
    /// Email address (unique, optional)
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Password hash (never store plain text passwords)
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Salt used for password hashing
    /// </summary>
    public string PasswordSalt { get; private set; }

    // ==================== PERSONAL INFORMATION ====================

    /// <summary>
    /// Full name in Arabic (الاسم الكامل)
    /// </summary>
    public string FullNameArabic { get; private set; }

    /// <summary>
    /// Full name in English (optional)
    /// </summary>
    public string? FullNameEnglish { get; private set; }

    /// <summary>
    /// Employee ID or staff number (optional)
    /// </summary>
    public string? EmployeeId { get; private set; }

    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Organization/department (e.g., "UN-Habitat Aleppo", "Municipality")
    /// </summary>
    public string? Organization { get; private set; }

    /// <summary>
    /// Job title (e.g., "Field Collector", "Data Manager")
    /// </summary>
    public string? JobTitle { get; private set; }

    // ==================== ROLE & PERMISSIONS ====================

    /// <summary>
    /// Primary user role (FieldCollector, DataManager, Administrator, etc.)
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Additional roles or permissions (stored as JSON array)
    /// For users with multiple roles
    /// </summary>
    public string? AdditionalRoles { get; private set; }

    /// <summary>
    /// Indicates if user has mobile app access
    /// </summary>
    public bool HasMobileAccess { get; private set; }

    /// <summary>
    /// Indicates if user has desktop app access
    /// </summary>
    public bool HasDesktopAccess { get; private set; }

    // ==================== ACCOUNT STATUS ====================

    /// <summary>
    /// Indicates if user account is active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Indicates if user is locked out (due to failed login attempts)
    /// </summary>
    public bool IsLockedOut { get; private set; }

    /// <summary>
    /// Lockout end date (when lockout expires)
    /// </summary>
    public DateTime? LockoutEndDate { get; private set; }

    /// <summary>
    /// Failed login attempts count
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// Date of last failed login attempt
    /// </summary>
    public DateTime? LastFailedLoginDate { get; private set; }

    // ==================== LOGIN TRACKING ====================

    /// <summary>
    /// Date of last successful login
    /// </summary>
    public DateTime? LastLoginDate { get; private set; }

    /// <summary>
    /// Date of last password change
    /// </summary>
    public DateTime? LastPasswordChangeDate { get; private set; }

    /// <summary>
    /// Indicates if user must change password on next login
    /// </summary>
    public bool MustChangePassword { get; private set; }

    /// <summary>
    /// Password expiry date (if password expiration is enforced)
    /// </summary>
    public DateTime? PasswordExpiryDate { get; private set; }

    // ==================== TABLET ASSIGNMENT (for Field Collectors) ====================

    /// <summary>
    /// Tablet device ID assigned to this user (for field collectors)
    /// </summary>
    public string? AssignedTabletId { get; private set; }

    /// <summary>
    /// Date when tablet was assigned
    /// </summary>
    public DateTime? TabletAssignedDate { get; private set; }

    // ==================== SUPERVISION (for Field Teams) ====================

    /// <summary>
    /// Supervisor user ID (if this user reports to someone)
    /// </summary>
    public Guid? SupervisorUserId { get; private set; }

    /// <summary>
    /// Team name or identifier
    /// </summary>
    public string? TeamName { get; private set; }

    // ==================== PREFERENCES ====================

    /// <summary>
    /// Preferred language (ar, en)
    /// </summary>
    public string PreferredLanguage { get; private set; }

    /// <summary>
    /// User preferences stored as JSON
    /// </summary>
    public string? Preferences { get; private set; }

    // ==================== SECURITY ====================

    /// <summary>
    /// Security stamp - changes when password or security settings change
    /// </summary>
    public string SecurityStamp { get; private set; }

    /// <summary>
    /// Two-factor authentication enabled
    /// </summary>
    public bool TwoFactorEnabled { get; private set; }

    /// <summary>
    /// Refresh token for JWT authentication
    /// </summary>
    public string? RefreshToken { get; private set; }

    /// <summary>
    /// Refresh token expiry date
    /// </summary>
    public DateTime? RefreshTokenExpiryDate { get; private set; }

    // ==================== NOTES ====================

    /// <summary>
    /// Administrative notes about this user
    /// </summary>
    public string? Notes { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Supervisor (if applicable)
    /// </summary>
    public virtual User? Supervisor { get; private set; }

    /// <summary>
    /// Users supervised by this user (if they are a supervisor)
    /// </summary>
    public virtual ICollection<User> Supervisees { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private User() : base()
    {
        Username = string.Empty;
        PasswordHash = string.Empty;
        PasswordSalt = string.Empty;
        FullNameArabic = string.Empty;
        SecurityStamp = Guid.NewGuid().ToString();
        PreferredLanguage = "ar"; // Default to Arabic
        IsActive = true;
        IsLockedOut = false;
        MustChangePassword = false;
        TwoFactorEnabled = false;
        FailedLoginAttempts = 0;
        HasMobileAccess = false;
        HasDesktopAccess = false;
        Supervisees = new List<User>();
    }

    /// <summary>
    /// Create new user
    /// </summary>
    public static User Create(
        string username,
        string fullNameArabic,
        string passwordHash,
        string passwordSalt,
        UserRole role,
        bool hasMobileAccess,
        bool hasDesktopAccess,
        string? email,
        string? phoneNumber,
        Guid createdByUserId)
    {
        var user = new User
        {
            Username = username,
            FullNameArabic = fullNameArabic,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Role = role,
            Email = email,
            PhoneNumber = phoneNumber,
            HasMobileAccess = hasMobileAccess,
            HasDesktopAccess = hasDesktopAccess,
            IsActive = true,
            IsLockedOut = false,
            MustChangePassword = true, // Force password change on first login
            SecurityStamp = Guid.NewGuid().ToString(),
            PreferredLanguage = "ar",
            FailedLoginAttempts = 0,
            TwoFactorEnabled = false
        };

        user.MarkAsCreated(createdByUserId);

        return user;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Update user profile information
    /// </summary>
    public void UpdateProfile(
        string fullNameArabic,
        string? fullNameEnglish,
        string? email,
        string? phoneNumber,
        string? organization,
        string? jobTitle,
        Guid modifiedByUserId)
    {
        FullNameArabic = fullNameArabic;
        FullNameEnglish = fullNameEnglish;
        Email = email;
        PhoneNumber = phoneNumber;
        Organization = organization;
        JobTitle = jobTitle;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Change password
    /// </summary>
    public void ChangePassword(string newPasswordHash, string newPasswordSalt, Guid modifiedByUserId)
    {
        PasswordHash = newPasswordHash;
        PasswordSalt = newPasswordSalt;
        LastPasswordChangeDate = DateTime.UtcNow;
        MustChangePassword = false;
        SecurityStamp = Guid.NewGuid().ToString(); // Invalidate existing tokens
        FailedLoginAttempts = 0; // Reset failed attempts
        IsLockedOut = false; // Unlock if locked
        LockoutEndDate = null;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Record successful login
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginDate = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        IsLockedOut = false;
        LockoutEndDate = null;
    }

    /// <summary>
    /// Record failed login attempt
    /// </summary>
    public void RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 30)
    {
        FailedLoginAttempts++;
        LastFailedLoginDate = DateTime.UtcNow;

        if (FailedLoginAttempts >= maxAttempts)
        {
            IsLockedOut = true;
            LockoutEndDate = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }
    }

    /// <summary>
    /// Unlock user account
    /// </summary>
    public void Unlock(Guid modifiedByUserId)
    {
        IsLockedOut = false;
        LockoutEndDate = null;
        FailedLoginAttempts = 0;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Deactivate user account
    /// </summary>
    public void Deactivate(string reason, Guid modifiedByUserId)
    {
        IsActive = false;
        Notes = string.IsNullOrWhiteSpace(Notes)
            ? $"[Deactivated]: {reason}"
            : $"{Notes}\n[Deactivated]: {reason}";
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Reactivate user account
    /// </summary>
    public void Reactivate(Guid modifiedByUserId)
    {
        IsActive = true;
        IsLockedOut = false;
        LockoutEndDate = null;
        FailedLoginAttempts = 0;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update user role
    /// </summary>
    public void UpdateRole(UserRole newRole, Guid modifiedByUserId)
    {
        Role = newRole;

        // Adjust access based on role
        switch (newRole)
        {
            case UserRole.FieldCollector:
                HasMobileAccess = true;
                HasDesktopAccess = false;
                break;
            case UserRole.OfficeClerk:
            case UserRole.DataManager:
            case UserRole.Administrator:
                HasMobileAccess = false;
                HasDesktopAccess = true;
                break;
            case UserRole.FieldSupervisor:
            case UserRole.Analyst:
                HasMobileAccess = false;
                HasDesktopAccess = true;
                break;
        }

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Assign tablet to user
    /// </summary>
    public void AssignTablet(string tabletId, Guid modifiedByUserId)
    {
        AssignedTabletId = tabletId;
        TabletAssignedDate = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Remove tablet assignment
    /// </summary>
    public void RemoveTablet(Guid modifiedByUserId)
    {
        AssignedTabletId = null;
        TabletAssignedDate = null;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Assign supervisor
    /// </summary>
    public void AssignSupervisor(Guid supervisorUserId, Guid modifiedByUserId)
    {
        SupervisorUserId = supervisorUserId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set team
    /// </summary>
    public void SetTeam(string teamName, Guid modifiedByUserId)
    {
        TeamName = teamName;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update refresh token
    /// </summary>
    public void UpdateRefreshToken(string refreshToken, DateTime expiryDate)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryDate = expiryDate;
    }

    /// <summary>
    /// Clear refresh token (on logout)
    /// </summary>
    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryDate = null;
    }

    /// <summary>
    /// Enable two-factor authentication
    /// </summary>
    public void EnableTwoFactor(Guid modifiedByUserId)
    {
        TwoFactorEnabled = true;
        SecurityStamp = Guid.NewGuid().ToString();
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Disable two-factor authentication
    /// </summary>
    public void DisableTwoFactor(Guid modifiedByUserId)
    {
        TwoFactorEnabled = false;
        SecurityStamp = Guid.NewGuid().ToString();
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Check if lockout has expired
    /// </summary>
    public bool IsLockoutExpired()
    {
        if (!IsLockedOut || !LockoutEndDate.HasValue)
            return true;

        return DateTime.UtcNow > LockoutEndDate.Value;
    }

    /// <summary>
    /// Check if password is expired
    /// </summary>
    public bool IsPasswordExpired()
    {
        if (!PasswordExpiryDate.HasValue)
            return false;

        return DateTime.UtcNow > PasswordExpiryDate.Value;
    }
}