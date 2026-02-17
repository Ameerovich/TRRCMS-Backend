using FluentValidation;

namespace TRRCMS.Application.Vocabularies.Commands.ImportVocabularies;

public class ImportVocabulariesCommandValidator : AbstractValidator<ImportVocabulariesCommand>
{
    public ImportVocabulariesCommandValidator()
    {
        RuleFor(x => x.Vocabularies)
            .NotEmpty().WithMessage("At least one vocabulary must be provided for import.");

        RuleForEach(x => x.Vocabularies).ChildRules(vocab =>
        {
            vocab.RuleFor(v => v.VocabularyName)
                .NotEmpty().WithMessage("Vocabulary name is required.")
                .Matches(@"^[A-Za-z][A-Za-z0-9_]*$")
                .WithMessage("Vocabulary name must start with a letter and contain only letters, digits, and underscores.");

            vocab.RuleFor(v => v.DisplayNameArabic)
                .NotEmpty().WithMessage("Arabic display name is required.");

            vocab.RuleFor(v => v.Values)
                .NotEmpty().WithMessage("Vocabulary must have at least one value.");

            vocab.RuleFor(v => v.Values)
                .Must(values => values.Select(val => val.Code).Distinct().Count() == values.Count)
                .When(v => v.Values.Count > 0)
                .WithMessage("Vocabulary values must have unique codes.");
        });
    }
}
