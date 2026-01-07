using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for PersonPropertyRelation entity
/// </summary>
public class PersonPropertyRelationRepository : IPersonPropertyRelationRepository
{
    private readonly ApplicationDbContext _context;

    public PersonPropertyRelationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PersonPropertyRelation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .Include(r => r.Person)
            .Include(r => r.PropertyUnit)
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

    public async Task<IEnumerable<PersonPropertyRelation>> GetActiveRelationsByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .Include(r => r.Person)
            .Include(r => r.PropertyUnit)
            .Where(r => r.PersonId == personId && r.IsActive && !r.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PersonPropertyRelation>()
            .AnyAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
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

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
