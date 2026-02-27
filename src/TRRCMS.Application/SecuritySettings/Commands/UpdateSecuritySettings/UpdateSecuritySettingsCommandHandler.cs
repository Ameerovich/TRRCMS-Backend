using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.SecuritySettings.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Application.SecuritySettings.Commands.UpdateSecuritySettings;

/// <summary>
/// Handler for UpdateSecuritySettingsCommand.
/// UC-011 S06–S08: Validate, apply, and enforce security policy.
///
/// Transactional flow:
///   1. Validate combined policy (done by FluentValidation pipeline + domain invariants).
///   2. Deactivate the current active policy.
///   3. Create a new versioned SecurityPolicy record as active.
///   4. Commit both changes atomically via IUnitOfWork.
///   5. Log the administrative action to the audit trail.
/// </summary>
public class UpdateSecuritySettingsCommandHandler : IRequestHandler<UpdateSecuritySettingsCommand, SecurityPolicyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityPolicyRepository _securityPolicyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdateSecuritySettingsCommandHandler(
        IUnitOfWork unitOfWork,
        ISecurityPolicyRepository securityPolicyRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _securityPolicyRepository = securityPolicyRepository ?? throw new ArgumentNullException(nameof(securityPolicyRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<SecurityPolicyDto> Handle(
        UpdateSecuritySettingsCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        // Build value objects (domain-level invariant validation happens here)
        PasswordPolicy passwordPolicy;
        SessionLockoutPolicy sessionLockoutPolicy;
        AccessControlPolicy accessControlPolicy;

        try
        {
            passwordPolicy = PasswordPolicy.Create(
                request.PasswordMinLength,
                request.PasswordRequireUppercase,
                request.PasswordRequireLowercase,
                request.PasswordRequireDigit,
                request.PasswordRequireSpecialCharacter,
                request.PasswordExpiryDays,
                request.PasswordReuseHistory);

            sessionLockoutPolicy = SessionLockoutPolicy.Create(
                request.SessionTimeoutMinutes,
                request.MaxFailedLoginAttempts,
                request.LockoutDurationMinutes);

            accessControlPolicy = AccessControlPolicy.Create(
                request.AllowPasswordAuthentication,
                request.AllowSsoAuthentication,
                request.AllowTokenAuthentication,
                request.EnforceIpAllowlist,
                request.IpAllowlist,
                request.IpDenylist,
                request.RestrictByEnvironment,
                request.AllowedEnvironments);
        }
        catch (ArgumentException ex)
        {
            throw new ValidationException(ex.Message);
        }

        // Execute atomically: deactivate old + create new in a single transaction
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Get current active policy (may be null on first setup)
            var currentPolicy = await _securityPolicyRepository.GetActiveAsync(cancellationToken);
            var nextVersion = (await _securityPolicyRepository.GetLatestVersionNumberAsync(cancellationToken)) + 1;

            // Capture old values for audit (before deactivation)
            string? oldValues = null;
            if (currentPolicy is not null)
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(MapToDto(currentPolicy));
                currentPolicy.Deactivate(currentUserId);
                await _securityPolicyRepository.UpdateAsync(currentPolicy, cancellationToken);
            }

            // Create new versioned policy (UC-011 S07: Apply)
            SecurityPolicy newPolicy;
            try
            {
                newPolicy = SecurityPolicy.CreateNewVersion(
                    nextVersion,
                    passwordPolicy,
                    sessionLockoutPolicy,
                    accessControlPolicy,
                    request.ChangeDescription,
                    currentUserId);
            }
            catch (InvalidOperationException ex)
            {
                // UC-011 S06a: Cross-cutting validation failure
                throw new ValidationException(ex.Message);
            }

            await _securityPolicyRepository.AddAsync(newPolicy, cancellationToken);

            // Commit both deactivation and creation atomically
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // UC-011 S08: Log administrative action
            var resultDto = MapToDto(newPolicy);
            await _auditService.LogSecurityActionAsync(
                actionType: AuditActionType.ConfigurationChange,
                actionDescription: $"Security policy updated to v{newPolicy.Version}. {request.ChangeDescription ?? "No description provided."}",
                isSecuritySensitive: true,
                cancellationToken: cancellationToken);

            await _auditService.LogActionAsync(
                actionType: AuditActionType.ConfigurationChange,
                actionDescription: $"Applied security policy v{newPolicy.Version}",
                entityType: "SecurityPolicy",
                entityId: newPolicy.Id,
                entityIdentifier: $"v{newPolicy.Version}",
                oldValues: oldValues,
                newValues: System.Text.Json.JsonSerializer.Serialize(resultDto),
                changedFields: BuildChangedFields(currentPolicy, newPolicy),
                cancellationToken: cancellationToken);

            return resultDto;
        }, cancellationToken);
    }

    // ==================== PRIVATE HELPERS ====================

    private static SecurityPolicyDto MapToDto(SecurityPolicy entity)
    {
        return new SecurityPolicyDto
        {
            Id = entity.Id,
            Version = entity.Version,
            IsActive = entity.IsActive,
            EffectiveFromUtc = entity.EffectiveFromUtc,
            EffectiveToUtc = entity.EffectiveToUtc,
            ChangeDescription = entity.ChangeDescription,
            AppliedByUserId = entity.AppliedByUserId,
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            PasswordPolicy = new PasswordPolicyDto
            {
                MinLength = entity.PasswordPolicy.MinLength,
                RequireUppercase = entity.PasswordPolicy.RequireUppercase,
                RequireLowercase = entity.PasswordPolicy.RequireLowercase,
                RequireDigit = entity.PasswordPolicy.RequireDigit,
                RequireSpecialCharacter = entity.PasswordPolicy.RequireSpecialCharacter,
                ExpiryDays = entity.PasswordPolicy.ExpiryDays,
                ReuseHistory = entity.PasswordPolicy.ReuseHistory
            },
            SessionLockoutPolicy = new SessionLockoutPolicyDto
            {
                SessionTimeoutMinutes = entity.SessionLockoutPolicy.SessionTimeoutMinutes,
                MaxFailedLoginAttempts = entity.SessionLockoutPolicy.MaxFailedLoginAttempts,
                LockoutDurationMinutes = entity.SessionLockoutPolicy.LockoutDurationMinutes
            },
            AccessControlPolicy = new AccessControlPolicyDto
            {
                AllowPasswordAuthentication = entity.AccessControlPolicy.AllowPasswordAuthentication,
                AllowSsoAuthentication = entity.AccessControlPolicy.AllowSsoAuthentication,
                AllowTokenAuthentication = entity.AccessControlPolicy.AllowTokenAuthentication,
                EnforceIpAllowlist = entity.AccessControlPolicy.EnforceIpAllowlist,
                IpAllowlist = entity.AccessControlPolicy.IpAllowlist,
                IpDenylist = entity.AccessControlPolicy.IpDenylist,
                RestrictByEnvironment = entity.AccessControlPolicy.RestrictByEnvironment,
                AllowedEnvironments = entity.AccessControlPolicy.AllowedEnvironments
            }
        };
    }

    private static string BuildChangedFields(SecurityPolicy? previous, SecurityPolicy current)
    {
        if (previous is null)
            return "Initial policy creation";

        var changes = new List<string>();

        // Password policy changes
        var pp = previous.PasswordPolicy;
        var cp = current.PasswordPolicy;
        if (pp.MinLength != cp.MinLength) changes.Add($"PasswordMinLength: {pp.MinLength}→{cp.MinLength}");
        if (pp.RequireUppercase != cp.RequireUppercase) changes.Add($"RequireUppercase: {pp.RequireUppercase}→{cp.RequireUppercase}");
        if (pp.RequireLowercase != cp.RequireLowercase) changes.Add($"RequireLowercase: {pp.RequireLowercase}→{cp.RequireLowercase}");
        if (pp.RequireDigit != cp.RequireDigit) changes.Add($"RequireDigit: {pp.RequireDigit}→{cp.RequireDigit}");
        if (pp.RequireSpecialCharacter != cp.RequireSpecialCharacter) changes.Add($"RequireSpecialChar: {pp.RequireSpecialCharacter}→{cp.RequireSpecialCharacter}");
        if (pp.ExpiryDays != cp.ExpiryDays) changes.Add($"PasswordExpiryDays: {pp.ExpiryDays}→{cp.ExpiryDays}");
        if (pp.ReuseHistory != cp.ReuseHistory) changes.Add($"PasswordReuseHistory: {pp.ReuseHistory}→{cp.ReuseHistory}");

        // Session/lockout changes
        var ps = previous.SessionLockoutPolicy;
        var cs = current.SessionLockoutPolicy;
        if (ps.SessionTimeoutMinutes != cs.SessionTimeoutMinutes) changes.Add($"SessionTimeout: {ps.SessionTimeoutMinutes}→{cs.SessionTimeoutMinutes}");
        if (ps.MaxFailedLoginAttempts != cs.MaxFailedLoginAttempts) changes.Add($"MaxFailedLogins: {ps.MaxFailedLoginAttempts}→{cs.MaxFailedLoginAttempts}");
        if (ps.LockoutDurationMinutes != cs.LockoutDurationMinutes) changes.Add($"LockoutDuration: {ps.LockoutDurationMinutes}→{cs.LockoutDurationMinutes}");

        // Access control changes
        var pa = previous.AccessControlPolicy;
        var ca = current.AccessControlPolicy;
        if (pa.AllowPasswordAuthentication != ca.AllowPasswordAuthentication) changes.Add($"PasswordAuth: {pa.AllowPasswordAuthentication}→{ca.AllowPasswordAuthentication}");
        if (pa.AllowSsoAuthentication != ca.AllowSsoAuthentication) changes.Add($"SsoAuth: {pa.AllowSsoAuthentication}→{ca.AllowSsoAuthentication}");
        if (pa.AllowTokenAuthentication != ca.AllowTokenAuthentication) changes.Add($"TokenAuth: {pa.AllowTokenAuthentication}→{ca.AllowTokenAuthentication}");
        if (pa.EnforceIpAllowlist != ca.EnforceIpAllowlist) changes.Add($"EnforceIpAllowlist: {pa.EnforceIpAllowlist}→{ca.EnforceIpAllowlist}");
        if (pa.IpAllowlist != ca.IpAllowlist) changes.Add("IpAllowlist");
        if (pa.IpDenylist != ca.IpDenylist) changes.Add("IpDenylist");
        if (pa.RestrictByEnvironment != ca.RestrictByEnvironment) changes.Add($"RestrictByEnv: {pa.RestrictByEnvironment}→{ca.RestrictByEnvironment}");

        return changes.Count > 0 ? string.Join(", ", changes) : "No field changes detected";
    }
}
