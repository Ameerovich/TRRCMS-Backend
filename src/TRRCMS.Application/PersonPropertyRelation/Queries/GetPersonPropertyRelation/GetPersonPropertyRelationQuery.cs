using MediatR;
using TRRCMS.Application.PersonPropertyRelations.Dtos;

namespace TRRCMS.Application.PersonPropertyRelations.Queries.GetPersonPropertyRelation;

/// <summary>
/// Query to get a person-property relation by ID
/// </summary>
public class GetPersonPropertyRelationQuery : IRequest<PersonPropertyRelationDto?>
{
    public Guid Id { get; }

    public GetPersonPropertyRelationQuery(Guid id)
    {
        Id = id;
    }
}
