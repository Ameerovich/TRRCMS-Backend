using MediatR;
using TRRCMS.Application.SecuritySettings.Dtos;

namespace TRRCMS.Application.SecuritySettings.Queries.GetSecuritySettingsHistory;

/// <summary>
/// Query to retrieve the full version history of security policy changes.
/// Returns all versions ordered newest first.
/// </summary>
public class GetSecuritySettingsHistoryQuery : IRequest<List<SecurityPolicyDto>>
{
}
