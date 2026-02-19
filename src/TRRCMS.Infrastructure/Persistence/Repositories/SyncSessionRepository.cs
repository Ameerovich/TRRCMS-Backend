using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

public class SyncSessionRepository : ISyncSessionRepository
{
    private readonly ApplicationDbContext _context;

    public SyncSessionRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<SyncSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SyncSessions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<SyncSession> AddAsync(SyncSession session, CancellationToken cancellationToken = default)
    {
        await _context.SyncSessions.AddAsync(session, cancellationToken);
        return session;
    }

    public Task UpdateAsync(SyncSession session, CancellationToken cancellationToken = default)
    {
        _context.SyncSessions.Update(session);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<SyncSession?> GetLatestByDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        deviceId = deviceId?.Trim() ?? string.Empty;

        return await _context.SyncSessions
            .Where(x => x.DeviceId == deviceId)
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<SyncSession>> GetByFieldCollectorAsync(
        Guid fieldCollectorId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        SyncSessionStatus? status = null,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0) take = 50;
        if (take > 500) take = 500;

        var query = _context.SyncSessions
            .Where(x => x.FieldCollectorId == fieldCollectorId);

        if (fromUtc.HasValue)
            query = query.Where(x => x.StartedAtUtc >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(x => x.StartedAtUtc <= toUtc.Value);

        if (status.HasValue)
            query = query.Where(x => x.SessionStatus == status.Value);

        return await query
            .OrderByDescending(x => x.StartedAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
