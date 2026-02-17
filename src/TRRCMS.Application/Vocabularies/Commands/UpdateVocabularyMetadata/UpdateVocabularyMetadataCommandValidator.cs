using FluentValidation;

namespace TRRCMS.Application.Vocabularies.Commands.UpdateVocabularyMetadata;

public class UpdateVocabularyMetadataCommandValidator : AbstractValidator<UpdateVocabularyMetadataCommand>
{
    public UpdateVocabularyMetadataCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vocabulary ID is required");

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
    }
}
