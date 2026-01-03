using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Audit Log entity - comprehensive audit trail for all system actions
/// Referenced in FSD section 13: Security & Compliance - Audit Trail Requirements
/// Tracks all changes with 10+ year retention requirement
/// </summary>
public class AuditLog : BaseEntity
{
    // ==================== AUDIT IDENTIFICATION ====================

    /// <summary>
    /// Audit log entry number (sequential)
    /// </summary>
    public long AuditLogNumber { get; private set; }

    /// <summary>
    /// Timestamp when action occurred (UTC)
    /// </summary>
    public DateTime Timestamp { get; private set; }

    // ==================== ACTION DETAILS ====================

    /// <summary>
    /// Type of action performed (Create, Update, Delete, Login, etc.)
    /// </summary>
    public AuditActionType ActionType { get; private set; }

    /// <summary>
    /// Human-readable description of the action
    /// </summary>
    public string ActionDescription { get; private set; }

    /// <summary>
    /// Result of the action (Success, Failed, Partial)
    /// </summary>
    public string ActionResult { get; private set; }

    // ==================== USER INFORMATION ====================

    /// <summary>
    /// User who performed the action
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Username at the time of action
    /// </summary>
    public string Username { get; private set; }

    /// <summary>
    /// User's role at the time of action
    /// </summary>
    public string UserRole { get; private set; }

    /// <summary>
    /// Full name of user (for historical record)
    /// </summary>
    public string UserFullName { get; private set; }

    // ==================== ENTITY INFORMATION ====================

    /// <summary>
    /// Type of entity affected (e.g., "Claim", "Building", "Person")
    /// </summary>
    public string? EntityType { get; private set; }

    /// <summary>
    /// ID of the entity affected
    /// </summary>
    public Guid? EntityId { get; private set; }

    /// <summary>
    /// Human-readable identifier of entity (e.g., Claim Number, Building ID)
    /// </summary>
    public string? EntityIdentifier { get; private set; }

    // ==================== CHANGE TRACKING ====================

    /// <summary>
    /// Previous state/value (before change) stored as JSON
    /// For Update actions
    /// </summary>
    public string? OldValues { get; private set; }

    /// <summary>
    /// New state/value (after change) stored as JSON
    /// For Create and Update actions
    /// </summary>
    public string? NewValues { get; private set; }

    /// <summary>
    /// Specific fields that were changed (comma-separated)
    /// </summary>
    public string? ChangedFields { get; private set; }

    // ==================== REQUEST CONTEXT ====================

    /// <summary>
    /// IP address from which action was performed
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// User agent (browser/app information)
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Source application (Mobile, Desktop, API)
    /// </summary>
    public string? SourceApplication { get; private set; }

    /// <summary>
    /// Device ID (for mobile/tablet actions)
    /// </summary>
    public string? DeviceId { get; private set; }

    /// <summary>
    /// Session ID
    /// </summary>
    public string? SessionId { get; private set; }

    // ==================== ADDITIONAL CONTEXT ====================

    /// <summary>
    /// Additional contextual information stored as JSON
    /// E.g., import package details, export parameters, etc.
    /// </summary>
    public string? AdditionalData { get; private set; }

    /// <summary>
    /// Error message if action failed
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Stack trace if action failed (for debugging)
    /// </summary>
    public string? StackTrace { get; private set; }

    // ==================== CORRELATION ====================

    /// <summary>
    /// Correlation ID to group related actions
    /// E.g., all changes in a single import operation
    /// </summary>
    public Guid? CorrelationId { get; private set; }

    /// <summary>
    /// Parent audit log ID (for nested/related actions)
    /// </summary>
    public Guid? ParentAuditLogId { get; private set; }

    // ==================== COMPLIANCE ====================

    /// <summary>
    /// Indicates if this is a security-sensitive action
    /// </summary>
    public bool IsSecuritySensitive { get; private set; }

    /// <summary>
    /// Indicates if this action requires legal retention
    /// </summary>
    public bool RequiresLegalRetention { get; private set; }

