using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Surveys.Commands.UpdatePersonPropertyRelation;

public class UpdatePersonPropertyRelationCommandValidator : LocalizedValidator<UpdatePersonPropertyRelationCommand>
{
    public UpdatePersonPropertyRelationCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.RelationId)
            .NotEmpty()
            .WithMessage(L("RelationId_Required"));

        // RelationType enum validation (optional int field for partial update)
        RuleFor(x => x.RelationType)
            .Must(v => vocabService.IsValidCode("relation_type", v!.Value))
            .When(x => x.RelationType.HasValue)
            .WithMessage(L("RelationType_Invalid"));

        // OccupancyType enum validation (optional int field)
        RuleFor(x => x.OccupancyType)
            .Must(v => vocabService.IsValidCode("occupancy_type", v!.Value))
            .When(x => x.OccupancyType.HasValue)
            .WithMessage(L("OccupancyType_Invalid"));

        // Ownership share validation
        RuleFor(x => x.OwnershipShare)
            .GreaterThan(0)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage(L("OwnershipShare_GreaterThanZero"));

        RuleFor(x => x.OwnershipShare)
            .LessThanOrEqualTo(2400)
            .When(x => x.OwnershipShare.HasValue)
            .WithMessage(L("OwnershipShare_Max2400"));

        // Text field max lengths
        RuleFor(x => x.ContractDetails)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.ContractDetails));

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
