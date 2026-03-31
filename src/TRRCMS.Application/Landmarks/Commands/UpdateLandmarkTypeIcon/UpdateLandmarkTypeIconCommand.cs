using MediatR;
using TRRCMS.Application.Landmarks.Dtos;

namespace TRRCMS.Application.Landmarks.Commands.UpdateLandmarkTypeIcon;

public record UpdateLandmarkTypeIconCommand : IRequest<LandmarkTypeIconDto>
{
    public int Type { get; init; }
    public string SvgContent { get; init; } = string.Empty;
}
