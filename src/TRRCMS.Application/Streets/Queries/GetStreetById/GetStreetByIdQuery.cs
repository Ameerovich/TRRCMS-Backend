using MediatR;
using TRRCMS.Application.Streets.Dtos;

namespace TRRCMS.Application.Streets.Queries.GetStreetById;

public record GetStreetByIdQuery : IRequest<StreetDto>
{
    public Guid Id { get; init; }
}
