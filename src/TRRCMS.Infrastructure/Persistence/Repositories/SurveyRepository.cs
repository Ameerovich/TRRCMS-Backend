using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Survey entity
/// </summary>
public class SurveyRepository : ISurveyRepository
{
    private readonly ApplicationDbContext _context;

    public SurveyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

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

    public async Task<List<Survey>> GetByFieldCollectorAsync(Guid fieldCollectorId, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => s.FieldCollectorId == fieldCollectorId && !s.IsDeleted)
            .OrderByDescending(s => s.SurveyDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Survey>> GetDraftsByCollectorAsync(Guid fieldCollectorId, CancellationToken cancellationToken = default)
    {
        return await _context.Surveys
            .Include(s => s.Building)
            .Include(s => s.PropertyUnit)
            .Where(s => s.FieldCollectorId == fieldCollectorId
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
            .Where(s => s.Status == SurveyStatus.Finalized && !s.IsDeleted);

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.SurveyDate >= fromDate.Value);
        }

        return await query
            .OrderBy(s => s.SurveyDate)
            .ToListAsync(cancellationToken);
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
        var prefix = $"ALG-{currentYear}-";

        // Get all reference codes for current year
        var referenceCodes = await _context.Surveys
            .Where(s => s.ReferenceCode.StartsWith(prefix))
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
}