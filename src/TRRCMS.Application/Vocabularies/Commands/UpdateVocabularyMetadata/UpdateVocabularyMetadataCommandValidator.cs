using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Vocabularies.Commands.UpdateVocabularyMetadata;

public class UpdateVocabularyMetadataCommandValidator : LocalizedValidator<UpdateVocabularyMetadataCommand>
{
    public UpdateVocabularyMetadataCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(L("VocabularyId_Required"));

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
    }
}
