using FluentValidation;

namespace TRRCMS.Application.Surveys.Commands.LinkEvidenceToRelation;

/// <summary>
/// Validator for LinkEvidenceToRelationCommand
/// </summary>
public class LinkEvidenceToRelationCommandValidator : AbstractValidator<LinkEvidenceToRelationCommand>
{
    public LinkEvidenceToRelationCommandValidator()
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage("Survey ID is required");

        RuleFor(x => x.EvidenceId)
            .NotEmpty()
            .WithMessage("Evidence ID is required");

        RuleFor(x => x.PersonPropertyRelationId)
            .NotEmpty()
            .WithMessage("Person-property relation ID is required");

        RuleFor(x => x.LinkReason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.LinkReason))
            .WithMessage("Link reason cannot exceed 500 characters");
    }
}
