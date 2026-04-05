using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Case entity
/// </summary>
public interface ICaseRepository
{
    Task<Case?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Case?> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForPropertyUnitAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);
    Task<(List<Case> Items, int TotalCount)> GetAllAsync(
        CaseLifecycleStatus? status = null,
        Guid? buildingId = null,
        string? buildingCode = null,
        string? unitIdentifier = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<CaseLifecycleStatus, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default);
    Task<List<(int Year, int Month, int Count)>> GetMonthlyCreationCountsAsync(
        DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task AddAsync(Case entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Case entity, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
