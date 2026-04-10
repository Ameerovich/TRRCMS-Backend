using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Vocabularies.Commands.ActivateVocabulary;

public class ActivateVocabularyCommandValidator : LocalizedValidator<ActivateVocabularyCommand>
{
    public ActivateVocabularyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(L("VocabularyId_Required"));
    }
}
