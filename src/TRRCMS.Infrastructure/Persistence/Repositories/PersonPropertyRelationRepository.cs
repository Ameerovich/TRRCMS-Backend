using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

public class PersonPropertyRelationRepository : IPersonPropertyRelationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public PersonPropertyRelationRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PersonPropertyRelation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .Include(r => r.Person)
            .Include(r => r.PropertyUnit)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    public async Task<PersonPropertyRelation?> GetByIdWithEvidencesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .Include(r => r.Person)
            .Include(r => r.PropertyUnit)
            .Include(r => r.EvidenceRelations.Where(er => !er.IsDeleted && er.IsActive))
                .ThenInclude(er => er.Evidence)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<PersonPropertyRelation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .Include(r => r.Person)
            .Include(r => r.PropertyUnit)
            .Where(r => !r.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PersonPropertyRelation>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .Include(r => r.Person)
            .Include(r => r.PropertyUnit)
            .Where(r => r.PersonId == personId && !r.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PersonPropertyRelation>> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .Include(r => r.Person)
            .Include(r => r.PropertyUnit)
            .Where(r => r.PropertyUnitId == propertyUnitId && !r.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PersonPropertyRelation>> GetByPropertyUnitIdWithEvidencesAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .Include(r => r.Person)
            .Include(r => r.PropertyUnit)
            .Include(r => r.EvidenceRelations.Where(er => !er.IsDeleted && er.IsActive))
                .ThenInclude(er => er.Evidence)
            .Where(r => r.PropertyUnitId == propertyUnitId && !r.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PersonPropertyRelation>> GetBySurveyIdWithEvidencesAsync(Guid surveyId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .Include(r => r.Person)
            .Include(r => r.PropertyUnit)
            .Include(r => r.EvidenceRelations.Where(er => !er.IsDeleted && er.IsActive))
                .ThenInclude(er => er.Evidence)
            .Where(r => r.SurveyId == surveyId && !r.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PersonPropertyRelation>> GetActiveRelationsByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .Include(r => r.Person)
            .Include(r => r.PropertyUnit)
            .Where(r => r.PersonId == personId && r.IsActive && !r.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<PersonPropertyRelation?> GetByPersonAndPropertyUnitAsync(Guid personId, Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .Include(r => r.Person)
            .Include(r => r.PropertyUnit)
            .FirstOrDefaultAsync(r => r.PersonId == personId && r.PropertyUnitId == propertyUnitId && !r.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>().AnyAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
    }

    public async Task<PersonPropertyRelation> AddAsync(PersonPropertyRelation relation, CancellationToken cancellationToken = default)
    {
        await _context.Set<PersonPropertyRelation>().AddAsync(relation, cancellationToken);
        return relation;
    }

    public Task UpdateAsync(PersonPropertyRelation relation, CancellationToken cancellationToken = default)
    {
        _context.Set<PersonPropertyRelation>().Update(relation);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(PersonPropertyRelation relation, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId ?? Guid.Empty;
        relation.MarkAsDeleted(currentUserId);
        _context.Set<PersonPropertyRelation>().Update(relation);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PersonPropertyRelations.Where(r => !r.IsDeleted).CountAsync(cancellationToken);
    }

    public async Task<Dictionary<Domain.Enums.RelationType, int>> GetRelationTypeCountsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.PersonPropertyRelations
            .Where(r => !r.IsDeleted)
            .GroupBy(r => r.RelationType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);
    }

    public async Task<int> GetCountWithEvidenceAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PersonPropertyRelations
            .Where(r => !r.IsDeleted)
            .Where(r => r.EvidenceRelations.Any(er => !er.IsDeleted && er.IsActive))
            .CountAsync(cancellationToken);
    }
}
