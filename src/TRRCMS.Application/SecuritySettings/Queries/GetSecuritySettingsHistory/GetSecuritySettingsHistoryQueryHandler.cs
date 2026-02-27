using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.SecuritySettings.Dtos;

namespace TRRCMS.Application.SecuritySettings.Queries.GetSecuritySettingsHistory;

/// <summary>
/// Handler for GetSecuritySettingsHistoryQuery.
/// Returns all security policy versions ordered by version descending (newest first).
/// Each entry shows the full configuration snapshot at that point in time.
/// </summary>
public class GetSecuritySettingsHistoryQueryHandler
    : IRequestHandler<GetSecuritySettingsHistoryQuery, List<SecurityPolicyDto>>
{
    private readonly ISecurityPolicyRepository _repository;

    public GetSecuritySettingsHistoryQueryHandler(ISecurityPolicyRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<List<SecurityPolicyDto>> Handle(
        GetSecuritySettingsHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var history = await _repository.GetVersionHistoryAsync(cancellationToken);

        return history.Select(policy => new SecurityPolicyDto
        {
            Id = policy.Id,
            Version = policy.Version,
            IsActive = policy.IsActive,
            EffectiveFromUtc = policy.EffectiveFromUtc,
            EffectiveToUtc = policy.EffectiveToUtc,
            ChangeDescription = policy.ChangeDescription,
            AppliedByUserId = policy.AppliedByUserId,
            CreatedAtUtc = policy.CreatedAtUtc,
            CreatedBy = policy.CreatedBy,
            PasswordPolicy = new PasswordPolicyDto
            {
                MinLength = policy.PasswordPolicy.MinLength,
                RequireUppercase = policy.PasswordPolicy.RequireUppercase,
                RequireLowercase = policy.PasswordPolicy.RequireLowercase,
                RequireDigit = policy.PasswordPolicy.RequireDigit,
                RequireSpecialCharacter = policy.PasswordPolicy.RequireSpecialCharacter,
                ExpiryDays = policy.PasswordPolicy.ExpiryDays,
                ReuseHistory = policy.PasswordPolicy.ReuseHistory
            },
            SessionLockoutPolicy = new SessionLockoutPolicyDto
            {
                SessionTimeoutMinutes = policy.SessionLockoutPolicy.SessionTimeoutMinutes,
                MaxFailedLoginAttempts = policy.SessionLockoutPolicy.MaxFailedLoginAttempts,
                LockoutDurationMinutes = policy.SessionLockoutPolicy.LockoutDurationMinutes
            },
            AccessControlPolicy = new AccessControlPolicyDto
            {
                AllowPasswordAuthentication = policy.AccessControlPolicy.AllowPasswordAuthentication,
                AllowSsoAuthentication = policy.AccessControlPolicy.AllowSsoAuthentication,
                AllowTokenAuthentication = policy.AccessControlPolicy.AllowTokenAuthentication,
                EnforceIpAllowlist = policy.AccessControlPolicy.EnforceIpAllowlist,
                IpAllowlist = policy.AccessControlPolicy.IpAllowlist,
                IpDenylist = policy.AccessControlPolicy.IpDenylist,
                RestrictByEnvironment = policy.AccessControlPolicy.RestrictByEnvironment,
                AllowedEnvironments = policy.AccessControlPolicy.AllowedEnvironments
            }
        }).ToList();
    }
}
