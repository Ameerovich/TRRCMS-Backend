using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Dashboard.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Dashboard.Queries.GetRegistrationCoverage;

public sealed class GetRegistrationCoverageQueryHandler
    : IRequestHandler<GetRegistrationCoverageQuery, RegistrationCoverageDashboardDto>
{
    private readonly IUnitOfWork _uow;

    public GetRegistrationCoverageQueryHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<RegistrationCoverageDashboardDto> Handle(
        GetRegistrationCoverageQuery request,
        CancellationToken cancellationToken)
    {
        var totalPersons = await _uow.Persons.GetTotalCountAsync(cancellationToken);
        var totalHouseholds = await _uow.Households.GetTotalCountAsync(cancellationToken);
        var genderCounts = await _uow.Persons.GetGenderCountsAsync(cancellationToken);
        var withNationalId = await _uow.Persons.GetCountWithNationalIdAsync(cancellationToken);

        var totalRelations = await _uow.PersonPropertyRelations.GetTotalCountAsync(cancellationToken);
        var relationTypeCounts = await _uow.PersonPropertyRelations.GetRelationTypeCountsAsync(cancellationToken);
        var relationsWithEvidence = await _uow.PersonPropertyRelations.GetCountWithEvidenceAsync(cancellationToken);

        var caseStatusCounts = await _uow.Claims.GetCaseStatusCountsAsync(cancellationToken);
        var claimTypeCounts = await _uow.Claims.GetClaimTypeCountsAsync(cancellationToken);

        var totalEvidence = await _uow.Evidences.GetTotalCountAsync(cancellationToken);

        return new RegistrationCoverageDashboardDto
        {
            TotalPersons = totalPersons,
            TotalHouseholds = totalHouseholds,
            PersonsByGender = genderCounts.ToDictionary(
                kvp => kvp.Key.ToString(), kvp => kvp.Value),
            PersonsWithNationalId = withNationalId,
            TotalPersonPropertyRelations = totalRelations,
            RelationsByType = relationTypeCounts.ToDictionary(
                kvp => kvp.Key.ToString(), kvp => kvp.Value),
            RelationsWithEvidence = relationsWithEvidence,
            ClaimsOpen = caseStatusCounts.GetValueOrDefault(CaseStatus.Open, 0),
            ClaimsClosed = caseStatusCounts.GetValueOrDefault(CaseStatus.Closed, 0),
            ClaimsByType = claimTypeCounts.ToDictionary(
                kvp => kvp.Key.ToString(), kvp => kvp.Value),
            TotalEvidenceItems = totalEvidence,
            GeneratedAtUtc = DateTime.UtcNow
        };
    }
}
