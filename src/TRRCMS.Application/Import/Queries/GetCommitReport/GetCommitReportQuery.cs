using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Queries.GetCommitReport;

/// <summary>
/// Query to retrieve the commit report for an import package.
/// Returns the detailed report generated during the commit process.
///
/// The report is reconstructed from the ImportPackage entity and its
/// staging data traceability links (CommittedEntityId mappings).
///
/// UC-003 Stage 4 — S11 (Archive / Review Results).
/// </summary>
public class GetCommitReportQuery : IRequest<CommitReportDto>
{
    /// <summary>ImportPackage.Id (surrogate key).</summary>
    public Guid ImportPackageId { get; set; }
}
