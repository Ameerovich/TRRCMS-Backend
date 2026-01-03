using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

public interface IBuildingRepository
{
    Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Building?> GetByBuildingIdAsync(string buildingId, CancellationToken cancellationToken = default);
    Task<List<Building>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Building> AddAsync(Building building, CancellationToken cancellationToken = default);
    Task UpdateAsync(Building building, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}