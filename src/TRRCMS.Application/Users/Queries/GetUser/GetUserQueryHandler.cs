using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Queries.GetUser
{
    /// <summary>
    /// Handles the GetUserQuery to fetch user details.
    /// </summary>
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDetailDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDetailDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
                throw new NotFoundException($"User with ID '{request.UserId}' was not found.");

            return _mapper.Map<UserDetailDto>(user);
        }
    }
}
