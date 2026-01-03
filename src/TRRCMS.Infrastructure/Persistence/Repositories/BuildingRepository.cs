using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

public class BuildingRepository : IBuildingRepository
{
    private readonly ApplicationDbContext _context;

    public BuildingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Buildings
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
    }

    public async Task<Building?> GetByBuildingIdAsync(string buildingId, CancellationToken cancellationToken = default)
    {
        return await _context.Buildings
            .FirstOrDefaultAsync(b => b.BuildingId == buildingId && !b.IsDeleted, cancellationToken);
    }

    public async Task<List<Building>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Buildings
            .Where(b => !b.IsDeleted)
            .OrderBy(b => b.BuildingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Building> AddAsync(Building building, CancellationToken cancellationToken = default)
    {
        await _context.Buildings.AddAsync(building, cancellationToken);
        return building;
    }

    public async Task UpdateAsync(Building building, CancellationToken cancellationToken = default)
    {
        _context.Buildings.Update(building);
        await Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}