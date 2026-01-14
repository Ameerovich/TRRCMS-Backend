using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Extensions;

/// <summary>
/// Extension methods to simplify audit logging in command handlers
/// </summary>
public static class AuditExtensions
{
    /// <summary>
    /// Log claim creation
    /// </summary>
    public static async Task LogClaimCreatedAsync(
        this IAuditService auditService,
        Claim claim,
        CancellationToken cancellationToken = default)
    {
        var newValues = JsonSerializer.Serialize(new
        {
            claim.ClaimNumber,
            claim.PropertyUnitId,
            claim.PrimaryClaimantId,
            claim.ClaimType,
            claim.ClaimSource,
            claim.Priority,
            claim.LifecycleStage
        });

        await auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Created claim {claim.ClaimNumber}",
            entityType: "Claim",
            entityId: claim.Id,
            entityIdentifier: claim.ClaimNumber,
            oldValues: null,
            newValues: newValues,
            changedFields: null,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Log claim update with field tracking
    /// </summary>
    public static async Task LogClaimUpdatedAsync(
        this IAuditService auditService,
        Guid claimId,
        string claimNumber,
        string changedFields,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default)
    {
        var oldJson = oldValues != null ? JsonSerializer.Serialize(oldValues) : null;
        var newJson = newValues != null ? JsonSerializer.Serialize(newValues) : null;

        await auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated claim {claimNumber}: {changedFields}",
            entityType: "Claim",
            entityId: claimId,
            entityIdentifier: claimNumber,
            oldValues: oldJson,
            newValues: newJson,
            changedFields: changedFields,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Log claim submission
    /// </summary>
    public static async Task LogClaimSubmittedAsync(
        this IAuditService auditService,
        Guid claimId,
        string claimNumber,
        CancellationToken cancellationToken = default)
    {
        await auditService.LogActionAsync(
            actionType: AuditActionType.Submit,
            actionDescription: $"Submitted claim {claimNumber} for review",
            entityType: "Claim",
            entityId: claimId,
            entityIdentifier: claimNumber,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new { SubmittedDate = DateTime.UtcNow }),
            changedFields: "SubmittedDate,LifecycleStage",
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Log claim assignment
    /// </summary>
    public static async Task LogClaimAssignedAsync(
        this IAuditService auditService,
        Guid claimId,
        string claimNumber,
        Guid assignedToUserId,
        string assignedToUserName,
        CancellationToken cancellationToken = default)
    {
        await auditService.LogActionAsync(
            actionType: AuditActionType.Assign,
            actionDescription: $"Assigned claim {claimNumber} to {assignedToUserName}",
            entityType: "Claim",
            entityId: claimId,
            entityIdentifier: claimNumber,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new { AssignedToUserId = assignedToUserId, AssignedDate = DateTime.UtcNow }),
            changedFields: "AssignedToUserId,AssignedDate",
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Log claim verification
    /// </summary>
    public static async Task LogClaimVerifiedAsync(
        this IAuditService auditService,
        Guid claimId,
        string claimNumber,
        string verificationNotes,
        CancellationToken cancellationToken = default)
    {
        await auditService.LogActionAsync(
            actionType: AuditActionType.Verify,
            actionDescription: $"Verified claim {claimNumber}",
            entityType: "Claim",
            entityId: claimId,
            entityIdentifier: claimNumber,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new { VerificationDate = DateTime.UtcNow, VerificationNotes = verificationNotes }),
            changedFields: "VerificationStatus,VerificationDate,VerifiedByUserId,VerificationNotes",
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Log claim approval (Administrator only)
    /// </summary>
    public static async Task LogClaimApprovedAsync(
        this IAuditService auditService,
        Guid claimId,
        string claimNumber,
        string? approvalNotes = null,
        CancellationToken cancellationToken = default)
    {
        await auditService.LogActionAsync(
            actionType: AuditActionType.Approve,
            actionDescription: $"Approved claim {claimNumber}" + (string.IsNullOrWhiteSpace(approvalNotes) ? "" : $": {approvalNotes}"),
            entityType: "Claim",
            entityId: claimId,
            entityIdentifier: claimNumber,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new { DecisionDate = DateTime.UtcNow, FinalDecision = "Approved", DecisionNotes = approvalNotes }),
            changedFields: "FinalDecision,DecisionDate,DecisionByUserId,DecisionNotes",
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Log claim rejection (Administrator only)
    /// </summary>
    public static async Task LogClaimRejectedAsync(
        this IAuditService auditService,
        Guid claimId,
        string claimNumber,
        string rejectionReason,
        CancellationToken cancellationToken = default)
    {
        await auditService.LogActionAsync(
            actionType: AuditActionType.Reject,
            actionDescription: $"Rejected claim {claimNumber}: {rejectionReason}",
            entityType: "Claim",
            entityId: claimId,
            entityIdentifier: claimNumber,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new { DecisionDate = DateTime.UtcNow, FinalDecision = "Rejected", DecisionReason = rejectionReason }),
            changedFields: "FinalDecision,DecisionDate,DecisionByUserId,DecisionReason",
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Log claim deletion (soft delete)
    /// </summary>
    public static async Task LogClaimDeletedAsync(
        this IAuditService auditService,
        Guid claimId,
        string claimNumber,
        string? deletionReason = null,
        CancellationToken cancellationToken = default)
    {
        await auditService.LogActionAsync(
            actionType: AuditActionType.Delete,
            actionDescription: $"Deleted claim {claimNumber}" + (string.IsNullOrWhiteSpace(deletionReason) ? "" : $": {deletionReason}"),
            entityType: "Claim",
            entityId: claimId,
            entityIdentifier: claimNumber,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new { DeletedAt = DateTime.UtcNow, IsDeleted = true }),
            changedFields: "IsDeleted,DeletedAtUtc,DeletedBy",
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Log evidence upload
    /// </summary>
    public static async Task LogEvidenceUploadedAsync(
        this IAuditService auditService,
        Guid evidenceId,
        string fileName,
        Guid claimId,
        string claimNumber,
        CancellationToken cancellationToken = default)
    {
        await auditService.LogActionAsync(
            actionType: AuditActionType.Upload,
            actionDescription: $"Uploaded evidence '{fileName}' for claim {claimNumber}",
            entityType: "Evidence",
            entityId: evidenceId,
            entityIdentifier: fileName,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new { FileName = fileName, ClaimId = claimId }),
            changedFields: null,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Log user login
    /// </summary>
    public static async Task LogUserLoginAsync(
        this IAuditService auditService,
        string username,
        bool isSuccessful,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        if (isSuccessful)
        {
            await auditService.LogSecurityActionAsync(
                actionType: AuditActionType.Login,
                actionDescription: $"User {username} logged in successfully",
                isSecuritySensitive: true,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            await auditService.LogFailedActionAsync(
                actionType: AuditActionType.LoginFailed,
                actionDescription: $"Failed login attempt for user {username}",
                errorMessage: failureReason ?? "Invalid credentials",
                stackTrace: null,
                entityType: "User",
                entityId: null,
                cancellationToken: cancellationToken
            );
        }
    }

    /// <summary>
    /// Log permission granted
    /// </summary>
    public static async Task LogPermissionGrantedAsync(
        this IAuditService auditService,
        Guid userId,
        string username,
        Permission permission,
        string reason,
        CancellationToken cancellationToken = default)
    {
        await auditService.LogSecurityActionAsync(
            actionType: AuditActionType.PermissionGranted,
            actionDescription: $"Granted {permission} permission to {username}: {reason}",
            isSecuritySensitive: true,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Log permission revoked
    /// </summary>
    public static async Task LogPermissionRevokedAsync(
        this IAuditService auditService,
        Guid userId,
        string username,
        Permission permission,
        string reason,
        CancellationToken cancellationToken = default)
    {
        await auditService.LogSecurityActionAsync(
            actionType: AuditActionType.PermissionRevoked,
            actionDescription: $"Revoked {permission} permission from {username}: {reason}",
            isSecuritySensitive: true,
            cancellationToken: cancellationToken
        );
    }
}
