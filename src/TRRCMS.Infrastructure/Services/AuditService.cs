using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;
using TRRCMS.Infrastructure.Persistence;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Implementation of audit logging service
/// Provides comprehensive tracking of all system actions
/// </summary>
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private Guid? _correlationId;

    public AuditService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <inheritdoc/>
    public async Task LogActionAsync(
        AuditActionType actionType,
        string actionDescription,
        string? entityType = null,
        Guid? entityId = null,
        string? entityIdentifier = null,
        string? oldValues = null,
        string? newValues = null,
        string? changedFields = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current user information
            var user = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            if (user == null)
            {
                // If no authenticated user, log as system action
                await LogSystemActionAsync(
                    actionType,
                    actionDescription,
                    entityType,
                    entityId,
                    entityIdentifier,
                    oldValues,
                    newValues,
                    changedFields,
                    cancellationToken);
                return;
            }

            // Generate audit log number (sequential)
            var auditLogNumber = await GetNextAuditLogNumberAsync(cancellationToken);

            // Create audit log entry
            var auditLog = AuditLog.Create(
                auditLogNumber: auditLogNumber,
                actionType: actionType,
                actionDescription: actionDescription,
                userId: user.Id,
                username: user.Username,
                userRole: user.Role.ToString(),
                userFullName: user.FullNameArabic ?? user.Username,
                entityType: entityType,
                entityId: entityId,
                entityIdentifier: entityIdentifier,
                oldValues: oldValues,
                newValues: newValues,
                changedFields: changedFields,
                actionResult: "Success"
            );

            // Set request context
            auditLog.SetRequestContext(
                ipAddress: _currentUserService.IpAddress,
                userAgent: null, // Could be extracted from HttpContext if needed
                sourceApplication: _currentUserService.SourceApplication,
                deviceId: null, // Could be extracted from JWT if needed
                sessionId: null // Could be tracked if session management is added
            );

            // Set correlation ID if active
            if (_correlationId.HasValue)
            {
                auditLog.SetCorrelation(_correlationId.Value);
            }

            // Save to database
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the error but don't throw - audit logging should never break the main operation
            Console.WriteLine($"[AUDIT ERROR] Failed to log action: {ex.Message}");
            // In production, consider using ILogger instead of Console.WriteLine
        }
    }

    /// <inheritdoc/>
    public async Task LogFailedActionAsync(
        AuditActionType actionType,
        string actionDescription,
        string errorMessage,
        string? stackTrace = null,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            var auditLogNumber = await GetNextAuditLogNumberAsync(cancellationToken);

            var auditLog = AuditLog.Create(
                auditLogNumber: auditLogNumber,
                actionType: actionType,
                actionDescription: actionDescription,
                userId: user?.Id ?? Guid.Empty,
                username: user?.Username ?? "System",
                userRole: user?.Role.ToString() ?? "System",
                userFullName: user?.FullNameArabic ?? "System",
                entityType: entityType,
                entityId: entityId,
                entityIdentifier: null,
                oldValues: null,
                newValues: null,
                changedFields: null,
                actionResult: "Failed"
            );

            auditLog.RecordError(errorMessage, stackTrace);

            auditLog.SetRequestContext(
                ipAddress: _currentUserService.IpAddress,
                userAgent: null,
                sourceApplication: _currentUserService.SourceApplication,
                deviceId: null,
                sessionId: null
            );

            if (_correlationId.HasValue)
            {
                auditLog.SetCorrelation(_correlationId.Value);
            }

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AUDIT ERROR] Failed to log failed action: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task LogClaimTransitionAsync(
        Guid claimId,
        string claimNumber,
        LifecycleStage fromStage,
        LifecycleStage toStage,
        string? transitionReason = null,
        CancellationToken cancellationToken = default)
    {
        var actionDescription = $"Claim {claimNumber} transitioned from {fromStage} to {toStage}";
        if (!string.IsNullOrWhiteSpace(transitionReason))
        {
            actionDescription += $". Reason: {transitionReason}";
        }

        var oldValues = JsonSerializer.Serialize(new { LifecycleStage = fromStage.ToString() });
        var newValues = JsonSerializer.Serialize(new { LifecycleStage = toStage.ToString() });

        await LogActionAsync(
            actionType: AuditActionType.StateTransition,
            actionDescription: actionDescription,
            entityType: "Claim",
            entityId: claimId,
            entityIdentifier: claimNumber,
            oldValues: oldValues,
            newValues: newValues,
            changedFields: "LifecycleStage",
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc/>
    public async Task LogSecurityActionAsync(
        AuditActionType actionType,
        string actionDescription,
        bool isSecuritySensitive = true,
        CancellationToken cancellationToken = default)
    {
        // Security actions use the same logging but are flagged as sensitive
        await LogActionAsync(
            actionType: actionType,
            actionDescription: actionDescription,
            entityType: null,
            entityId: null,
            entityIdentifier: null,
            oldValues: null,
            newValues: null,
            changedFields: null,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc/>
    public async Task<List<AuditLog>> GetEntityHistoryAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<AuditLog>> GetUserRecentActivityAsync(
        Guid userId,
        int count = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<AuditLog>> GetAuditLogsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? userId = null,
        string? entityType = null,
        AuditActionType? actionType = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (actionType.HasValue)
            query = query.Where(a => a.ActionType == actionType.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public void SetCorrelationId(Guid correlationId)
    {
        _correlationId = correlationId;
    }

    /// <inheritdoc/>
    public void ClearCorrelationId()
    {
        _correlationId = null;
    }

    // ==================== PRIVATE HELPERS ====================

    /// <summary>
    /// Generate next sequential audit log number
    /// </summary>
    private async Task<long> GetNextAuditLogNumberAsync(CancellationToken cancellationToken)
    {
        var maxNumber = await _context.AuditLogs
            .MaxAsync(a => (long?)a.AuditLogNumber, cancellationToken) ?? 0;

        return maxNumber + 1;
    }

    /// <summary>
    /// Log system action (when no user is authenticated)
    /// </summary>
    private async Task LogSystemActionAsync(
        AuditActionType actionType,
        string actionDescription,
        string? entityType,
        Guid? entityId,
        string? entityIdentifier,
        string? oldValues,
        string? newValues,
        string? changedFields,
        CancellationToken cancellationToken)
    {
        var auditLogNumber = await GetNextAuditLogNumberAsync(cancellationToken);

        var auditLog = AuditLog.Create(
            auditLogNumber: auditLogNumber,
            actionType: actionType,
            actionDescription: actionDescription,
            userId: Guid.Empty,
            username: "System",
            userRole: "System",
            userFullName: "System",
            entityType: entityType,
            entityId: entityId,
            entityIdentifier: entityIdentifier,
            oldValues: oldValues,
            newValues: newValues,
            changedFields: changedFields,
            actionResult: "Success"
        );

        auditLog.SetRequestContext(
            ipAddress: "System",
            userAgent: null,
            sourceApplication: "System",
            deviceId: null,
            sessionId: null
        );

        if (_correlationId.HasValue)
        {
            auditLog.SetCorrelation(_correlationId.Value);
        }

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
