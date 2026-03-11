using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Streets.Commands.DeleteStreet;

public class DeleteStreetCommandHandler : IRequestHandler<DeleteStreetCommand, Unit>
{
    private readonly IStreetRepository _streetRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteStreetCommandHandler(
        IStreetRepository streetRepository,
        ICurrentUserService currentUserService)
    {
        _streetRepository = streetRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteStreetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var street = await _streetRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Street with ID {request.Id} not found.");

        street.MarkAsDeleted(userId);
        await _streetRepository.UpdateAsync(street, cancellationToken);
        await _streetRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
