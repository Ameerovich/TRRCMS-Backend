using MediatR;
using TRRCMS.Application.Sync.DTOs;

namespace TRRCMS.Application.Sync.Commands.UploadSyncPackage;

/// <summary>
/// Upload package stream + manifest.
/// Stream is handled in Application to allow checksum verification and storage abstraction.
/// </summary>
public sealed record UploadSyncPackageCommand(UploadSyncPackageDto Manifest, Stream PackageStream)
    : IRequest<UploadSyncPackageResultDto>;