    /// <summary>
    /// Retention end date (for legal hold requirements)
    /// Minimum 10 years from claim closure per FSD
    /// </summary>
    public DateTime? RetentionEndDate { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Parent audit log (if applicable)
    /// </summary>
    public virtual AuditLog? ParentAuditLog { get; private set; }

    // Note: User would be from User entity
    // public virtual User User { get; private set; } = null!;

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private AuditLog() : base()
    {
        ActionDescription = string.Empty;
        ActionResult = "Success";
        Username = string.Empty;
        UserRole = string.Empty;
        UserFullName = string.Empty;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Create new audit log entry
    /// </summary>
    public static AuditLog Create(
        long auditLogNumber,
        AuditActionType actionType,
        string actionDescription,
        Guid userId,
        string username,
        string userRole,
        string userFullName,
        string? entityType,
        Guid? entityId,
        string? entityIdentifier,
        string? oldValues,
        string? newValues,
        string? changedFields,
        string actionResult = "Success")
    {
        var auditLog = new AuditLog
        {
            AuditLogNumber = auditLogNumber,
            Timestamp = DateTime.UtcNow,
            ActionType = actionType,
            ActionDescription = actionDescription,
            ActionResult = actionResult,
            UserId = userId,
            Username = username,
            UserRole = userRole,
            UserFullName = userFullName,
            EntityType = entityType,
            EntityId = entityId,
            EntityIdentifier = entityIdentifier,
            OldValues = oldValues,
            NewValues = newValues,
            ChangedFields = changedFields,
            IsSecuritySensitive = IsSecuritySensitiveAction(actionType),
            RequiresLegalRetention = RequiresLegalRetentionCheck(actionType, entityType)
        };

        // Set retention end date (10 years for legal retention)
        if (auditLog.RequiresLegalRetention)
        {
            auditLog.RetentionEndDate = DateTime.UtcNow.AddYears(10);
        }

        return auditLog;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Set request context information
    /// </summary>
    public void SetRequestContext(
        string? ipAddress,
        string? userAgent,
        string? sourceApplication,
        string? deviceId,
        string? sessionId)
    {
        IpAddress = ipAddress;
        UserAgent = userAgent;
        SourceApplication = sourceApplication;
        DeviceId = deviceId;
        SessionId = sessionId;
    }

    /// <summary>
    /// Set correlation ID for grouping related actions
    /// </summary>
    public void SetCorrelation(Guid correlationId, Guid? parentAuditLogId = null)
    {
        CorrelationId = correlationId;
        ParentAuditLogId = parentAuditLogId;
    }

    /// <summary>
    /// Add additional context data
    /// </summary>
    public void SetAdditionalData(string additionalDataJson)
    {
        AdditionalData = additionalDataJson;
    }

    /// <summary>
    /// Record error information for failed actions
    /// </summary>
    public void RecordError(string errorMessage, string? stackTrace = null)
    {
        ActionResult = "Failed";
        ErrorMessage = errorMessage;
        StackTrace = stackTrace;
    }

    /// <summary>
    /// Extend retention period (for legal hold)
    /// </summary>
    public void ExtendRetention(DateTime newRetentionEndDate)
    {
        if (newRetentionEndDate > (RetentionEndDate ?? DateTime.MinValue))
        {
            RetentionEndDate = newRetentionEndDate;
        }
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Check if action type is security-sensitive
    /// </summary>
    private static bool IsSecuritySensitiveAction(AuditActionType actionType)
    {
        return actionType switch
        {
            AuditActionType.Login => true,
            AuditActionType.Logout => true,
            AuditActionType.LoginFailed => true,
            AuditActionType.PasswordChange => true,
            AuditActionType.PermissionGranted => true,
            AuditActionType.PermissionRevoked => true,
            AuditActionType.AccessDenied => true,
            AuditActionType.UserCreated => true,
            AuditActionType.UserDeactivated => true,
            AuditActionType.RoleAssigned => true,
            AuditActionType.ConfigurationChange => true,
            _ => false
        };
    }

    /// <summary>
    /// Check if action requires legal retention
    /// </summary>
    private static bool RequiresLegalRetentionCheck(AuditActionType actionType, string? entityType)
    {
        // Claims and certificates require 10+ year retention
        if (entityType == "Claim" || entityType == "Certificate")
            return true;

        // Certain action types always require legal retention
        return actionType switch
        {
            AuditActionType.Approve => true,
            AuditActionType.Reject => true,
            AuditActionType.CertificateIssued => true,
            AuditActionType.CertificateVoided => true,
            AuditActionType.Merge => true,
            _ => false
        };
    }
}