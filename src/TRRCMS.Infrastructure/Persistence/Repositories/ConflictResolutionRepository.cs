using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for ConflictResolution entity.
/// Handles duplicate detection results and conflict resolution workflow.
/// </summary>
public class ConflictResolutionRepository : IConflictResolutionRepository
{
    private readonly ApplicationDbContext _context;

    public ConflictResolutionRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // ==================== BASIC CRUD ====================

    public async Task<ConflictResolution?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<List<ConflictResolution>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.DetectedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ConflictResolution conflict, CancellationToken cancellationToken = default)
    {
        await _context.ConflictResolutions.AddAsync(conflict, cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<ConflictResolution> conflicts,
        CancellationToken cancellationToken = default)
    {
        await _context.ConflictResolutions.AddRangeAsync(conflicts, cancellationToken);
    }

    public Task UpdateAsync(ConflictResolution conflict, CancellationToken cancellationToken = default)
    {
        _context.ConflictResolutions.Update(conflict);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    // ==================== QUERY BY PACKAGE ====================

    public async Task<List<ConflictResolution>> GetByPackageIdAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => c.ImportPackageId == importPackageId && !c.IsDeleted)
            .OrderByDescending(c => c.SimilarityScore)
            .ThenBy(c => c.DetectedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByPackageIdAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => c.ImportPackageId == importPackageId && !c.IsDeleted)
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetUnresolvedCountByPackageIdAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => c.ImportPackageId == importPackageId
                && c.Status == "PendingReview"
                && !c.IsDeleted)
            .CountAsync(cancellationToken);
    }

    public async Task<bool> AreAllResolvedForPackageAsync(
        Guid importPackageId,
        CancellationToken cancellationToken = default)
    {
        // True if no conflicts exist OR all conflicts are resolved/ignored
        return !await _context.ConflictResolutions
            .AnyAsync(c => c.ImportPackageId == importPackageId
                && c.Status == "PendingReview"
                && !c.IsDeleted,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<ConflictResolution>> GetResolvedMergesForPackageAsync(
        Guid importPackageId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => c.ImportPackageId == importPackageId
                && c.EntityType == entityType
                && c.ResolutionAction == ConflictResolutionAction.Merge
                && c.MergedEntityId.HasValue
                && c.DiscardedEntityId.HasValue
                && !c.IsDeleted)
            .ToListAsync(cancellationToken);
    }


    // ==================== QUERY BY STATUS ====================

    public async Task<List<ConflictResolution>> GetByStatusAsync(
        string status,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => c.Status == status && !c.IsDeleted)
            .OrderByDescending(c => c.DetectedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ConflictResolution>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => c.Status == "PendingReview" && !c.IsDeleted)
            .OrderByDescending(c => c.SimilarityScore)
            .ThenBy(c => c.Priority)
            .ThenBy(c => c.DetectedDate)
            .ToListAsync(cancellationToken);
    }

    // ==================== QUERY BY ENTITY PAIR ====================

    public async Task<ConflictResolution?> GetByEntityPairAsync(
        Guid firstEntityId,
        Guid secondEntityId,
        CancellationToken cancellationToken = default)
    {
        // Order-independent: check both (A,B) and (B,A)
        return await _context.ConflictResolutions
            .FirstOrDefaultAsync(c =>
                ((c.FirstEntityId == firstEntityId && c.SecondEntityId == secondEntityId) ||
                 (c.FirstEntityId == secondEntityId && c.SecondEntityId == firstEntityId))
                && !c.IsDeleted,
                cancellationToken);
    }

    public async Task<List<ConflictResolution>> GetByEntityIdAsync(
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => (c.FirstEntityId == entityId || c.SecondEntityId == entityId)
                && !c.IsDeleted)
            .OrderByDescending(c => c.DetectedDate)
            .ToListAsync(cancellationToken);
    }

    // ==================== QUERY BY ASSIGNMENT ====================

    public async Task<List<ConflictResolution>> GetByAssignedUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => c.AssignedToUserId == userId && !c.IsDeleted)
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.DetectedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ConflictResolution>> GetEscalatedAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => c.IsEscalated && !c.IsDeleted)
            .OrderBy(c => c.EscalatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ConflictResolution>> GetOverdueAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => c.IsOverdue && c.Status == "PendingReview" && !c.IsDeleted)
            .OrderBy(c => c.DetectedDate)
            .ToListAsync(cancellationToken);
    }

    // ==================== QUERY BY TYPE ====================

    public async Task<List<ConflictResolution>> GetByConflictTypeAsync(
        string conflictType,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => c.ConflictType == conflictType && !c.IsDeleted)
            .OrderByDescending(c => c.SimilarityScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ConflictResolution>> GetByConflictTypeAndStatusAsync(
        string conflictType,
        string status,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => c.ConflictType == conflictType
                && c.Status == status
                && !c.IsDeleted)
            .OrderByDescending(c => c.SimilarityScore)
            .ToListAsync(cancellationToken);
    }

    // ==================== SEARCH WITH PAGINATION ====================

    public async Task<(List<ConflictResolution> Conflicts, int TotalCount)> SearchAsync(
        Guid? importPackageId = null,
        string? conflictType = null,
        string? entityType = null,
        string? status = null,
        string? priority = null,
        Guid? assignedToUserId = null,
        bool? isEscalated = null,
        bool? isOverdue = null,
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ConflictResolutions
            .Where(c => !c.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (importPackageId.HasValue)
            query = query.Where(c => c.ImportPackageId == importPackageId.Value);

        if (!string.IsNullOrWhiteSpace(conflictType))
            query = query.Where(c => c.ConflictType == conflictType);

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(c => c.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);

        if (!string.IsNullOrWhiteSpace(priority))
            query = query.Where(c => c.Priority == priority);

        if (assignedToUserId.HasValue)
            query = query.Where(c => c.AssignedToUserId == assignedToUserId.Value);

        if (isEscalated.HasValue)
            query = query.Where(c => c.IsEscalated == isEscalated.Value);

        if (isOverdue.HasValue)
            query = query.Where(c => c.IsOverdue == isOverdue.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "conflictnumber" => sortDescending
                ? query.OrderByDescending(c => c.ConflictNumber)
                : query.OrderBy(c => c.ConflictNumber),
            "conflicttype" => sortDescending
                ? query.OrderByDescending(c => c.ConflictType)
                : query.OrderBy(c => c.ConflictType),
            "similarityscore" => sortDescending
                ? query.OrderByDescending(c => c.SimilarityScore)
                : query.OrderBy(c => c.SimilarityScore),
            "priority" => sortDescending
                ? query.OrderByDescending(c => c.Priority)
                : query.OrderBy(c => c.Priority),
            "status" => sortDescending
                ? query.OrderByDescending(c => c.Status)
                : query.OrderBy(c => c.Status),
            _ => sortDescending
                ? query.OrderByDescending(c => c.DetectedDate)
                : query.OrderBy(c => c.DetectedDate)
        };

        // Apply pagination
        var conflicts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (conflicts, totalCount);
    }

    // ==================== AGGREGATE QUERIES ====================

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => !c.IsDeleted)
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetTypeCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => !c.IsDeleted)
            .GroupBy(c => c.ConflictType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .Where(c => !c.IsDeleted)
            .CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ConflictResolutions
            .AnyAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    // ==================== QUERYABLE ACCESS ====================

    public IQueryable<ConflictResolution> GetQueryable()
    {
        return _context.ConflictResolutions
            .Where(c => !c.IsDeleted)
            .AsQueryable();
    }
}