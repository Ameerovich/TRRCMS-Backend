using MediatR;
using TRRCMS.Application.SecuritySettings.Dtos;

namespace TRRCMS.Application.SecuritySettings.Queries.GetCurrentSecuritySettings;

/// <summary>
/// Query to retrieve the currently active (enforced) security policy.
/// UC-011 S02: Select Security Settings → Load current security policy configuration.
/// </summary>
public class GetCurrentSecuritySettingsQuery : IRequest<SecurityPolicyDto>
{
}
