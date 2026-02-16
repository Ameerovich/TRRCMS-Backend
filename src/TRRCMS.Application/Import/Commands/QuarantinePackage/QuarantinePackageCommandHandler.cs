using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Import.Commands.QuarantinePackage;

public class QuarantinePackageCommandHandler : IRequestHandler<QuarantinePackageCommand, ImportPackageDto>
{
    private readonly IImportPackageRepository _importPackageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<QuarantinePackageCommandHandler> _logger;

    public QuarantinePackageCommandHandler(
        IImportPackageRepository importPackageRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<QuarantinePackageCommandHandler> logger)
    {
        _importPackageRepository = importPackageRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ImportPackageDto> Handle(
        QuarantinePackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _importPackageRepository.GetByIdAsync(request.ImportPackageId, cancellationToken)
            ?? throw new NotFoundException($"ImportPackage with ID '{request.ImportPackageId}' was not found.");

        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user context is required for quarantine.");

        var terminalStatuses = new[]
        {
            ImportStatus.Completed, ImportStatus.PartiallyCompleted, ImportStatus.Cancelled
        };

        if (terminalStatuses.Contains(package.Status))
        {
            throw new ConflictException(
                $"Cannot quarantine package in '{package.Status}' status. " +
                "Only active (non-terminal) imports can be quarantined.");
        }

        package.Quarantine(request.Reason, userId);

        await _importPackageRepository.UpdateAsync(package, cancellationToken);
        await _importPackageRepository.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "Import package {PackageNumber} quarantined by user {UserId}. Reason: {Reason}",
            package.PackageNumber, userId, request.Reason);

        return _mapper.Map<ImportPackageDto>(package);
    }
}
