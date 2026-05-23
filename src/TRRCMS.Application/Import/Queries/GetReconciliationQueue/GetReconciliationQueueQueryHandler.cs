using MediatR;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Import.Queries.GetReconciliationQueue;

/// <summary>
/// Handler for <see cref="GetReconciliationQueueQuery"/>. Loads the two kinds of
/// Keep-Separate adjustments that await manual reconciliation and maps them to DTOs.
/// </summary>
public class GetReconciliationQueueQueryHandler
    : IRequestHandler<GetReconciliationQueueQuery, ReconciliationQueueDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReconciliationQueueQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ReconciliationQueueDto> Handle(
        GetReconciliationQueueQuery request,
        CancellationToken cancellationToken)
    {
        var filter = request.EntityTypeFilter?.Trim().ToLowerInvariant();
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var response = new ReconciliationQueueDto { Page = page, PageSize = pageSize };

        if (filter is null or "" or "person")
        {
            var (persons, personTotal) = await _unitOfWork.Persons
                .GetPendingNationalIdReconciliationAsync(page, pageSize, cancellationToken);

            response.PersonsTotalCount = personTotal;
            response.Persons = persons.Select(p => new PersonNationalIdReconciliationDto
            {
                PersonId = p.Id,
                FullNameArabic = p.GetFullNameArabic(),
                PreservedNationalId = p.PreservedNationalId,
                MobileNumber = p.MobileNumber,
                LastModifiedAtUtc = p.LastModifiedAtUtc
            }).ToList();
        }

        if (filter is null or "" or "propertyunit")
        {
            var (units, unitTotal) = await _unitOfWork.PropertyUnits
                .GetPendingIdentifierReconciliationAsync(page, pageSize, cancellationToken);

            response.PropertyUnitsTotalCount = unitTotal;
            response.PropertyUnits = units.Select(u => new PropertyUnitIdentifierReconciliationDto
            {
                PropertyUnitId = u.Id,
                BuildingId = u.BuildingId,
                BuildingCode = u.Building?.BuildingId,
                CurrentUnitIdentifier = u.UnitIdentifier,
                OriginalUnitIdentifier = u.OriginalUnitIdentifier,
                LastModifiedAtUtc = u.LastModifiedAtUtc
            }).ToList();
        }

        return response;
    }
}
