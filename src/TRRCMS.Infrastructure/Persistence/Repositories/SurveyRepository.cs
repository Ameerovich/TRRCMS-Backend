using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Survey entity
/// Supports both field and office survey operations
/// </summary>
public class SurveyRepository : ISurveyRepository
{
    private readonly ApplicationDbContext _context;

    public SurveyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // ==================== COMMON METHODS ====================

    public async Task<Survey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);
    }

    public async Task<Survey?> GetByReferenceCodeAsync(string referenceCode, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .FirstOrDefaultAsync(s => s.ReferenceCode == referenceCode && !s.IsDeleted, cancellationToken);
    }

    public async Task<List<Survey>> GetByBuildingAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.PropertyUnit)
            .Where(s => s.BuildingId == buildingId && !s.IsDeleted)
            .OrderByDescending(s => s.SurveyDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Survey>> GetByPropertyUnitAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Building)
            .Where(s => s.PropertyUnitId == propertyUnitId && !s.IsDeleted)
            .OrderByDescending(s => s.SurveyDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Survey> AddAsync(Survey survey, CancellationToken cancellationToken = default)
    {
        await _context.Surveys.AddAsync(survey, cancellationToken);
        return survey;
    }

    public async Task UpdateAsync(Survey survey, CancellationToken cancellationToken = default)
    {
        _context.Surveys.Update(survey);
        await Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, Survey>> GetByClaimIdsAsync(
        IEnumerable<Guid> claimIds, CancellationToken cancellationToken = default)
    {
        var ids = claimIds.ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, Survey>();

        return await _context.Surveys
            .Where(s => s.ClaimId.HasValue && ids.Contains(s.ClaimId.Value) && !s.IsDeleted)
            .GroupBy(s => s.ClaimId!.Value)
            .Select(g => g.OrderByDescending(s => s.SurveyDate).First())
            .ToDictionaryAsync(s => s.ClaimId!.Value, cancellationToken);
    }

    // ==================== OFFICE SURVEY METHODS ====================

    public async Task<(List<Survey> Surveys, int TotalCount)> GetOfficeSurveysAsync(
        string? status = null,
        Guid? buildingId = null,
        Guid? clerkId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? referenceCode = null,
        string? contactPersonName = null,
        int page = 1,
        int pageSize = 20,
        string sortBy = "SurveyDate",
        string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var query = _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => !s.IsDeleted);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SurveyStatus>(status, true, out var statusEnum))
        {
            query = query.Where(s => s.Status == statusEnum);
        }

        if (buildingId.HasValue)
        {
            query = query.Where(s => s.BuildingId == buildingId.Value);
        }

        if (clerkId.HasValue)
        {
            query = query.Where(s => s.FieldCollectorId == clerkId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.SurveyDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.SurveyDate <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(referenceCode))
        {
            query = query.Where(s => s.ReferenceCode.Contains(referenceCode));
        }

        if (!string.IsNullOrWhiteSpace(contactPersonName))
        {
            query = query.Where(s =>
                s.ContactPersonFullName != null && s.ContactPersonFullName.Contains(contactPersonName));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = sortBy.ToLowerInvariant() switch
        {
            "referencecode" => sortDirection.ToLowerInvariant() == "asc"
                ? query.OrderBy(s => s.ReferenceCode)
                : query.OrderByDescending(s => s.ReferenceCode),
            "status" => sortDirection.ToLowerInvariant() == "asc"
                ? query.OrderBy(s => s.Status)
                : query.OrderByDescending(s => s.Status),
            "createdatutc" => sortDirection.ToLowerInvariant() == "asc"
                ? query.OrderBy(s => s.CreatedAtUtc)
                : query.OrderByDescending(s => s.CreatedAtUtc),
            _ => sortDirection.ToLowerInvariant() == "asc"
                ? query.OrderBy(s => s.SurveyDate)
                : query.OrderByDescending(s => s.SurveyDate)
        };

        // Apply pagination
        var surveys = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (surveys, totalCount);
    }

    public async Task<List<Survey>> GetOfficeDraftsByClerkAsync(Guid clerkId, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => s.FieldCollectorId == clerkId
                && s.Type == SurveyType.Office
                && s.Status == SurveyStatus.Draft
                && !s.IsDeleted)
            .OrderByDescending(s => s.LastModifiedAtUtc ?? s.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Survey>> GetByOfficeClerkAsync(Guid clerkId, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => s.FieldCollectorId == clerkId
                && s.Type == SurveyType.Office
                && !s.IsDeleted)
            .OrderByDescending(s => s.SurveyDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Survey>> GetFinalizedOfficeSurveysAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => s.Type == SurveyType.Office
                && s.Status == SurveyStatus.Finalized
                && !s.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.SurveyDate >= fromDate.Value);
        }

        return await query
            .OrderBy(s => s.SurveyDate)
            .ToListAsync(cancellationToken);
    }
    // ==================== AGGREGATE QUERIES (Dashboard) ====================

    public async Task<Dictionary<SurveyStatus, int>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Where(s => !s.IsDeleted)
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Where(s => !s.IsDeleted)
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetFinalizedCountSinceAsync(
        DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Where(s => !s.IsDeleted
                && s.Status >= SurveyStatus.Finalized
                && s.SurveyDate >= sinceUtc)
            .CountAsync(cancellationToken);
    }

    public async Task<Dictionary<SurveyType, int>> GetTypeCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Where(s => !s.IsDeleted)
            .GroupBy(s => s.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);
    }

    // ==================== DASHBOARD TREND QUERIES ====================

    public async Task<List<(int Year, int Month, int Count)>> GetMonthlyCreationCountsAsync(
        DateTime? from = null, DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Surveys.Where(s => !s.IsDeleted);
        if (from.HasValue) query = query.Where(s => s.CreatedAtUtc >= from.Value);
        if (to.HasValue) query = query.Where(s => s.CreatedAtUtc <= to.Value);

        var results = await query
            .GroupBy(s => new { s.CreatedAtUtc.Year, s.CreatedAtUtc.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.Year, r.Month, r.Count)).ToList();
    }

    public async Task<List<(Guid UserId, int Completed, int Draft, int Total)>> GetCountsByCollectorAsync(
        DateTime? from = null, DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Surveys.Where(s => !s.IsDeleted);
        if (from.HasValue) query = query.Where(s => s.CreatedAtUtc >= from.Value);
        if (to.HasValue) query = query.Where(s => s.CreatedAtUtc <= to.Value);

        var results = await query
            .GroupBy(s => s.FieldCollectorId)
            .Select(g => new
            {
                UserId = g.Key,
                Completed = g.Count(s => s.Status >= SurveyStatus.Finalized),
                Draft = g.Count(s => s.Status == SurveyStatus.Draft),
                Total = g.Count()
            })
            .ToListAsync(cancellationToken);

        return results.Select(r => (r.UserId, r.Completed, r.Draft, r.Total)).ToList();
    }
}