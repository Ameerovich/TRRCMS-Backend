using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for SyncSession operations.
/// Supports LAN Sync telemetry and auditability.
/// </summary>
public interface ISyncSessionRepository
{
    // ==================== BASIC CRUD ====================

    Task<SyncSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SyncSession> AddAsync(SyncSession session, CancellationToken cancellationToken = default);

    Task UpdateAsync(SyncSession session, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // ==================== QUERIES ====================

    Task<SyncSession?> GetLatestByDeviceAsync(
        string deviceId,
        CancellationToken cancellationToken = default);

    Task<List<SyncSession>> GetByFieldCollectorAsync(
        Guid fieldCollectorId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        SyncSessionStatus? status = null,
        int take = 50,
        CancellationToken cancellationToken = default);
}
