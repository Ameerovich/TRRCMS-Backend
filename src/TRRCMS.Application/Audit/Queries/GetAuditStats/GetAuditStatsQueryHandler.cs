using MediatR;
using TRRCMS.Application.Audit.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Audit.Queries.GetAuditStats;

public class GetAuditStatsQueryHandler : IRequestHandler<GetAuditStatsQuery, AuditStatsDto>
{
    private readonly IAuditService _auditService;

    public GetAuditStatsQueryHandler(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public Task<AuditStatsDto> Handle(GetAuditStatsQuery request, CancellationToken cancellationToken)
    {
        if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate > request.ToDate)
            throw new ValidationException("fromDate must be earlier than or equal to toDate.");

        return _auditService.GetAuditStatsAsync(
            fromDate: request.FromDate,
            toDate: request.ToDate,
            topUsersLimit: request.TopUsersLimit <= 0 ? 5 : request.TopUsersLimit,
            cancellationToken: cancellationToken);
    }
}
