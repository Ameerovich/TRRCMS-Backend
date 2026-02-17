using FluentValidation;

namespace TRRCMS.Application.Vocabularies.Commands.CreateVocabulary;

public class CreateVocabularyCommandValidator : AbstractValidator<CreateVocabularyCommand>
{
    public CreateVocabularyCommandValidator()
    {
        RuleFor(x => x.VocabularyName)
            .NotEmpty().WithMessage("Vocabulary name is required")
            .MaximumLength(100).WithMessage("Vocabulary name must not exceed 100 characters")
            .Matches(@"^[A-Za-z][A-Za-z0-9_]*$").WithMessage("Vocabulary name must start with a letter and contain only letters, numbers, and underscores");

        RuleFor(x => x.DisplayNameArabic)
            .NotEmpty().WithMessage("Arabic display name is required")
            .MaximumLength(200).WithMessage("Arabic display name must not exceed 200 characters");

        RuleFor(x => x.DisplayNameEnglish)
            .MaximumLength(200).WithMessage("English display name must not exceed 200 characters")
            .When(x => x.DisplayNameEnglish is not null);

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters")
            .When(x => x.Category is not null);

        RuleFor(x => x.Values)
            .NotEmpty().WithMessage("At least one vocabulary value is required");

        RuleForEach(x => x.Values).ChildRules(value =>
        {
            value.RuleFor(v => v.Code)
                .GreaterThanOrEqualTo(0).WithMessage("Value code must be non-negative");

            value.RuleFor(v => v.LabelArabic)
                .NotEmpty().WithMessage("Arabic label is required for each value");
        });

        RuleFor(x => x.Values)
            .Must(HaveUniqueCodes).WithMessage("Vocabulary values must have unique codes")
            .When(x => x.Values is { Count: > 0 });
    }

    private static bool HaveUniqueCodes(List<Dtos.VocabularyValueDto> values)
    {
        return values.Select(v => v.Code).Distinct().Count() == values.Count;
    }
}
