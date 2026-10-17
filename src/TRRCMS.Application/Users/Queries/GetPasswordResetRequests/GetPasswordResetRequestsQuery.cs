using MediatR;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Users.Queries.GetPasswordResetRequests;

/// <summary>
/// Pending "forgot password" requests, oldest first
/// </summary>
public record GetPasswordResetRequestsQuery : IRequest<List<PasswordResetRequestDto>>;
