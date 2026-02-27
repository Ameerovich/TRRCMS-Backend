using MediatR;
using TRRCMS.Application.SecuritySettings.Dtos;

namespace TRRCMS.Application.SecuritySettings.Queries.GetSecuritySettingsHistory;

/// <summary>
/// Query to retrieve the full version history of security policy changes.
/// Supports FSD 13.4: Legal Audit Trail — complete change history for security configurations.
/// Returns all versions ordered newest first.
/// </summary>
public class GetSecuritySettingsHistoryQuery : IRequest<List<SecurityPolicyDto>>
{
}
