using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for BuildingAssignment operations
/// UC-012: Assign Buildings to Field Collectors
/// </summary>
public class BuildingAssignmentRepository : IBuildingAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public BuildingAssignmentRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // ==================== BASIC CRUD ====================

    public async Task<BuildingAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BuildingAssignments
            .FirstOrDefaultAsync(ba => ba.Id == id, cancellationToken);
    }

    public async Task<BuildingAssignment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BuildingAssignments
            .Include(ba => ba.Building)
            .Include(ba => ba.OriginalAssignment)
            .FirstOrDefaultAsync(ba => ba.Id == id, cancellationToken);
    }

    public async Task<BuildingAssignment> AddAsync(BuildingAssignment assignment, CancellationToken cancellationToken = default)
    {
        await _context.BuildingAssignments.AddAsync(assignment, cancellationToken);
        return assignment;
    }

    public async Task AddRangeAsync(IEnumerable<BuildingAssignment> assignments, CancellationToken cancellationToken = default)
    {
        await _context.BuildingAssignments.AddRangeAsync(assignments, cancellationToken);
    }

    public Task UpdateAsync(BuildingAssignment assignment, CancellationToken cancellationToken = default)
    {
        _context.BuildingAssignments.Update(assignment);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    // ==================== FIELD COLLECTOR QUERIES ====================

    public async Task<List<BuildingAssignment>> GetByFieldCollectorAsync(
        Guid fieldCollectorId, 
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.BuildingAssignments
            .Where(ba => ba.FieldCollectorId == fieldCollectorId);

        if (activeOnly)
        {
            query = query.Where(ba => ba.IsActive);
        }

        return await query
            .OrderByDescending(ba => ba.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BuildingAssignment>> GetByFieldCollectorWithBuildingsAsync(
        Guid fieldCollectorId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.BuildingAssignments
            .Include(ba => ba.Building)
            .Where(ba => ba.FieldCollectorId == fieldCollectorId);

        if (activeOnly)
        {
            query = query.Where(ba => ba.IsActive);
        }

        return await query
            .OrderByDescending(ba => ba.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetAssignmentCountAsync(
        Guid fieldCollectorId, 
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.BuildingAssignments
            .Where(ba => ba.FieldCollectorId == fieldCollectorId);

        if (activeOnly)
        {
            query = query.Where(ba => ba.IsActive);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<List<BuildingAssignment>> GetByFieldCollectorAndStatusAsync(
        Guid fieldCollectorId,
        TransferStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.BuildingAssignments
            .Where(ba => ba.FieldCollectorId == fieldCollectorId && ba.TransferStatus == status)
            .OrderByDescending(ba => ba.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    // ==================== BUILDING QUERIES ====================

    public async Task<List<BuildingAssignment>> GetByBuildingIdAsync(
        Guid buildingId,
        CancellationToken cancellationToken = default)
    {
        return await _context.BuildingAssignments
            .Where(ba => ba.BuildingId == buildingId)
            .OrderByDescending(ba => ba.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<BuildingAssignment?> GetActiveAssignmentForBuildingAsync(
        Guid buildingId,
        CancellationToken cancellationToken = default)
    {
        return await _context.BuildingAssignments
            .Where(ba => ba.BuildingId == buildingId && ba.IsActive)
            .OrderByDescending(ba => ba.AssignedDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasActiveAssignmentAsync(
        Guid buildingId,
        CancellationToken cancellationToken = default)
    {
        return await _context.BuildingAssignments
            .AnyAsync(ba => ba.BuildingId == buildingId && ba.IsActive, cancellationToken);
    }

    // ==================== STATUS QUERIES ====================

    public async Task<List<BuildingAssignment>> GetByTransferStatusAsync(
        TransferStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.BuildingAssignments
            .Where(ba => ba.TransferStatus == status && ba.IsActive)
            .OrderByDescending(ba => ba.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BuildingAssignment>> GetPendingAssignmentsAsync(
        Guid? fieldCollectorId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.BuildingAssignments
            .Where(ba => ba.TransferStatus == TransferStatus.Pending && ba.IsActive);

        if (fieldCollectorId.HasValue)
        {
            query = query.Where(ba => ba.FieldCollectorId == fieldCollectorId.Value);
        }

        return await query
            .OrderBy(ba => ba.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BuildingAssignment>> GetOverdueAssignmentsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.BuildingAssignments
            .Where(ba => ba.IsActive &&
                        !ba.ActualCompletionDate.HasValue &&
                        ba.TargetCompletionDate.HasValue &&
                        ba.TargetCompletionDate.Value < now)
            .OrderBy(ba => ba.TargetCompletionDate)
            .ToListAsync(cancellationToken);
    }

    // ==================== SEARCH WITH FILTERS ====================

    public async Task<(List<BuildingAssignment> Assignments, int TotalCount)> SearchAssignmentsAsync(
        Guid? fieldCollectorId = null,
        Guid? buildingId = null,
        TransferStatus? transferStatus = null,
        bool? isActive = null,
        bool? isRevisit = null,
        DateTime? assignedFromDate = null,
        DateTime? assignedToDate = null,
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.BuildingAssignments.AsQueryable();

        // Apply filters
        if (fieldCollectorId.HasValue)
        {
            query = query.Where(ba => ba.FieldCollectorId == fieldCollectorId.Value);
        }

        if (buildingId.HasValue)
        {
            query = query.Where(ba => ba.BuildingId == buildingId.Value);
        }

        if (transferStatus.HasValue)
        {
            query = query.Where(ba => ba.TransferStatus == transferStatus.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(ba => ba.IsActive == isActive.Value);
        }

        if (isRevisit.HasValue)
        {
            query = query.Where(ba => ba.IsRevisit == isRevisit.Value);
        }

        if (assignedFromDate.HasValue)
        {
            query = query.Where(ba => ba.AssignedDate >= assignedFromDate.Value);
        }

        if (assignedToDate.HasValue)
        {
            query = query.Where(ba => ba.AssignedDate <= assignedToDate.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = sortBy?.ToLowerInvariant() switch
        {
            "assigneddate" => sortDescending 
                ? query.OrderByDescending(ba => ba.AssignedDate) 
                : query.OrderBy(ba => ba.AssignedDate),
            "targetcompletiondate" => sortDescending 
                ? query.OrderByDescending(ba => ba.TargetCompletionDate) 
                : query.OrderBy(ba => ba.TargetCompletionDate),
            "transferstatus" => sortDescending 
                ? query.OrderByDescending(ba => ba.TransferStatus) 
                : query.OrderBy(ba => ba.TransferStatus),
            "priority" => sortDescending 
                ? query.OrderByDescending(ba => ba.Priority) 
                : query.OrderBy(ba => ba.Priority),
            _ => query.OrderByDescending(ba => ba.AssignedDate)
        };

        // Apply pagination
        var assignments = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (assignments, totalCount);
    }

    // ==================== SYNC QUERIES ====================

    /// <inheritdoc />
    public async Task<List<BuildingAssignment>> GetPendingOrFailedByFieldCollectorAsync(
        Guid fieldCollectorId,
        DateTime? modifiedSinceUtc = null,
        CancellationToken cancellationToken = default)
    {
        // Base filter: active assignments whose transfer is not yet acknowledged.
        var query = _context.BuildingAssignments
            .Include(ba => ba.Building)
                .ThenInclude(b => b.PropertyUnits)
            .Where(ba =>
                ba.FieldCollectorId == fieldCollectorId &&
                ba.IsActive &&
                (ba.TransferStatus == TransferStatus.Pending ||
                 ba.TransferStatus == TransferStatus.Failed));

        // Optional incremental sync: only return assignments touched since
        // the tablet's last sync timestamp (tablet sends its last-synced-at value).
        if (modifiedSinceUtc.HasValue)
        {
            query = query.Where(ba =>
                ba.CreatedAtUtc >= modifiedSinceUtc.Value ||
                ba.LastModifiedAtUtc >= modifiedSinceUtc.Value);
        }

        return await query
            .OrderBy(ba => ba.Priority)       // Urgent/High before Normal
            .ThenBy(ba => ba.AssignedDate)    // Oldest first within the same priority level
            .ToListAsync(cancellationToken);
    }

    // ==================== REVISIT QUERIES ====================

    public async Task<List<BuildingAssignment>> GetRevisitAssignmentsAsync(
        Guid buildingId,
        CancellationToken cancellationToken = default)
    {
        return await _context.BuildingAssignments
            .Where(ba => ba.BuildingId == buildingId && ba.IsRevisit)
            .OrderByDescending(ba => ba.AssignedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<BuildingAssignment?> GetOriginalAssignmentAsync(
        Guid revisitAssignmentId,
        CancellationToken cancellationToken = default)
    {
        var revisit = await _context.BuildingAssignments
            .FirstOrDefaultAsync(ba => ba.Id == revisitAssignmentId, cancellationToken);

        if (revisit?.OriginalAssignmentId == null)
            return null;

        return await _context.BuildingAssignments
            .FirstOrDefaultAsync(ba => ba.Id == revisit.OriginalAssignmentId.Value, cancellationToken);
    }

    // ==================== STATISTICS ====================

    public async Task<FieldCollectorAssignmentStats> GetFieldCollectorStatsAsync(
        Guid fieldCollectorId,
        CancellationToken cancellationToken = default)
    {
        var assignments = await _context.BuildingAssignments
            .Where(ba => ba.FieldCollectorId == fieldCollectorId)
            .ToListAsync(cancellationToken);

        var activeAssignments = assignments.Where(a => a.IsActive).ToList();
        var now = DateTime.UtcNow;

        return new FieldCollectorAssignmentStats
        {
            TotalAssignments = assignments.Count,
            ActiveAssignments = activeAssignments.Count,
            PendingTransfer = activeAssignments.Count(a => a.TransferStatus == TransferStatus.Pending),
            Transferred = activeAssignments.Count(a => a.TransferStatus == TransferStatus.Transferred),
            Completed = assignments.Count(a => a.ActualCompletionDate.HasValue),
            OverdueAssignments = activeAssignments.Count(a => 
                a.TargetCompletionDate.HasValue && 
                !a.ActualCompletionDate.HasValue && 
                a.TargetCompletionDate.Value < now),
            TotalPropertyUnits = activeAssignments.Sum(a => a.TotalPropertyUnits),
            CompletedPropertyUnits = activeAssignments.Sum(a => a.CompletedPropertyUnits)
        };
    }

    public async Task<AssignmentOverviewStats> GetOverviewStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var assignments = await _context.BuildingAssignments
            .ToListAsync(cancellationToken);

        var activeAssignments = assignments.Where(a => a.IsActive).ToList();
        var now = DateTime.UtcNow;

        return new AssignmentOverviewStats
        {
            TotalAssignments = assignments.Count,
            TotalActiveAssignments = activeAssignments.Count,
            TotalPendingTransfer = activeAssignments.Count(a => a.TransferStatus == TransferStatus.Pending),
            TotalTransferred = activeAssignments.Count(a => a.TransferStatus == TransferStatus.Transferred),
            TotalCompleted = assignments.Count(a => a.ActualCompletionDate.HasValue),
            TotalOverdue = activeAssignments.Count(a => 
                a.TargetCompletionDate.HasValue && 
                !a.ActualCompletionDate.HasValue && 
                a.TargetCompletionDate.Value < now),
            TotalFieldCollectorsWithAssignments = activeAssignments
                .Select(a => a.FieldCollectorId)
                .Distinct()
                .Count()
        };
    }
}
