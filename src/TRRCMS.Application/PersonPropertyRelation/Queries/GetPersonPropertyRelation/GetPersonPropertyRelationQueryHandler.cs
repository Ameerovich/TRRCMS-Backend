using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.PersonPropertyRelations.Dtos;

namespace TRRCMS.Application.PersonPropertyRelations.Queries.GetPersonPropertyRelation;

/// <summary>
/// Handler for GetPersonPropertyRelationQuery
/// </summary>
public class GetPersonPropertyRelationQueryHandler : IRequestHandler<GetPersonPropertyRelationQuery, PersonPropertyRelationDto?>
{
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IMapper _mapper;

    public GetPersonPropertyRelationQueryHandler(
        IPersonPropertyRelationRepository relationRepository,
        IMapper mapper)
    {
        _relationRepository = relationRepository;
        _mapper = mapper;
    }

    public async Task<PersonPropertyRelationDto?> Handle(GetPersonPropertyRelationQuery request, CancellationToken cancellationToken)
    {
        var relation = await _relationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (relation == null)
            return null;

        return _mapper.Map<PersonPropertyRelationDto>(relation);
    }
}
