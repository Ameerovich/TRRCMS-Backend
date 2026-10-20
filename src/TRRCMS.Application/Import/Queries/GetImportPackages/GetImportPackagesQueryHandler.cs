using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Application.Import.Models;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Import.Queries.GetImportPackages;

public class GetImportPackagesQueryHandler
    : IRequestHandler<GetImportPackagesQuery, GetImportPackagesResponse>
{
    private readonly IImportPackageRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IBuildingAssignmentRepository _assignmentRepository;
    private readonly IAdministrativeNameResolver _nameResolver;
    private readonly IImportService _importService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetImportPackagesQueryHandler> _logger;

    public GetImportPackagesQueryHandler(
        IImportPackageRepository repository,
        IUserRepository userRepository,
        IBuildingAssignmentRepository assignmentRepository,
        IAdministrativeNameResolver nameResolver,
        IImportService importService,
        IMapper mapper,
        ILogger<GetImportPackagesQueryHandler> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _assignmentRepository = assignmentRepository;
        _nameResolver = nameResolver;
        _importService = importService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GetImportPackagesResponse> Handle(
        GetImportPackagesQuery request,
        CancellationToken cancellationToken)
    {
        var page = PagedQuery.ClampPageNumber(request.Page);
        var pageSize = PagedQuery.ClampPageSize(request.PageSize);

        var (packages, totalCount) = await _repository.SearchAsync(
            status: request.Status,
            exportedByUserId: request.ExportedByUserId,
            importedByUserId: request.ImportedByUserId,
            importedAfter: request.ImportedAfter,
            importedBefore: request.ImportedBefore,
            searchTerm: request.SearchTerm,
            page: page,
            pageSize: pageSize,
            sortBy: request.SortBy,
            sortDescending: request.SortDescending,
            cancellationToken: cancellationToken);

        var items = _mapper.Map<List<ImportPackageDto>>(packages);

        var collectorIds = packages.Select(ResolveCollectorId).ToList();
        var distinctCollectorIds = collectorIds.OfType<Guid>().Distinct().ToList();

        var buildingsPerPackage = new List<IReadOnlyList<UhcBuildingSummary>>(packages.Count);
        foreach (var package in packages)
            buildingsPerPackage.Add(await ReadBuildingSummariesAsync(package, cancellationToken));

        // "Assigned by" — the supervisor who assigned each building to the package's collector.
        // One query for the whole page, not one per building.
        var buildingCodes = buildingsPerPackage
            .SelectMany(buildings => buildings)
            .Select(building => building.BuildingCode)
            .OfType<string>()
            .Distinct()
            .ToList();

        var assigners = (await _assignmentRepository.GetLatestAssignersAsync(
                buildingCodes,
                distinctCollectorIds,
                cancellationToken))
            .ToDictionary(
                a => (a.BuildingCode, a.FieldCollectorId),
                a => a.AssignedByUserId);

        // Only the users this page refers to (not every user entity, with credentials)
        var users = await _userRepository.GetByIdsAsync(
            distinctCollectorIds.Concat(assigners.Values.OfType<Guid>()).Distinct(),
            cancellationToken);

        var userNames = users.ToDictionary(
            user => user.Id,
            user => !string.IsNullOrWhiteSpace(user.FullNameArabic)
                ? user.FullNameArabic
                : user.FullNameEnglish ?? user.Username);

        // Many packages share a neighbourhood — resolve each code tuple once per request
        var adminNames = new Dictionary<(string, string, string, string, string), AdministrativeNames>();

        for (var i = 0; i < packages.Count; i++)
        {
            var dto = items[i];
            var collectorId = collectorIds[i];

            if (collectorId.HasValue &&
                userNames.TryGetValue(
                    collectorId.Value,
                    out var collectorName))
            {
                dto.CollectorName = collectorName;
            }

            foreach (var building in buildingsPerPackage[i])
            {
                var resolved = await ResolveAdministrativeNamesAsync(building, adminNames, cancellationToken);

                Guid? assignedByUserId = null;
                if (collectorId.HasValue && building.BuildingCode is not null)
                    assigners.TryGetValue((building.BuildingCode, collectorId.Value), out assignedByUserId);

                dto.Buildings.Add(new ImportPackageBuildingDto
                {
                    BuildingNumber = building.BuildingNumber,
                    GovernorateName = PreferFileName(building.GovernorateName, resolved?.GovernorateName),
                    DistrictName = PreferFileName(building.DistrictName, resolved?.DistrictName),
                    SubDistrictName = PreferFileName(building.SubDistrictName, resolved?.SubDistrictName),
                    CommunityName = PreferFileName(building.CommunityName, resolved?.CommunityName),
                    NeighborhoodName = PreferFileName(building.NeighborhoodName, resolved?.NeighborhoodName),
                    AssignedByName = assignedByUserId.HasValue &&
                                     userNames.TryGetValue(assignedByUserId.Value, out var assignedByName)
                        ? assignedByName
                        : null
                });
            }
        }

        return new GetImportPackagesResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Building rows of the package file; empty when the file is missing or unreadable
    /// (one bad package must not fail the whole list).
    /// </summary>
    private async Task<IReadOnlyList<UhcBuildingSummary>> ReadBuildingSummariesAsync(
        ImportPackage package,
        CancellationToken cancellationToken)
    {
        var packagePath = ResolvePackagePath(package);

        if (packagePath is null)
            return Array.Empty<UhcBuildingSummary>();

        try
        {
            return await _importService.ReadBuildingSummariesAsync(
                packagePath,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                "Could not read building display metadata for import package {PackageId}",
                package.PackageId);

            return Array.Empty<UhcBuildingSummary>();
        }
    }

    /// <summary>
    /// Arabic names for the building's administrative codes, or null when every name is
    /// already in the package or the codes are missing.
    /// </summary>
    private async Task<AdministrativeNames?> ResolveAdministrativeNamesAsync(
        UhcBuildingSummary building,
        Dictionary<(string, string, string, string, string), AdministrativeNames> cache,
        CancellationToken cancellationToken)
    {
        var hasAllNames =
            !string.IsNullOrWhiteSpace(building.GovernorateName) &&
            !string.IsNullOrWhiteSpace(building.DistrictName) &&
            !string.IsNullOrWhiteSpace(building.SubDistrictName) &&
            !string.IsNullOrWhiteSpace(building.CommunityName) &&
            !string.IsNullOrWhiteSpace(building.NeighborhoodName);

        if (hasAllNames || string.IsNullOrWhiteSpace(building.GovernorateCode))
            return null;

        var key = (
            building.GovernorateCode!,
            building.DistrictCode ?? string.Empty,
            building.SubDistrictCode ?? string.Empty,
            building.CommunityCode ?? string.Empty,
            building.NeighborhoodCode ?? string.Empty);

        if (!cache.TryGetValue(key, out var names))
        {
            names = await _nameResolver.ResolveAsync(
                key.Item1, key.Item2, key.Item3, key.Item4, key.Item5,
                cancellationToken);
            cache[key] = names;
        }

        return names;
    }

    /// <summary>
    /// A name written in the package wins over the one resolved from its codes;
    /// the resolver's empty string (code not found) becomes null.
    /// </summary>
    private static string? PreferFileName(string? fromFile, string? resolved) =>
        !string.IsNullOrWhiteSpace(fromFile) ? fromFile
        : !string.IsNullOrWhiteSpace(resolved) ? resolved
        : null;

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
