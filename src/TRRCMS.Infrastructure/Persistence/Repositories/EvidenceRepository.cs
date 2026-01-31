using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

public class EvidenceRepository : IEvidenceRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public EvidenceRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Evidence?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Evidence>()
            .Include(e => e.Person)
            .Include(e => e.PersonPropertyRelation)
            .Include(e => e.PreviousVersion)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Evidence>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Evidence>()
            .Include(e => e.Person)
            .Include(e => e.PersonPropertyRelation)
            .Where(e => !e.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Evidence>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Evidence>()
            .Include(e => e.Person)
            .Include(e => e.PersonPropertyRelation)
            .Where(e => e.PersonId == personId && !e.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Evidence>> GetByRelationIdAsync(Guid relationId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Evidence>()
            .Include(e => e.Person)
            .Include(e => e.PersonPropertyRelation)
            .Where(e => e.PersonPropertyRelationId == relationId && !e.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Evidence>> GetByRelationIdAsync(
        Guid relationId,
        EvidenceType? evidenceType,
        bool onlyCurrentVersions,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Evidence>()
            .Include(e => e.Person)
            .Include(e => e.PersonPropertyRelation)
            .Where(e => e.PersonPropertyRelationId == relationId && !e.IsDeleted);

        if (evidenceType.HasValue)
            query = query.Where(e => e.EvidenceType == evidenceType.Value);

        if (onlyCurrentVersions)
            query = query.Where(e => e.IsCurrentVersion);

        return await query.OrderByDescending(e => e.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Evidence>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Evidence>()
            .Include(e => e.Person)
            .Include(e => e.PersonPropertyRelation)
            .Where(e => e.ClaimId == claimId && !e.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Evidence>> GetCurrentVersionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Evidence>()
            .Include(e => e.Person)
            .Include(e => e.PersonPropertyRelation)
            .Where(e => e.IsCurrentVersion && !e.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Evidence>> GetBySurveyContextAsync(
        Guid buildingId,
        EvidenceType? evidenceType = null,
        CancellationToken cancellationToken = default)
    {
        var personIds = await _context.Persons
            .Where(p => p.HouseholdId.HasValue && !p.IsDeleted)
            .Where(p => _context.Households
                .Where(h => h.Id == p.HouseholdId && !h.IsDeleted)
                .Where(h => _context.PropertyUnits
                    .Any(pu => pu.Id == h.PropertyUnitId && pu.BuildingId == buildingId && !pu.IsDeleted))
                .Any())
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var relationIds = await _context.PersonPropertyRelations
            .Where(ppr => !ppr.IsDeleted)
            .Where(ppr => _context.PropertyUnits
                .Any(pu => pu.Id == ppr.PropertyUnitId && pu.BuildingId == buildingId && !pu.IsDeleted))
            .Select(ppr => ppr.Id)
            .ToListAsync(cancellationToken);

        var query = _context.Evidences
            .Where(e => !e.IsDeleted)
            .Where(e => (e.PersonId.HasValue && personIds.Contains(e.PersonId.Value))
                     || (e.PersonPropertyRelationId.HasValue && relationIds.Contains(e.PersonPropertyRelationId.Value)))
            .Where(e => e.IsCurrentVersion);

        // Apply evidence type filter using enum
        if (evidenceType.HasValue)
            query = query.Where(e => e.EvidenceType == evidenceType.Value);

        return await query.OrderByDescending(e => e.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Evidence>> GetVersionHistoryAsync(Guid evidenceId, CancellationToken cancellationToken = default)
    {
        var currentVersion = await _context.Set<Evidence>()
            .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

        if (currentVersion == null)
            return Enumerable.Empty<Evidence>();

        var versions = new List<Evidence> { currentVersion };
        var previousVersionId = currentVersion.PreviousVersionId;

        while (previousVersionId.HasValue)
        {
            var previousVersion = await _context.Set<Evidence>()
                .FirstOrDefaultAsync(e => e.Id == previousVersionId.Value, cancellationToken);

            if (previousVersion == null)
                break;

            versions.Add(previousVersion);
            previousVersionId = previousVersion.PreviousVersionId;
        }

        return versions.OrderByDescending(v => v.VersionNumber);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Evidence>().AnyAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public async Task<Evidence> AddAsync(Evidence evidence, CancellationToken cancellationToken = default)
    {
        await _context.Set<Evidence>().AddAsync(evidence, cancellationToken);
        return evidence;
    }

    public Task UpdateAsync(Evidence evidence, CancellationToken cancellationToken = default)
    {
        _context.Set<Evidence>().Update(evidence);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Evidence evidence, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId ?? Guid.Empty;
        evidence.MarkAsDeleted(currentUserId);
        _context.Set<Evidence>().Update(evidence);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
