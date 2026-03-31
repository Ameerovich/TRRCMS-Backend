using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

public interface ILandmarkTypeIconRepository
{
    Task<List<LandmarkTypeIcon>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LandmarkTypeIcon?> GetByTypeAsync(LandmarkType type, CancellationToken cancellationToken = default);
    Task AddAsync(LandmarkTypeIcon icon, CancellationToken cancellationToken = default);
}
