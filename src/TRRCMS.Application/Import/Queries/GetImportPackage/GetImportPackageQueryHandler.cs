using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Queries.GetImportPackage;

public class GetImportPackageQueryHandler : IRequestHandler<GetImportPackageQuery, ImportPackageDto?>
{
    private readonly IImportPackageRepository _repository;
    private readonly IMapper _mapper;

    public GetImportPackageQueryHandler(IImportPackageRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ImportPackageDto?> Handle(GetImportPackageQuery request, CancellationToken cancellationToken)
    {
        var package = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return package is null ? null : _mapper.Map<ImportPackageDto>(package);
    }
}
