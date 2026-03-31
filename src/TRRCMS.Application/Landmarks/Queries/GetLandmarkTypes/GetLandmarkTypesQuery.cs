using MediatR;
using TRRCMS.Application.Landmarks.Dtos;

namespace TRRCMS.Application.Landmarks.Queries.GetLandmarkTypes;

public record GetLandmarkTypesQuery : IRequest<List<LandmarkTypeIconDto>>;
