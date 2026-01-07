using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.PersonPropertyRelations.Dtos;

namespace TRRCMS.Application.PersonPropertyRelations.Queries.GetAllPersonPropertyRelations;

/// <summary>
/// Handler for GetAllPersonPropertyRelationsQuery
/// </summary>
public class GetAllPersonPropertyRelationsQueryHandler : IRequestHandler<GetAllPersonPropertyRelationsQuery, IEnumerable<PersonPropertyRelationDto>>
{
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IMapper _mapper;

    public GetAllPersonPropertyRelationsQueryHandler(
        IPersonPropertyRelationRepository relationRepository,
        IMapper mapper)
    {
        _relationRepository = relationRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PersonPropertyRelationDto>> Handle(GetAllPersonPropertyRelationsQuery request, CancellationToken cancellationToken)
    {
        var relations = await _relationRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<PersonPropertyRelationDto>>(relations);
    }
}
