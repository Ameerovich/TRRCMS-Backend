using FluentValidation;
using TRRCMS.Application;
using TRRCMS.Application.Common.Localization;
using Microsoft.Extensions.Localization;

namespace TRRCMS.Application.BuildingAssignments.Commands.ResetAssignmentToPending;

public class ResetAssignmentToPendingCommandValidator : LocalizedValidator<ResetAssignmentToPendingCommand>
{
    public ResetAssignmentToPendingCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.AssignmentId)
            .NotEmpty()
            .WithMessage(L("AssignmentId_Required"));

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => x.Reason != null)
            .WithMessage(L("CancellationReason_Assignment_MaxLength1000"));
    }
}
