using AutoMapper;
using MediatR;
using TRRCMS.Application.Audit.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Audit.Queries.GetEntityAuditHistory;

public class GetEntityAuditHistoryQueryHandler : IRequestHandler<GetEntityAuditHistoryQuery, List<AuditLogDetailDto>>
{
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public GetEntityAuditHistoryQueryHandler(IAuditService auditService, IMapper mapper)
    {
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<List<AuditLogDetailDto>> Handle(GetEntityAuditHistoryQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EntityType))
            throw new ValidationException("EntityType is required.");

        var logs = await _auditService.GetEntityHistoryAsync(
            entityType: request.EntityType,
            entityId: request.EntityId,
            cancellationToken: cancellationToken);

        return _mapper.Map<List<AuditLogDetailDto>>(logs);
    }
}
