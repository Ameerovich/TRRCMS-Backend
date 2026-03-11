using MediatR;

namespace TRRCMS.Application.Landmarks.Commands.DeleteLandmark;

/// <summary>
/// Soft-delete a landmark.
/// حذف معلم
/// </summary>
public record DeleteLandmarkCommand : IRequest<Unit>
{
    /// <summary>Landmark ID (GUID)</summary>
    public Guid Id { get; init; }
}
