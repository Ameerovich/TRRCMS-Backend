using FluentValidation;

namespace TRRCMS.Application.BuildingAssignments.Commands.CheckTransferTimeout;

public sealed class CheckTransferTimeoutCommandValidator
    : AbstractValidator<CheckTransferTimeoutCommand>
{
    public CheckTransferTimeoutCommandValidator()
    {
        RuleFor(x => x.TimeoutMinutes)
            .GreaterThan(0)
            .WithMessage("Timeout must be greater than 0 minutes.")
            .LessThanOrEqualTo(1440)
            .WithMessage("Timeout cannot exceed 24 hours (1440 minutes).");
    }
}
