using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Vocabularies.Commands.CreateVocabulary;

public class CreateVocabularyCommandValidator : LocalizedValidator<CreateVocabularyCommand>
{
    public CreateVocabularyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.VocabularyName)
            .NotEmpty().WithMessage(L("VocabName_Required"))
            .MaximumLength(100).WithMessage(L("VocabName_MaxLength100"))
            .Matches(@"^[A-Za-z][A-Za-z0-9_]*$").WithMessage(L("VocabName_InvalidChars"));

        RuleFor(x => x.DisplayNameArabic)
            .NotEmpty().WithMessage(L("VocabDisplayNameAr_Required"))
            .MaximumLength(200).WithMessage(L("VocabDisplayNameAr_MaxLength200"));

        RuleFor(x => x.DisplayNameEnglish)
            .MaximumLength(200).WithMessage(L("VocabDisplayNameEn_MaxLength200"))
            .When(x => x.DisplayNameEnglish is not null);

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage(L("Description_MaxLength2000"))
            .When(x => x.Description is not null);

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage(L("VocabCategory_MaxLength100"))
            .When(x => x.Category is not null);

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
