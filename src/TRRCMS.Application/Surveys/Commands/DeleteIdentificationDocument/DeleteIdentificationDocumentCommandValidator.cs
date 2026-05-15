using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;

namespace TRRCMS.Application.Surveys.Commands.DeleteIdentificationDocument;

public class DeleteIdentificationDocumentCommandValidator : LocalizedValidator<DeleteIdentificationDocumentCommand>
{
    public DeleteIdentificationDocumentCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty().WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.DocumentId)
            .NotEmpty();
    }
}
