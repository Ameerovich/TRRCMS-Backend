using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Queries.GetUserAuditLog
{
    /// <summary>
    /// Handler for retrieving recent audit activity for a specific user.
    /// </summary>
    public class GetUserAuditLogQueryHandler : IRequestHandler<GetUserAuditLogQuery, List<AuditLogDto>>
    {
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;

        public GetUserAuditLogQueryHandler(IAuditService auditService, IMapper mapper)
        {
            _auditService = auditService;
            _mapper = mapper;
        }

        public async Task<List<AuditLogDto>> Handle(GetUserAuditLogQuery request, CancellationToken cancellationToken)
        {
            var count = request.Count <= 0 ? 50 : request.Count;
            if (count > 500) count = 500;

            var logs = await _auditService.GetUserRecentActivityAsync(
                userId: request.UserId,
                count: count,
                cancellationToken: cancellationToken);

            return _mapper.Map<List<AuditLogDto>>(logs);
        }
    }
}
