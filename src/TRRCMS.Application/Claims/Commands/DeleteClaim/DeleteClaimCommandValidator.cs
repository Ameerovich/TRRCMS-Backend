using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Claims.Commands.DeleteClaim;

public class DeleteClaimCommandValidator : LocalizedValidator<DeleteClaimCommand>
{
    public DeleteClaimCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.ClaimId)
            .NotEmpty()
            .WithMessage(L("ClaimId_Required"));
    }
}
