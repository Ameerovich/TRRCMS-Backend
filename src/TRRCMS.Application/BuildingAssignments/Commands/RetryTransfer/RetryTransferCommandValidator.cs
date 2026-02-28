using FluentValidation;

namespace TRRCMS.Application.BuildingAssignments.Commands.RetryTransfer;

public sealed class RetryTransferCommandValidator
    : AbstractValidator<RetryTransferCommand>
{
    private const int MaxAssignmentIds = 500;

    public RetryTransferCommandValidator()
    {
        RuleFor(x => x.AssignmentIds)
            .NotNull().WithMessage("AssignmentIds must not be null.")
            .NotEmpty().WithMessage("At least one assignment ID must be provided.")
            .Must(ids => ids.Count <= MaxAssignmentIds)
            .WithMessage($"A maximum of {MaxAssignmentIds} assignments may be retried per request.");

        RuleForEach(x => x.AssignmentIds)
            .NotEmpty()
            .WithMessage("Each assignment ID must be a valid non-empty GUID.");
    }
}
