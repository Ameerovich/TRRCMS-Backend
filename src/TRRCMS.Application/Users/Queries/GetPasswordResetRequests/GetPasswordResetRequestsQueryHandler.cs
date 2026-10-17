using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Queries.GetPasswordResetRequests;

public class GetPasswordResetRequestsQueryHandler
    : IRequestHandler<GetPasswordResetRequestsQuery, List<PasswordResetRequestDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetPasswordResetRequestsQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<List<PasswordResetRequestDto>> Handle(
        GetPasswordResetRequestsQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetWithPendingPasswordResetRequestAsync(cancellationToken);
        return _mapper.Map<List<PasswordResetRequestDto>>(users);
    }
}
