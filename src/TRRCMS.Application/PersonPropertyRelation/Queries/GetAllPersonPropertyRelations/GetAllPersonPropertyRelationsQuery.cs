using MediatR;
using TRRCMS.Application.PersonPropertyRelations.Dtos;

namespace TRRCMS.Application.PersonPropertyRelations.Queries.GetAllPersonPropertyRelations;

/// <summary>
/// Query to get all person-property relations
/// </summary>
public class GetAllPersonPropertyRelationsQuery : IRequest<IEnumerable<PersonPropertyRelationDto>>
{
}
