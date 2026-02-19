namespace TRRCMS.Application.Sync.DTOs;

public sealed record CreateSyncSessionDto(
    Guid FieldCollectorId,
    string DeviceId,
    string? ServerIpAddress
);
