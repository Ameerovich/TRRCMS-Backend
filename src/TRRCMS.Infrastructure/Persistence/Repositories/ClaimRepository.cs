using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;
using TRRCMS.Infrastructure.Persistence;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Claim entity
/// Provides comprehensive data access operations with optimized queries
/// </summary>
public class ClaimRepository : IClaimRepository
{
    private readonly ApplicationDbContext _context;
    
    public ClaimRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    // ==================== BASIC CRUD OPERATIONS ====================
    
    public async Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Include(c => c.Evidences)
            .Include(c => c.Documents)
            .Include(c => c.Referrals)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        await _context.Claims.AddAsync(claim, cancellationToken);
    }
    
    public Task UpdateAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        _context.Claims.Update(claim);
        return Task.CompletedTask;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    
    // ==================== QUERY BY UNIQUE IDENTIFIERS ====================
    
    public async Task<Claim?> GetByClaimNumberAsync(string claimNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Include(c => c.Evidences)
            .Include(c => c.Documents)
            .Include(c => c.Referrals)
            .FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber, cancellationToken);
    }
    
    // ==================== QUERY BY RELATIONSHIPS ====================
    
    public async Task<Claim?> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Include(c => c.Evidences)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.PropertyUnitId == propertyUnitId, cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetByPrimaryClaimantIdAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.PrimaryClaimantId == personId)
            .OrderByDescending(c => c.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetByAssignedUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.AssignedToUserId == userId)
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.TargetCompletionDate)
            .ToListAsync(cancellationToken);
    }
    
    // ==================== QUERY BY WORKFLOW STATES ====================
    
    public async Task<IEnumerable<Claim>> GetByLifecycleStageAsync(LifecycleStage stage, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.LifecycleStage == stage)
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetByStatusAsync(ClaimStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetByPriorityAsync(CasePriority priority, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.Priority == priority)
            .OrderBy(c => c.TargetCompletionDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetByVerificationStatusAsync(VerificationStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.VerificationStatus == status)
            .OrderBy(c => c.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetByCertificateStatusAsync(CertificateStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.CertificateStatus == status)
            .OrderBy(c => c.DecisionDate)
            .ToListAsync(cancellationToken);
    }
    
    // ==================== SPECIALIZED QUERIES ====================
    
    public async Task<IEnumerable<Claim>> GetConflictingClaimsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.HasConflicts)
            .OrderByDescending(c => c.ConflictCount)
            .ThenBy(c => c.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetOverdueClaimsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.TargetCompletionDate.HasValue 
                && c.TargetCompletionDate.Value < now
                && !c.DecisionDate.HasValue)
            .OrderBy(c => c.TargetCompletionDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetClaimsAwaitingDocumentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.LifecycleStage == LifecycleStage.AwaitingDocuments
                || !c.AllRequiredDocumentsSubmitted)
            .OrderBy(c => c.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetClaimsPendingVerificationAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.VerificationStatus == VerificationStatus.Pending)
            .OrderBy(c => c.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetClaimsForAdjudicationAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.LifecycleStage == LifecycleStage.ConflictDetected 
                || c.LifecycleStage == LifecycleStage.InAdjudication
                || c.HasConflicts)
            .OrderByDescending(c => c.ConflictCount)
            .ThenBy(c => c.Priority)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetClaimsBySubmittedDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.SubmittedDate.HasValue
                && c.SubmittedDate.Value >= startDate
                && c.SubmittedDate.Value <= endDate)
            .OrderBy(c => c.SubmittedDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Claim>> GetClaimsByDecisionDateRangeAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Where(c => c.DecisionDate.HasValue
                && c.DecisionDate.Value >= startDate
                && c.DecisionDate.Value <= endDate)
            .OrderBy(c => c.DecisionDate)
            .ToListAsync(cancellationToken);
    }
    
    // ==================== FILTERED QUERY ====================

    public async Task<List<Claim>> GetFilteredAsync(
        ClaimStatus? status,
        ClaimSource? source,
        Guid? createdByUserId,
        Guid? claimId,
        string? buildingCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Claims
            .Include(c => c.PropertyUnit)
                .ThenInclude(pu => pu.Building)
            .Include(c => c.PrimaryClaimant)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);
        if (source.HasValue)
            query = query.Where(c => c.ClaimSource == source.Value);
        if (createdByUserId.HasValue)
            query = query.Where(c => c.CreatedBy == createdByUserId.Value);
        if (claimId.HasValue)
            query = query.Where(c => c.Id == claimId.Value);
        if (!string.IsNullOrWhiteSpace(buildingCode))
            query = query.Where(c => c.PropertyUnit.Building.BuildingId == buildingCode);

        return await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    // ==================== EXISTENCE CHECKS ====================

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Claims.AnyAsync(c => c.Id == id, cancellationToken);
    }
    
    public async Task<bool> ExistsByClaimNumberAsync(string claimNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Claims.AnyAsync(c => c.ClaimNumber == claimNumber, cancellationToken);
    }
    
    public async Task<bool> HasClaimsAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims.AnyAsync(c => c.PropertyUnitId == propertyUnitId, cancellationToken);
    }
    
    public async Task<bool> HasConflictingClaimsAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        var claimCount = await _context.Claims
            .Where(c => c.PropertyUnitId == propertyUnitId)
            .CountAsync(cancellationToken);
        
        return claimCount > 1;
    }
    
    // ==================== AGGREGATE QUERIES ====================
    
    public async Task<int> GetConflictCountAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Where(c => c.PropertyUnitId == propertyUnitId)
            .CountAsync(cancellationToken);
    }
    
    public async Task<int> GetCountByLifecycleStageAsync(LifecycleStage stage, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Where(c => c.LifecycleStage == stage)
            .CountAsync(cancellationToken);
    }
    
    public async Task<int> GetCountByStatusAsync(ClaimStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Where(c => c.Status == status)
            .CountAsync(cancellationToken);
    }
    
    public async Task<int> GetCountByAssignedUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Where(c => c.AssignedToUserId == userId)
            .CountAsync(cancellationToken);
    }
    
    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Claims.CountAsync(cancellationToken);
    }
}
