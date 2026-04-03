using MediatR;
using TRRCMS.Application.Cases.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Cases.Queries.GetCase;

public class GetCaseQueryHandler : IRequestHandler<GetCaseQuery, CaseDto>
{
    private readonly ICaseRepository _caseRepository;

    public GetCaseQueryHandler(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository ?? throw new ArgumentNullException(nameof(caseRepository));
    }

    public async Task<CaseDto> Handle(GetCaseQuery request, CancellationToken cancellationToken)
    {
        var caseEntity = await _caseRepository.GetByIdAsync(request.CaseId, cancellationToken)
            ?? throw new NotFoundException($"Case with ID {request.CaseId} not found");

        return new CaseDto
        {
            Id = caseEntity.Id,
            CaseNumber = caseEntity.CaseNumber,
            PropertyUnitId = caseEntity.PropertyUnitId,
            Status = (int)caseEntity.Status,
            StatusName = caseEntity.Status.ToString(),
            OpenedDate = caseEntity.OpenedDate,
            ClosedDate = caseEntity.ClosedDate,
            ClosedByClaimId = caseEntity.ClosedByClaimId,
            IsEditable = caseEntity.IsEditable,
            Notes = caseEntity.Notes,
            SurveyCount = caseEntity.Surveys.Count,
            ClaimCount = caseEntity.Claims.Count,
            PersonPropertyRelationCount = caseEntity.PersonPropertyRelations.Count,
            SurveyIds = caseEntity.Surveys.Select(s => s.Id).ToList(),
            ClaimIds = caseEntity.Claims.Select(c => c.Id).ToList(),
            CreatedAtUtc = caseEntity.CreatedAtUtc,
            LastModifiedAtUtc = caseEntity.LastModifiedAtUtc
        };
    }
}
