using AutoMapper;
using MediatR;
using TRRCMS.Application.Audit;
using TRRCMS.Application.Audit.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Audit.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDetailDto>>
{
    private const int MaxPageSize = 200;

    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public GetAuditLogsQueryHandler(IAuditService auditService, IMapper mapper)
    {
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<PagedResult<AuditLogDetailDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate > request.ToDate)
            throw new ValidationException("fromDate must be earlier than or equal to toDate.");

        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var pageNumber = Math.Max(request.PageNumber, 1);

        // SecurityOnly is a convenience flag for the /audit/security endpoint.
        // If both are provided, IsSecuritySensitive wins (explicit beats convenience).
        var sensitivity = request.IsSecuritySensitive
            ?? (request.SecurityOnly == true ? true : (bool?)null);

        var (items, totalCount) = await _auditService.QueryAuditLogsAsync(
            fromDate: request.FromDate,
            toDate: request.ToDate,
            userId: request.UserId,
            usernameContains: request.UsernameContains,
            entityType: request.EntityType,
            actionTypes: request.ActionTypes,
            actionResult: request.ActionResult,
            isSecuritySensitive: sensitivity,
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var dtos = _mapper.Map<List<AuditLogDetailDto>>(items);
        foreach (var dto in dtos)
            dto.Changes = AuditChangeBuilder.Build(dto.OldValues, dto.NewValues, dto.ChangedFields);

        // Bypass PaginatedList.Create here: it clamps to MaxPageSize=100, but the
        // audit feed has its own contract of up to 200 (kept consistent with the
        // existing controller signature).
        return new PagedResult<AuditLogDetailDto>(dtos, totalCount, pageNumber, pageSize);
    }
}
