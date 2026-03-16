using MediatR;
using TRRCMS.Application.SecuritySettings.Dtos;

namespace TRRCMS.Application.SecuritySettings.Queries.GetCurrentSecuritySettings;

/// <summary>
/// Query to retrieve the currently active (enforced) security policy.
/// </summary>
public class GetCurrentSecuritySettingsQuery : IRequest<SecurityPolicyDto>
{
}
