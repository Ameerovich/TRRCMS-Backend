using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

public class LandmarkTypeIconRepository : ILandmarkTypeIconRepository
{
    private readonly ApplicationDbContext _context;

    public LandmarkTypeIconRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LandmarkTypeIcon>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<LandmarkTypeIcon>()
            .OrderBy(i => (int)i.Type)
            .ToListAsync(cancellationToken);
    }

    public async Task<LandmarkTypeIcon?> GetByTypeAsync(LandmarkType type, CancellationToken cancellationToken = default)
    {
        return await _context.Set<LandmarkTypeIcon>()
            .FirstOrDefaultAsync(i => i.Type == type, cancellationToken);
    }

    public async Task AddAsync(LandmarkTypeIcon icon, CancellationToken cancellationToken = default)
    {
        await _context.Set<LandmarkTypeIcon>().AddAsync(icon, cancellationToken);
    }
}
