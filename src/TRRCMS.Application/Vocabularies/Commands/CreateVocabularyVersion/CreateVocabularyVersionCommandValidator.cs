using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Vocabularies.Commands.CreateVocabularyVersion;

public class CreateVocabularyVersionCommandValidator : LocalizedValidator<CreateVocabularyVersionCommand>
{
    private static readonly string[] ValidVersionTypes = { "minor", "major", "patch" };

    public CreateVocabularyVersionCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.VocabularyId)
            .NotEmpty().WithMessage(L("VocabularyId_Required"));

        RuleFor(x => x.VersionType)
            .NotEmpty().WithMessage(L("VocabVersionType_Required"))
            .Must(vt => ValidVersionTypes.Contains(vt.ToLowerInvariant()))
            .WithMessage(L("VocabVersionType_Invalid"))
            .When(x => !string.IsNullOrWhiteSpace(x.VersionType));

        RuleFor(x => x.ChangeLog)
            .NotEmpty().WithMessage(L("VocabChangeLog_Required"))
            .MaximumLength(2000).WithMessage(L("VocabChangeLog_MaxLength2000"));

        RuleFor(x => x.Values)
            .NotEmpty().WithMessage(L("VocabValues_AtLeastOne"));

        RuleForEach(x => x.Values).ChildRules(value =>
        {
            value.RuleFor(v => v.Code)
                .GreaterThanOrEqualTo(0).WithMessage(L("VocabValueCode_NonNegative"));

            value.RuleFor(v => v.LabelArabic)
                .NotEmpty().WithMessage(L("VocabValueLabelAr_Required"));
        });

        RuleFor(x => x.Values)
            .Must(HaveUniqueCodes).WithMessage(L("VocabValues_UniqueCodes"))
            .When(x => x.Values is { Count: > 0 });
    }

    private static bool HaveUniqueCodes(List<Dtos.VocabularyValueDto> values)
    {
        return values.Select(v => v.Code).Distinct().Count() == values.Count;
    }
}
