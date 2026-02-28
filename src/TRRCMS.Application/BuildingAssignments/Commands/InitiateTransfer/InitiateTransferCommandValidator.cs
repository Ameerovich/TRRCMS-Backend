using FluentValidation;

namespace TRRCMS.Application.BuildingAssignments.Commands.InitiateTransfer;

public sealed class InitiateTransferCommandValidator
    : AbstractValidator<InitiateTransferCommand>
{
    private const int MaxAssignmentIds = 500;

    public InitiateTransferCommandValidator()
    {
        RuleFor(x => x.FieldCollectorId)
            .NotEmpty()
            .WithMessage("Field collector ID is required.");

        RuleFor(x => x.AssignmentIds)
            .NotNull().WithMessage("AssignmentIds must not be null.")
            .NotEmpty().WithMessage("At least one assignment ID must be provided.")
            .Must(ids => ids.Count <= MaxAssignmentIds)
            .WithMessage($"A maximum of {MaxAssignmentIds} assignments may be initiated per request.");

        RuleForEach(x => x.AssignmentIds)
            .NotEmpty()
            .WithMessage("Each assignment ID must be a valid non-empty GUID.");
    }
}
