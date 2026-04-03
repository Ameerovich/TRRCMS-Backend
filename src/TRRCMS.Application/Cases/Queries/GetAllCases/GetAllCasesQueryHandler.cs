using MediatR;
using TRRCMS.Application.Cases.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Cases.Queries.GetAllCases;

public class GetAllCasesQueryHandler : IRequestHandler<GetAllCasesQuery, ListResponse<CaseSummaryDto>>
{
    private readonly ICaseRepository _caseRepository;

    public GetAllCasesQueryHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public async Task<ListResponse<CaseSummaryDto>> Handle(GetAllCasesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = PagedQuery.ClampPageSize(request.PageSize);

        var (items, totalCount) = await _caseRepository.GetAllAsync(
            status: request.Status,
            buildingId: request.BuildingId,
            buildingCode: request.BuildingCode,
            unitIdentifier: request.UnitIdentifier,
            page: request.Page,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var dtos = items.Select(c => new CaseSummaryDto
        {
            Id = c.Id,
            CaseNumber = c.CaseNumber,
            PropertyUnitId = c.PropertyUnitId,
            Status = (int)c.Status,
            StatusName = c.Status.ToString(),
            OpenedDate = c.OpenedDate,
            ClosedDate = c.ClosedDate,
            IsEditable = c.IsEditable,
            SurveyCount = c.Surveys.Count,
            ClaimCount = c.Claims.Count,
            CreatedAtUtc = c.CreatedAtUtc
        }).ToList();

        return new ListResponse<CaseSummaryDto>
        {
            Items = dtos,
            TotalCount = totalCount
        };
    }
}
