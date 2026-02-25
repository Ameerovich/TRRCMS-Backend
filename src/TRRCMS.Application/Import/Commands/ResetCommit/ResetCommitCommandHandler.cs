using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Commands.ResetCommit;

/// <summary>
/// Handles the ResetCommitCommand — resets a stuck/failed package back to ReadyToCommit.
///
/// Use case: the commit handler set status to Committing, but the actual commit
/// threw an exception and the catch block also failed (e.g. DB connectivity issue),
/// leaving the package stuck. This endpoint allows recovery without manual DB edits.
///
/// Only valid from Committing or Failed status.
/// </summary>
public class ResetCommitCommandHandler : IRequestHandler<ResetCommitCommand, ImportPackageDto>
{
    private readonly IImportPackageRepository _importPackageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<ResetCommitCommandHandler> _logger;

    public ResetCommitCommandHandler(
        IImportPackageRepository importPackageRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<ResetCommitCommandHandler> logger)
    {
        _importPackageRepository = importPackageRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ImportPackageDto> Handle(
        ResetCommitCommand request, CancellationToken cancellationToken)
    {
        var package = await _importPackageRepository.GetByIdAsync(request.ImportPackageId, cancellationToken)
            ?? throw new NotFoundException($"ImportPackage with ID '{request.ImportPackageId}' was not found.");

        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user context is required for reset.");

        // Domain method validates status (only Committing or Failed allowed)
        package.ResetToReadyToCommit(request.Reason, userId);

        await _importPackageRepository.UpdateAsync(package, cancellationToken);
        await _importPackageRepository.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "Import package {PackageNumber} reset from stuck/failed state to ReadyToCommit " +
            "by user {UserId}. Reason: {Reason}",
            package.PackageNumber, userId, request.Reason);

        return _mapper.Map<ImportPackageDto>(package);
    }
}
