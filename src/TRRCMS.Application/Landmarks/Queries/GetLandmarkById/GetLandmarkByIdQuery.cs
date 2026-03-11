using MediatR;
using TRRCMS.Application.Landmarks.Dtos;

namespace TRRCMS.Application.Landmarks.Queries.GetLandmarkById;

public record GetLandmarkByIdQuery : IRequest<LandmarkDto>
{
    public Guid Id { get; init; }
}
