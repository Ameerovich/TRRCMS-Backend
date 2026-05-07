using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Neighborhoods.Commands.ImportAleppoNeighborhoods;

public class ImportAleppoNeighborhoodsCommandHandler
    : IRequestHandler<ImportAleppoNeighborhoodsCommand, NeighborhoodImportSummary>
{
    private readonly IAleppoNeighborhoodsImportService _importService;
    private readonly ICurrentUserService _currentUserService;

    public ImportAleppoNeighborhoodsCommandHandler(
        IAleppoNeighborhoodsImportService importService,
        ICurrentUserService currentUserService)
    {
        _importService = importService;
        _currentUserService = currentUserService;
    }

    public async Task<NeighborhoodImportSummary> Handle(
        ImportAleppoNeighborhoodsCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.JsonPayload))
            throw new ValidationException("JsonPayload is required.");

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        return await _importService.ApplyFromJsonAsync(request.JsonPayload, userId, cancellationToken);
    }
}
