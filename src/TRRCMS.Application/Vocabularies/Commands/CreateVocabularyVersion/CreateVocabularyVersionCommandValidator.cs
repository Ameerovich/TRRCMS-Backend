using FluentValidation;

namespace TRRCMS.Application.Vocabularies.Commands.CreateVocabularyVersion;

public class CreateVocabularyVersionCommandValidator : AbstractValidator<CreateVocabularyVersionCommand>
{
    private static readonly string[] ValidVersionTypes = { "minor", "major", "patch" };

    public CreateVocabularyVersionCommandValidator()
    {
        RuleFor(x => x.VocabularyId)
            .NotEmpty().WithMessage("Vocabulary ID is required");

        RuleFor(x => x.VersionType)
            .NotEmpty().WithMessage("Version type is required")
            .Must(vt => ValidVersionTypes.Contains(vt.ToLowerInvariant()))
            .WithMessage("Version type must be 'minor', 'major', or 'patch'")
            .When(x => !string.IsNullOrWhiteSpace(x.VersionType));

        RuleFor(x => x.ChangeLog)
            .NotEmpty().WithMessage("Change log is required")
            .MaximumLength(2000).WithMessage("Change log must not exceed 2000 characters");

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
