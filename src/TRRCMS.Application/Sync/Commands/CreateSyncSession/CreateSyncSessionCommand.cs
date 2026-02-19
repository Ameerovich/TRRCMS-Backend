using MediatR;
using TRRCMS.Application.Sync.DTOs;

namespace TRRCMS.Application.Sync.Commands.CreateSyncSession;

public sealed record CreateSyncSessionCommand(CreateSyncSessionDto Data)
    : IRequest<SyncSessionDto>;
