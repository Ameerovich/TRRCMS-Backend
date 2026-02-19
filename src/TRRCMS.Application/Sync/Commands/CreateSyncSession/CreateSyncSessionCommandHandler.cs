using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Sync.DTOs;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Sync.Commands.CreateSyncSession;

public sealed class CreateSyncSessionCommandHandler : IRequestHandler<CreateSyncSessionCommand, SyncSessionDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateSyncSessionCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<SyncSessionDto> Handle(CreateSyncSessionCommand request, CancellationToken ct)
    {
        var createdBy = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var entity = SyncSession.Create(
            request.Data.FieldCollectorId,
            request.Data.DeviceId,
            request.Data.ServerIpAddress,
            createdBy);

        await _uow.SyncSessions.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return new SyncSessionDto(
            entity.Id,
            entity.FieldCollectorId,
            entity.DeviceId,
            entity.ServerIpAddress,
            entity.SessionStatus,
            entity.StartedAtUtc,
            entity.CompletedAtUtc,
            entity.PackagesUploaded,
            entity.PackagesFailed,
            entity.AssignmentsDownloaded,
            entity.AssignmentsAcknowledged,
            entity.VocabularyVersionsSent,
            entity.ErrorMessage
        );
    }
}
