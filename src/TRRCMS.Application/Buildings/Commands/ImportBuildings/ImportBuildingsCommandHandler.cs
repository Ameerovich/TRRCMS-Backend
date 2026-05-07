using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Commands.ImportBuildings;

public class ImportBuildingsCommandHandler
    : IRequestHandler<ImportBuildingsCommand, BuildingsImportSummary>
{
    private readonly IBuildingsImportService _importService;
    private readonly ICurrentUserService _currentUserService;

    public ImportBuildingsCommandHandler(
        IBuildingsImportService importService,
        ICurrentUserService currentUserService)
    {
        _importService = importService;
        _currentUserService = currentUserService;
    }

    public async Task<BuildingsImportSummary> Handle(
        ImportBuildingsCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.JsonPayload))
            throw new ValidationException("JsonPayload is required.");

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        return await _importService.ApplyFromJsonAsync(request.JsonPayload, userId, cancellationToken);
    }
}
