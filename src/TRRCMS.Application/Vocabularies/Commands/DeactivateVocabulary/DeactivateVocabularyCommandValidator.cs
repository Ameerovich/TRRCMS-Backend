using FluentValidation;

namespace TRRCMS.Application.Vocabularies.Commands.DeactivateVocabulary;

public class DeactivateVocabularyCommandValidator : AbstractValidator<DeactivateVocabularyCommand>
{
    public DeactivateVocabularyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vocabulary ID is required");
    }
}
