using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.SecuritySettings.Dtos;

namespace TRRCMS.Application.SecuritySettings.Queries.GetCurrentSecuritySettings;

/// <summary>
/// Handler for GetCurrentSecuritySettingsQuery.
/// Returns the currently enforced security policy.
/// If no policy exists (fresh install before seeding), returns the default.
/// </summary>
public class GetCurrentSecuritySettingsQueryHandler
    : IRequestHandler<GetCurrentSecuritySettingsQuery, SecurityPolicyDto>
{
    private readonly ISecurityPolicyRepository _repository;

    public GetCurrentSecuritySettingsQueryHandler(ISecurityPolicyRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<SecurityPolicyDto> Handle(
        GetCurrentSecuritySettingsQuery request,
        CancellationToken cancellationToken)
    {
        var activePolicy = await _repository.GetActiveAsync(cancellationToken);

        if (activePolicy is null)
        {
            throw new NotFoundException(
                "No active security policy found. The system may need to be seeded with a default policy.");
        }

        return new SecurityPolicyDto
        {
            Id = activePolicy.Id,
            Version = activePolicy.Version,
            IsActive = activePolicy.IsActive,
            EffectiveFromUtc = activePolicy.EffectiveFromUtc,
            EffectiveToUtc = activePolicy.EffectiveToUtc,
            ChangeDescription = activePolicy.ChangeDescription,
            AppliedByUserId = activePolicy.AppliedByUserId,
            CreatedAtUtc = activePolicy.CreatedAtUtc,
            CreatedBy = activePolicy.CreatedBy,
            PasswordPolicy = new PasswordPolicyDto
            {
                MinLength = activePolicy.PasswordPolicy.MinLength,
                RequireUppercase = activePolicy.PasswordPolicy.RequireUppercase,
                RequireLowercase = activePolicy.PasswordPolicy.RequireLowercase,
                RequireDigit = activePolicy.PasswordPolicy.RequireDigit,
                RequireSpecialCharacter = activePolicy.PasswordPolicy.RequireSpecialCharacter,
                ExpiryDays = activePolicy.PasswordPolicy.ExpiryDays,
                ReuseHistory = activePolicy.PasswordPolicy.ReuseHistory
            },
            SessionLockoutPolicy = new SessionLockoutPolicyDto
            {
                SessionTimeoutMinutes = activePolicy.SessionLockoutPolicy.SessionTimeoutMinutes,
                MaxFailedLoginAttempts = activePolicy.SessionLockoutPolicy.MaxFailedLoginAttempts,
                LockoutDurationMinutes = activePolicy.SessionLockoutPolicy.LockoutDurationMinutes
            },
            AccessControlPolicy = new AccessControlPolicyDto
            {
                AllowPasswordAuthentication = activePolicy.AccessControlPolicy.AllowPasswordAuthentication,
                AllowSsoAuthentication = activePolicy.AccessControlPolicy.AllowSsoAuthentication,
                AllowTokenAuthentication = activePolicy.AccessControlPolicy.AllowTokenAuthentication,
                EnforceIpAllowlist = activePolicy.AccessControlPolicy.EnforceIpAllowlist,
                IpAllowlist = activePolicy.AccessControlPolicy.IpAllowlist,
                IpDenylist = activePolicy.AccessControlPolicy.IpDenylist,
                RestrictByEnvironment = activePolicy.AccessControlPolicy.RestrictByEnvironment,
                AllowedEnvironments = activePolicy.AccessControlPolicy.AllowedEnvironments
            }
        };
    }
}
