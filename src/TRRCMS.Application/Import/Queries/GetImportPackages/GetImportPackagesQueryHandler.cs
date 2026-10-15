using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Import.Queries.GetImportPackages;

public class GetImportPackagesQueryHandler
    : IRequestHandler<GetImportPackagesQuery, GetImportPackagesResponse>
{
    private readonly IImportPackageRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IImportService _importService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetImportPackagesQueryHandler> _logger;

    public GetImportPackagesQueryHandler(
        IImportPackageRepository repository,
        IUserRepository userRepository,
        IImportService importService,
        IMapper mapper,
        ILogger<GetImportPackagesQueryHandler> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _importService = importService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GetImportPackagesResponse> Handle(
        GetImportPackagesQuery request,
        CancellationToken cancellationToken)
    {
        var (packages, totalCount) = await _repository.SearchAsync(
            status: request.Status,
            exportedByUserId: request.ExportedByUserId,
            importedByUserId: request.ImportedByUserId,
            importedAfter: request.ImportedAfter,
            importedBefore: request.ImportedBefore,
            searchTerm: request.SearchTerm,
            page: PagedQuery.ClampPageNumber(request.Page),
            pageSize: PagedQuery.ClampPageSize(request.PageSize),
            sortBy: request.SortBy,
            sortDescending: request.SortDescending,
            cancellationToken: cancellationToken);

        var items = _mapper.Map<List<ImportPackageDto>>(packages);

        var users = await _userRepository.GetAllAsync(cancellationToken);

        var userNames = users.ToDictionary(
            user => user.Id,
            user => !string.IsNullOrWhiteSpace(user.FullNameArabic)
                ? user.FullNameArabic
                : user.FullNameEnglish ?? user.Username);

        for (var i = 0; i < packages.Count; i++)
        {
            var package = packages[i];
            var dto = items[i];

            var collectorId = ResolveCollectorId(package);

            if (collectorId.HasValue &&
                userNames.TryGetValue(
                    collectorId.Value,
                    out var collectorName))
            {
                dto.CollectorName = collectorName;
            }

            var packagePath = ResolvePackagePath(package);

            if (packagePath is null)
                continue;

            try
            {
                dto.Buildings =
                    (await _importService.ReadBuildingSummariesAsync(
                        packagePath,
                        cancellationToken))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not read building display metadata for import package {PackageId}",
                    package.PackageId);
            }
        }

        return new GetImportPackagesResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    private static Guid? ResolveCollectorId(
        ImportPackage package)
    {
        if (package.ExportedByUserId != Guid.Empty)
            return package.ExportedByUserId;

        if (string.Equals(
                package.ImportMethod,
                "Sync",
                StringComparison.OrdinalIgnoreCase))
        {
            return package.ImportedByUserId;
        }

        return null;
    }

    private static string? ResolvePackagePath(
        ImportPackage package)
    {
        if (!string.IsNullOrWhiteSpace(package.UploadedFilePath) &&
            File.Exists(package.UploadedFilePath))
        {
            return package.UploadedFilePath;
        }

        if (!string.IsNullOrWhiteSpace(package.ArchivePath) &&
            File.Exists(package.ArchivePath))
        {
            return package.ArchivePath;
        }

        return null;
    }
}