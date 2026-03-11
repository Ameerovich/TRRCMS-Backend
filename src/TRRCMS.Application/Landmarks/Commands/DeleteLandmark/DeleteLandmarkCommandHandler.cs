using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Landmarks.Commands.DeleteLandmark;

public class DeleteLandmarkCommandHandler : IRequestHandler<DeleteLandmarkCommand, Unit>
{
    private readonly ILandmarkRepository _landmarkRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteLandmarkCommandHandler(
        ILandmarkRepository landmarkRepository,
        ICurrentUserService currentUserService)
    {
        _landmarkRepository = landmarkRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteLandmarkCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var landmark = await _landmarkRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Landmark with ID {request.Id} not found.");

        landmark.MarkAsDeleted(userId);
        await _landmarkRepository.UpdateAsync(landmark, cancellationToken);
        await _landmarkRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
