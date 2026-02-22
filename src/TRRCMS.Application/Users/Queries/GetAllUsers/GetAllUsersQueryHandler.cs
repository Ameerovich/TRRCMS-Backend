using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Queries.GetAllUsers;

/// <summary>
/// Handler for fetching all users with optional filters and pagination.
/// </summary>
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserListDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserListDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        // 1) Load data (repository returns List<User>, so filtering is in-memory)
        List<TRRCMS.Domain.Entities.User> users;

        // Role filter first (preferred path)
        if (request.Role.HasValue)
        {
            var activeOnly = request.IsActive == true;
            users = await _userRepository.GetUsersByRoleAsync(
                role: request.Role.Value,
                activeOnly: activeOnly,
                cancellationToken: cancellationToken);
        }
        else
        {
            users = await _userRepository.GetAllAsync(cancellationToken);
        }

        // 2) Apply IsActive filter
        if (request.IsActive.HasValue)
        {
            users = users.Where(u => u.IsActive == request.IsActive.Value).ToList();
        }

        // 3) Search (username/email/full name)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();

            users = users.Where(u =>
                    (!string.IsNullOrWhiteSpace(u.Username) && u.Username.Contains(term)) ||
                    (!string.IsNullOrWhiteSpace(u.Email) && u.Email.Contains(term)) ||
                    (!string.IsNullOrWhiteSpace(u.FullNameArabic) && u.FullNameArabic.Contains(term)) ||
                    (!string.IsNullOrWhiteSpace(u.FullNameEnglish) && u.FullNameEnglish.Contains(term))
                )
                .ToList();
        }

        // 4) Map and paginate
        var userDtos = _mapper.Map<List<UserListDto>>(users);
        return PaginatedList.FromEnumerable(userDtos, request.PageNumber, request.PageSize);
    }
}
