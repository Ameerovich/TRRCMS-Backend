using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Vocabularies.Commands.DeactivateVocabulary;

public class DeactivateVocabularyCommandValidator : LocalizedValidator<DeactivateVocabularyCommand>
{
    public DeactivateVocabularyCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(L("VocabularyId_Required"));
    }
}
