using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Import.Commands.CancelPackage;

public class CancelPackageCommandHandler : IRequestHandler<CancelPackageCommand, ImportPackageDto>
{
    private readonly IImportPackageRepository _importPackageRepository;
    private readonly IStagingService _stagingService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelPackageCommandHandler> _logger;

    public CancelPackageCommandHandler(
        IImportPackageRepository importPackageRepository,
        IStagingService stagingService,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<CancelPackageCommandHandler> logger)
    {
        _importPackageRepository = importPackageRepository;
        _stagingService = stagingService;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ImportPackageDto> Handle(
        CancelPackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _importPackageRepository.GetByIdAsync(request.ImportPackageId, cancellationToken)
            ?? throw new NotFoundException($"ImportPackage with ID '{request.ImportPackageId}' was not found.");

        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user context is required for cancellation.");

        var terminalStatuses = new[]
        {
            ImportStatus.Completed, ImportStatus.PartiallyCompleted, ImportStatus.Cancelled
        };

        if (terminalStatuses.Contains(package.Status))
        {
            throw new InvalidOperationException(
                $"Cannot cancel package in '{package.Status}' status. " +
                "Only active (non-terminal) imports can be cancelled.");
        }

        package.Cancel(request.Reason, userId);

        await _importPackageRepository.UpdateAsync(package, cancellationToken);
        await _importPackageRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Import package {PackageNumber} cancelled by user {UserId}. Reason: {Reason}",
            package.PackageNumber, userId, request.Reason);

        if (request.CleanupStaging)
        {
            try
            {
                await _stagingService.CleanupStagingAsync(request.ImportPackageId, cancellationToken);
                _logger.LogInformation("Staging data cleaned up for cancelled package {PackageNumber}",
                    package.PackageNumber);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to cleanup staging data for cancelled package {PackageNumber}.",
                    package.PackageNumber);
            }
        }

        return _mapper.Map<ImportPackageDto>(package);
    }
}
