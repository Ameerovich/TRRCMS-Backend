using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service for comprehensive audit logging
/// Tracks all system actions for compliance and security
/// Referenced in FSD Section 13: Security & Compliance
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log a successful action (Create, Update, Delete, etc.)
    /// </summary>
    /// <param name="actionType">Type of action performed</param>
    /// <param name="actionDescription">Human-readable description</param>
    /// <param name="entityType">Type of entity affected (e.g., "Claim", "Building")</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="entityIdentifier">Human-readable identifier (e.g., Claim Number)</param>
    /// <param name="oldValues">Previous state (JSON) - for updates</param>
    /// <param name="newValues">New state (JSON) - for creates/updates</param>
    /// <param name="changedFields">Comma-separated list of changed fields</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogActionAsync(
        AuditActionType actionType,
        string actionDescription,
        string? entityType = null,
        Guid? entityId = null,
        string? entityIdentifier = null,
        string? oldValues = null,
        string? newValues = null,
        string? changedFields = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log a failed action with error details
    /// </summary>
    Task LogFailedActionAsync(
        AuditActionType actionType,
        string actionDescription,
        string errorMessage,
        string? stackTrace = null,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log claim state transition (for UC-006: Claim History Timeline)
    /// </summary>
    Task LogClaimTransitionAsync(
        Guid claimId,
        string claimNumber,
        LifecycleStage fromStage,
        LifecycleStage toStage,
        string? transitionReason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log security-sensitive action (login, permission changes, etc.)
    /// </summary>
    Task LogSecurityActionAsync(
        AuditActionType actionType,
        string actionDescription,
        bool isSecuritySensitive = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit history for a specific entity
    /// Supports UC-006: View claim history timeline
    /// </summary>
    Task<List<AuditLog>> GetEntityHistoryAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent audit logs for current user
    /// </summary>
    Task<List<AuditLog>> GetUserRecentActivityAsync(
        Guid userId,
        int count = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get audit logs with filters (for admin reporting)
    /// </summary>
    Task<List<AuditLog>> GetAuditLogsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? userId = null,
        string? entityType = null,
        AuditActionType? actionType = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Set correlation ID for grouping related actions
    /// Example: Group all changes in a single import operation
    /// </summary>
    void SetCorrelationId(Guid correlationId);

    /// <summary>
    /// Clear correlation ID after batch operation completes
    /// </summary>
    void ClearCorrelationId();
}
