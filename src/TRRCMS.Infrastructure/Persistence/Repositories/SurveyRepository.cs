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

    public async Task<int> GetNextReferenceSequenceAsync(CancellationToken cancellationToken = default)
    {
        // Get current year for reference code prefix
        var currentYear = DateTime.UtcNow.Year;

        // Get all reference codes for current year (both ALG- and OFC-)
        var referenceCodes = await _context.Surveys
            .Where(s => s.ReferenceCode.Contains($"-{currentYear}-"))
            .Select(s => s.ReferenceCode)
            .ToListAsync(cancellationToken);

        // If no surveys exist for this year, start at 1
        if (!referenceCodes.Any())
            return 1;

        // Extract sequence numbers and find max
        var sequences = referenceCodes
            .Select(code =>
            {
                var parts = code.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int seq))
                    return seq;
                return 0;
            })
            .Where(seq => seq > 0)
            .ToList();

        // If no valid sequences found, start at 1
        if (!sequences.Any())
            return 1;

        // Return next sequence number
        return sequences.Max() + 1;
    }

    // ==================== FIELD SURVEY METHODS ====================

    public async Task<List<Survey>> GetByFieldCollectorAsync(Guid fieldCollectorId, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => s.FieldCollectorId == fieldCollectorId
                && s.Type == SurveyType.Field
                && !s.IsDeleted)
            .OrderByDescending(s => s.SurveyDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Survey>> GetDraftsByCollectorAsync(Guid fieldCollectorId, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => s.FieldCollectorId == fieldCollectorId
                && s.Type == SurveyType.Field
                && s.Status == SurveyStatus.Draft
                && !s.IsDeleted)
            .OrderByDescending(s => s.LastModifiedAtUtc ?? s.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Survey>> GetFinalizedSurveysAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => s.Type == SurveyType.Field
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

    // ==================== OFFICE SURVEY METHODS ====================

    public async Task<(List<Survey> Surveys, int TotalCount)> GetOfficeSurveysAsync(
        string? status = null,
        Guid? buildingId = null,
        Guid? clerkId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? referenceCode = null,
        string? intervieweeName = null,
        int page = 1,
        int pageSize = 20,
        string sortBy = "SurveyDate",
        string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var query = _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => s.Type == SurveyType.Office && !s.IsDeleted);

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

        if (!string.IsNullOrWhiteSpace(intervieweeName))
        {
            query = query.Where(s => s.IntervieweeName != null &&
                s.IntervieweeName.Contains(intervieweeName));
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
}