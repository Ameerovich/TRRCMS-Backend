using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Queries.GetImportPackages;

public class GetImportPackagesQueryHandler : IRequestHandler<GetImportPackagesQuery, GetImportPackagesResponse>
{
    private readonly IImportPackageRepository _repository;
    private readonly IMapper _mapper;

    public GetImportPackagesQueryHandler(IImportPackageRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<GetImportPackagesResponse> Handle(
        GetImportPackagesQuery request, CancellationToken cancellationToken)
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

        return new GetImportPackagesResponse
        {
            Items = _mapper.Map<List<ImportPackageDto>>(packages),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
