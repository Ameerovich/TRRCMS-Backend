using MediatR;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.PersonPropertyRelations.Dtos;

namespace TRRCMS.Application.PersonPropertyRelations.Queries.GetAllPersonPropertyRelations;

/// <summary>
/// Query to get all person-property relations with pagination
/// </summary>
public class GetAllPersonPropertyRelationsQuery : PagedQuery, IRequest<PagedResult<PersonPropertyRelationDto>>
{
}
