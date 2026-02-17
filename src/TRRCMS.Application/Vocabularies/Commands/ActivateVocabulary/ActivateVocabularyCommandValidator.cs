using FluentValidation;

namespace TRRCMS.Application.Vocabularies.Commands.ActivateVocabulary;

public class ActivateVocabularyCommandValidator : AbstractValidator<ActivateVocabularyCommand>
{
    public ActivateVocabularyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vocabulary ID is required");
    }
}
