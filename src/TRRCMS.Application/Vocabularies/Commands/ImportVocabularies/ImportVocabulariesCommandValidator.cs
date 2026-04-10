using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Vocabularies.Commands.ImportVocabularies;

public class ImportVocabulariesCommandValidator : LocalizedValidator<ImportVocabulariesCommand>
{
    public ImportVocabulariesCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Vocabularies)
            .NotEmpty().WithMessage(L("VocabImport_AtLeastOne"));

        RuleFor(x => x.Vocabularies)
            .Must(vocabs => vocabs.Select(v => v.VocabularyName).Distinct().Count() == vocabs.Count)
            .When(x => x.Vocabularies.Count > 0)
            .WithMessage(L("VocabImport_DuplicateNames"));

        RuleForEach(x => x.Vocabularies).ChildRules(vocab =>
        {
            vocab.RuleFor(v => v.VocabularyName)
                .NotEmpty().WithMessage(L("VocabName_Required"))
                .Matches(@"^[A-Za-z][A-Za-z0-9_]*$")
                .WithMessage(L("VocabName_InvalidChars"));

            vocab.RuleFor(v => v.DisplayNameArabic)
                .NotEmpty().WithMessage(L("VocabDisplayNameAr_Required"));

            vocab.RuleFor(v => v.Values)
                .NotEmpty().WithMessage(L("VocabValues_AtLeastOne"));

            vocab.RuleFor(v => v.Values)
                .Must(values => values.Select(val => val.Code).Distinct().Count() == values.Count)
                .When(v => v.Values.Count > 0)
                .WithMessage(L("VocabValues_UniqueCodes"));

            vocab.RuleForEach(v => v.Values).ChildRules(value =>
            {
                value.RuleFor(v => v.Code)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage(L("VocabValueCode_NonNegative"));

                value.RuleFor(v => v.LabelArabic)
                    .NotEmpty()
                    .WithMessage(L("VocabValueLabelAr_Required"));

                value.RuleFor(v => v.LabelArabic)
                    .MaximumLength(200)
                    .When(v => !string.IsNullOrEmpty(v.LabelArabic))
                    .WithMessage(L("VocabValueLabelAr_MaxLength200"));

                value.RuleFor(v => v.LabelEnglish)
                    .MaximumLength(200)
                    .When(v => !string.IsNullOrEmpty(v.LabelEnglish))
                    .WithMessage(L("VocabValueLabelEn_MaxLength200"));

                value.RuleFor(v => v.DisplayOrder)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage(L("VocabDisplayOrder_NonNegative"));
            });
        });
    }
}
