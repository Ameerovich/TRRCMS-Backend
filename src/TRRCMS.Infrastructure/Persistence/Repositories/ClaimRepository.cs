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

    public async Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Include(c => c.Evidences)

            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Claim>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Include(c => c.Evidences)
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

    public async Task<Claim?> GetByClaimNumberAsync(string claimNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Include(c => c.Evidences)

            .FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber, cancellationToken);
    }

    public async Task<Claim?> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Include(c => c.Evidences)

            .FirstOrDefaultAsync(c => c.PropertyUnitId == propertyUnitId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<Claim>> GetAllByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Where(c => c.PropertyUnitId == propertyUnitId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Claim>> GetByPrimaryClaimantIdAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Include(c => c.Evidences)
            .Where(c => c.PrimaryClaimantId == personId)
            .OrderByDescending(c => c.SubmittedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Claim>> GetByCaseStatusAsync(CaseStatus caseStatus, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Include(c => c.PropertyUnit)
            .Include(c => c.PrimaryClaimant)
            .Include(c => c.Evidences)
            .Where(c => c.CaseStatus == caseStatus)
            .OrderByDescending(c => c.SubmittedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Claim>> GetFilteredAsync(
        CaseStatus? caseStatus,
        ClaimSource? source,
        Guid? createdByUserId,
        Guid? claimId,
        string? buildingCode = null,
        Guid? propertyUnitId = null,
        Guid? originatingSurveyId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Claims
            .Include(c => c.PropertyUnit)
                .ThenInclude(pu => pu.Building)
            .Include(c => c.PrimaryClaimant)
            .Include(c => c.Evidences)
            .AsQueryable();

        if (caseStatus.HasValue)
            query = query.Where(c => c.CaseStatus == caseStatus.Value);
        if (source.HasValue)
            query = query.Where(c => c.ClaimSource == source.Value);
        if (createdByUserId.HasValue)
            query = query.Where(c => c.CreatedBy == createdByUserId.Value);
        if (claimId.HasValue)
            query = query.Where(c => c.Id == claimId.Value);
        if (!string.IsNullOrWhiteSpace(buildingCode))
            query = query.Where(c => c.PropertyUnit.Building.BuildingId == buildingCode);
        if (propertyUnitId.HasValue)
            query = query.Where(c => c.PropertyUnitId == propertyUnitId.Value);
        if (originatingSurveyId.HasValue)
            query = query.Where(c => c.OriginatingSurveyId == originatingSurveyId.Value);

        return await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

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

    public async Task<int> GetCountByCaseStatusAsync(CaseStatus caseStatus, CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .Where(c => c.CaseStatus == caseStatus)
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Claims.CountAsync(cancellationToken);
    }

    public async Task<Dictionary<CaseStatus, int>> GetCaseStatusCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .GroupBy(c => c.CaseStatus)
            .Select(g => new { CaseStatus = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CaseStatus, x => x.Count, cancellationToken);
    }

    public async Task<Dictionary<ClaimType, int>> GetClaimTypeCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Claims
            .GroupBy(c => c.ClaimType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);
    }

    public async Task<List<(int Year, int Month, int Count)>> GetMonthlyCreationCountsAsync(
        DateTime? from = null, DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Claims.AsQueryable();
        if (from.HasValue) query = query.Where(c => c.CreatedAtUtc >= from.Value);
        if (to.HasValue) query = query.Where(c => c.CreatedAtUtc <= to.Value);

        var results = await query
            .GroupBy(c => new { c.CreatedAtUtc.Year, c.CreatedAtUtc.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.Year, r.Month, r.Count)).ToList();
    }
}
