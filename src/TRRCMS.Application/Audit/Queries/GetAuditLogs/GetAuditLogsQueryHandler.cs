using AutoMapper;
using MediatR;
using TRRCMS.Application.Audit.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Audit.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, List<AuditLogDetailDto>>
{
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public GetAuditLogsQueryHandler(IAuditService auditService, IMapper mapper)
    {
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<List<AuditLogDetailDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var pageNumber = Math.Max(request.PageNumber, 1);

        var actionTypeFilter = request.SecurityOnly == true
            ? null  // we filter in memory below
            : request.ActionType;

        var logs = await _auditService.GetAuditLogsAsync(
            fromDate: request.FromDate,
            toDate: request.ToDate,
            userId: request.UserId,
            entityType: request.EntityType,
            actionType: actionTypeFilter,
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        if (request.SecurityOnly == true)
            logs = logs.Where(l => l.IsSecuritySensitive).ToList();

        return _mapper.Map<List<AuditLogDetailDto>>(logs);
    }
}
