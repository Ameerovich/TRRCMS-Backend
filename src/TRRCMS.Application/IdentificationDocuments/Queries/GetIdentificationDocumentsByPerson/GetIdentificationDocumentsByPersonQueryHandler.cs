using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.IdentificationDocuments.Dtos;

namespace TRRCMS.Application.IdentificationDocuments.Queries.GetIdentificationDocumentsByPerson;

/// <summary>
/// Handler for GetIdentificationDocumentsByPersonQuery.
/// Returns all non-deleted identification documents for the given person,
/// ordered by creation date (newest first).
/// </summary>
public class GetIdentificationDocumentsByPersonQueryHandler
    : IRequestHandler<GetIdentificationDocumentsByPersonQuery, List<IdentificationDocumentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetIdentificationDocumentsByPersonQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<IdentificationDocumentDto>> Handle(
        GetIdentificationDocumentsByPersonQuery request,
        CancellationToken cancellationToken)
    {
        var documents = await _unitOfWork.IdentificationDocuments.GetByPersonIdAsync(
            request.PersonId, cancellationToken);

        return _mapper.Map<List<IdentificationDocumentDto>>(documents);
    }
}
