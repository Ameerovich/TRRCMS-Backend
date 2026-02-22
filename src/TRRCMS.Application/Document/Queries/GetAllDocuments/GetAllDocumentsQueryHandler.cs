using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Documents.Dtos;

namespace TRRCMS.Application.Documents.Queries.GetAllDocuments;

/// <summary>
/// Handler for GetAllDocumentsQuery
/// </summary>
public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, PagedResult<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMapper _mapper;

    public GetAllDocumentsQueryHandler(
        IDocumentRepository documentRepository,
        IMapper mapper)
    {
        _documentRepository = documentRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<DocumentDto>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<List<DocumentDto>>(documents);
        return PaginatedList.FromEnumerable(dtos, request.PageNumber, request.PageSize);
    }
}
