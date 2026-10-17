using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Common.Security;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Application.Users.Commands.ResetUserPassword;

/// <summary>
/// Validator for ResetUserPasswordCommand.
/// An administrator-chosen temporary password must satisfy the active password policy.
/// </summary>
public class ResetUserPasswordCommandValidator : LocalizedValidator<ResetUserPasswordCommand>
{
    private readonly ISecurityPolicyRepository _securityPolicyRepository;

    public ResetUserPasswordCommandValidator(
        IStringLocalizer<ValidationMessages> localizer,
        ISecurityPolicyRepository securityPolicyRepository) : base(localizer)
    {
        _securityPolicyRepository = securityPolicyRepository;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(L("UserId_Required"));

        RuleFor(x => x.TemporaryPassword!)
            .CustomAsync(ValidateAgainstActivePasswordPolicyAsync)
            .When(x => !string.IsNullOrEmpty(x.TemporaryPassword));
    }

    private async Task ValidateAgainstActivePasswordPolicyAsync(
        string password,
        ValidationContext<ResetUserPasswordCommand> ctx,
        CancellationToken cancellationToken)
    {
        var active = await _securityPolicyRepository.GetActiveAsync(cancellationToken);
        var policy = active?.PasswordPolicy ?? PasswordPolicy.Default();

        foreach (var (key, args) in PasswordPolicyRules.GetViolations(password, policy))
            ctx.AddFailure(nameof(ResetUserPasswordCommand.TemporaryPassword), L(key, args));
    }
}
