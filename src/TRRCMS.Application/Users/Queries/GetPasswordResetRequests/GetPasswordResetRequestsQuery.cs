using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Queries.GetPasswordResetRequests;

/// <summary>
/// Pending "forgot password" requests, oldest first
/// </summary>
public record GetPasswordResetRequestsQuery : IRequest<List<PasswordResetRequestDto>>;

public class GetPasswordResetRequestsQueryHandler
    : IRequestHandler<GetPasswordResetRequestsQuery, List<PasswordResetRequestDto>>
{
    private readonly IUserRepository _userRepository;

    public GetPasswordResetRequestsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<PasswordResetRequestDto>> Handle(
        GetPasswordResetRequestsQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetWithPendingPasswordResetRequestAsync(cancellationToken);

        return users.Select(u => new PasswordResetRequestDto
        {
            UserId = u.Id,
            Username = u.Username,
            FullNameArabic = u.FullNameArabic,
            FullNameEnglish = u.FullNameEnglish,
            Role = (int)u.Role,
            RoleName = u.Role.ToString(),
            PhoneNumber = u.PhoneNumber,
            Email = u.Email,
            IsActive = u.IsActive,
            IsLockedOut = u.IsLockedOut,
            RequestedAtUtc = u.PasswordResetRequestedAtUtc!.Value,
            Note = u.PasswordResetRequestNote
        }).ToList();
    }
}
