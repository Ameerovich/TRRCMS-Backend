using FluentValidation;

namespace TRRCMS.Application.Vocabularies.Commands.ImportVocabularies;

public class ImportVocabulariesCommandValidator : AbstractValidator<ImportVocabulariesCommand>
{
    public ImportVocabulariesCommandValidator()
    {
        RuleFor(x => x.Vocabularies)
            .NotEmpty().WithMessage("At least one vocabulary must be provided for import.");

        RuleFor(x => x.Vocabularies)
            .Must(vocabs => vocabs.Select(v => v.VocabularyName).Distinct().Count() == vocabs.Count)
            .When(x => x.Vocabularies.Count > 0)
            .WithMessage("Duplicate vocabulary names in import batch.");

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

            vocab.RuleForEach(v => v.Values).ChildRules(value =>
            {
                value.RuleFor(v => v.Code)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Code must be non-negative.");

                value.RuleFor(v => v.LabelArabic)
                    .NotEmpty()
                    .WithMessage("Arabic label is required for each value.");

                value.RuleFor(v => v.LabelArabic)
                    .MaximumLength(200)
                    .When(v => !string.IsNullOrEmpty(v.LabelArabic))
                    .WithMessage("Arabic label must not exceed 200 characters.");

                value.RuleFor(v => v.LabelEnglish)
                    .MaximumLength(200)
                    .When(v => !string.IsNullOrEmpty(v.LabelEnglish))
                    .WithMessage("English label must not exceed 200 characters.");

                value.RuleFor(v => v.DisplayOrder)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Display order must be non-negative.");
            });
        });
    }
}
