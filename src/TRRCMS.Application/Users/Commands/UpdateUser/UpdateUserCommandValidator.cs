using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Users.Commands.UpdateUser;

/// <summary>
/// Validator for UpdateUserCommand
/// </summary>
public class UpdateUserCommandValidator : LocalizedValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(L("UserId_Required"));

        RuleFor(x => x.FullNameArabic)
            .MaximumLength(200).WithMessage(L("FullNameAr_MaxLength200"))
            .When(x => !string.IsNullOrWhiteSpace(x.FullNameArabic));

        RuleFor(x => x.FullNameEnglish)
            .MaximumLength(200).WithMessage(L("FullNameEn_MaxLength200"))
            .When(x => !string.IsNullOrWhiteSpace(x.FullNameEnglish));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage(L("Email_InvalidFormat"))
            .MaximumLength(100).WithMessage(L("Email_MaxLength100"))
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+963|0)\d{7,9}$")
            .WithMessage(L("Phone_SyrianFormat"))
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage(L("UserRole_Invalid"))
            .When(x => x.Role.HasValue);

        RuleFor(x => x.Organization)
            .MaximumLength(100).WithMessage(L("Organization_MaxLength100"))
            .When(x => !string.IsNullOrWhiteSpace(x.Organization));

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage(L("JobTitle_MaxLength100"))
            .When(x => !string.IsNullOrWhiteSpace(x.JobTitle));

        RuleFor(x => x.EmployeeId)
            .MaximumLength(50).WithMessage(L("EmployeeId_MaxLength50"))
            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeId));
    }
}
