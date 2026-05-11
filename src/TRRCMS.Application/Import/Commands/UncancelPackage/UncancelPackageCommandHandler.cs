using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Import.Commands.UncancelPackage;

public class UncancelPackageCommandHandler : IRequestHandler<UncancelPackageCommand, ImportPackageDto>
{
    private readonly IImportPackageRepository _importPackageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<UncancelPackageCommandHandler> _logger;

    public UncancelPackageCommandHandler(
        IImportPackageRepository importPackageRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<UncancelPackageCommandHandler> logger)
    {
        _importPackageRepository = importPackageRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ImportPackageDto> Handle(
        UncancelPackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _importPackageRepository.GetByIdAsync(request.ImportPackageId, cancellationToken)
            ?? throw new NotFoundException($"ImportPackage with ID '{request.ImportPackageId}' was not found.");

        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user context is required.");

        if (package.Status != ImportStatus.Cancelled)
            throw new ConflictException(
                $"Cannot uncancel package '{package.PackageNumber}'. Current status is '{package.Status}'. Only Cancelled packages can be uncancelled.");

        var restoredTo = package.PreviousStatus?.ToString() ?? "Pending";

        package.Uncancel(request.Reason, userId);

        await _importPackageRepository.UpdateAsync(package, cancellationToken);
        await _importPackageRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Import package {PackageNumber} uncancelled by user {UserId}. Restored to {RestoredStatus}. Reason: {Reason}",
            package.PackageNumber, userId, restoredTo, request.Reason ?? "not specified");

        return _mapper.Map<ImportPackageDto>(package);
    }
}
