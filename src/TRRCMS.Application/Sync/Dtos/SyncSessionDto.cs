using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Sync.DTOs;

public sealed record SyncSessionDto(
    Guid Id,
    Guid FieldCollectorId,
    string DeviceId,
    string? ServerIpAddress,
    SyncSessionStatus SessionStatus,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    int PackagesUploaded,
    int PackagesFailed,
    int AssignmentsDownloaded,
    int AssignmentsAcknowledged,
    string? VocabularyVersionsSent,
    string? ErrorMessage
);
