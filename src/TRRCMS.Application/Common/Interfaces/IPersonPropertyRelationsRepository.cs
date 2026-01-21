using MediatR;
using System.Threading;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for PersonPropertyRelation entity operations
/// </summary>
public interface IPersonPropertyRelationRepository
{
    /// <summary>
    /// Get person-property relation by ID
    /// </summary>
    Task<PersonPropertyRelation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all person-property relations
    /// </summary>
    Task<IEnumerable<PersonPropertyRelation>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get person-property relations by person ID
    /// </summary>
    Task<IEnumerable<PersonPropertyRelation>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get person-property relations by property unit ID
    /// </summary>
    Task<IEnumerable<PersonPropertyRelation>> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active relations for a person
    /// </summary>
    Task<IEnumerable<PersonPropertyRelation>> GetActiveRelationsByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the relation for a person and a propertyunit
    /// </summary>
    Task<PersonPropertyRelation?> GetByPersonAndPropertyUnitAsync(Guid personId, Guid propertyUnitId, CancellationToken cancellationToken = default);
 
    /// <summary>
    /// Check if person-property relation exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new person-property relation
    /// </summary>
    Task<PersonPropertyRelation> AddAsync(PersonPropertyRelation relation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update person-property relation
    /// </summary>
    Task UpdateAsync(PersonPropertyRelation relation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes to database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
