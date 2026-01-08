using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Evidence entity
/// </summary>
public class EvidenceRepository : IEvidenceRepository
{
    private readonly ApplicationDbContext _context;

    public EvidenceRepository(ApplicationDbContext context)
    {
        _context = context;
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

    public async Task<IEnumerable<Evidence>> GetVersionHistoryAsync(Guid evidenceId, CancellationToken cancellationToken = default)
    {
        // Find the current version first
        var currentVersion = await _context.Set<Evidence>()
            .FirstOrDefaultAsync(e => e.Id == evidenceId, cancellationToken);

        if (currentVersion == null)
            return Enumerable.Empty<Evidence>();

        // Get all versions in the chain by following PreviousVersionId backward
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

        // Order by version number descending (newest first)
        return versions.OrderByDescending(v => v.VersionNumber);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Evidence>()
            .AnyAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
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

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
