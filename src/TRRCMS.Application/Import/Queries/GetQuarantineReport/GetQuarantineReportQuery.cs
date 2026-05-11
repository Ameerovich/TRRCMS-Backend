using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Queries.GetQuarantineReport;

public record GetQuarantineReportQuery : IRequest<QuarantineReportDto>
{
    public Guid ImportPackageId { get; init; }
}
