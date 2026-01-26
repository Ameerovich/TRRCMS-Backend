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
    /// <summary>
    /// Get field surveys with filtering and pagination
    /// </summary>
    public async Task<List<Survey>> GetFieldSurveysAsync(
        FieldSurveyFilterCriteria criteria,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFieldSurveyQuery(criteria);

        // Apply sorting
        query = ApplyFieldSurveySorting(query, criteria.SortBy, criteria.SortDirection);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        query = query.Skip(skip).Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get total count of field surveys matching criteria
    /// </summary>
    public async Task<int> GetFieldSurveysCountAsync(
        FieldSurveyFilterCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFieldSurveyQuery(criteria);
        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Get draft field surveys for a specific collector with pagination
    /// </summary>
    public async Task<(List<Survey> Surveys, int TotalCount)> GetFieldDraftSurveysByCollectorAsync(
        Guid fieldCollectorId,
        Guid? buildingId,
        int page,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => s.Type == SurveyType.Field
                && s.FieldCollectorId == fieldCollectorId
                && s.Status == SurveyStatus.Draft
                && !s.IsDeleted);

        // Apply building filter if provided
        if (buildingId.HasValue)
        {
            query = query.Where(s => s.BuildingId == buildingId.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplyFieldSurveySorting(query, sortBy, sortDirection);

        // Apply pagination
        var skip = (page - 1) * pageSize;
        var surveys = await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);

        return (surveys, totalCount);
    }

    /// <summary>
    /// Build query for field surveys with filters
    /// </summary>
    private IQueryable<Survey> BuildFieldSurveyQuery(FieldSurveyFilterCriteria criteria)
    {
        var query = _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => s.Type == SurveyType.Field && !s.IsDeleted);

        // Apply filters
        if (criteria.Status.HasValue)
        {
            query = query.Where(s => s.Status == criteria.Status.Value);
        }

        if (criteria.BuildingId.HasValue)
        {
            query = query.Where(s => s.BuildingId == criteria.BuildingId.Value);
        }

        if (criteria.FieldCollectorId.HasValue)
        {
            query = query.Where(s => s.FieldCollectorId == criteria.FieldCollectorId.Value);
        }

        if (criteria.PropertyUnitId.HasValue)
        {
            query = query.Where(s => s.PropertyUnitId == criteria.PropertyUnitId.Value);
        }

        if (criteria.FromDate.HasValue)
        {
            query = query.Where(s => s.SurveyDate >= criteria.FromDate.Value);
        }

        if (criteria.ToDate.HasValue)
        {
            query = query.Where(s => s.SurveyDate <= criteria.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(criteria.ReferenceCode))
        {
            query = query.Where(s => s.ReferenceCode.Contains(criteria.ReferenceCode));
        }

        if (!string.IsNullOrWhiteSpace(criteria.IntervieweeName))
        {
            query = query.Where(s => s.IntervieweeName != null
                && s.IntervieweeName.Contains(criteria.IntervieweeName));
        }

        return query;
    }

    /// <summary>
    /// Apply sorting to field survey query
    /// </summary>
    private IQueryable<Survey> ApplyFieldSurveySorting(
        IQueryable<Survey> query,
        string sortBy,
        string sortDirection)
    {
        var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLower() switch
        {
            "surveydate" => isDescending
                ? query.OrderByDescending(s => s.SurveyDate)
                : query.OrderBy(s => s.SurveyDate),
            "referencecode" => isDescending
                ? query.OrderByDescending(s => s.ReferenceCode)
                : query.OrderBy(s => s.ReferenceCode),
            "status" => isDescending
                ? query.OrderByDescending(s => s.Status)
                : query.OrderBy(s => s.Status),
            "createdatutc" => isDescending
                ? query.OrderByDescending(s => s.CreatedAtUtc)
                : query.OrderBy(s => s.CreatedAtUtc),
            "lastmodifiedatutc" => isDescending
                ? query.OrderByDescending(s => s.LastModifiedAtUtc ?? s.CreatedAtUtc)
                : query.OrderBy(s => s.LastModifiedAtUtc ?? s.CreatedAtUtc),
            "buildingid" => isDescending
                ? query.OrderByDescending(s => s.Building != null ? s.Building.BuildingNumber : "")
                : query.OrderBy(s => s.Building != null ? s.Building.BuildingNumber : ""),
            _ => isDescending
                ? query.OrderByDescending(s => s.SurveyDate)
                : query.OrderBy(s => s.SurveyDate)
        };
    }
}