using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.LinkPersonToPropertyUnit;

public class LinkPersonToPropertyUnitCommandValidator : LocalizedValidator<LinkPersonToPropertyUnitCommand>
{
    public LinkPersonToPropertyUnitCommandValidator(IStringLocalizer<ValidationMessages> localizer, IVocabularyValidationService vocabService) : base(localizer)
    {
        RuleFor(x => x.SurveyId)
            .NotEmpty()
            .WithMessage(L("SurveyId_Required"));

        RuleFor(x => x.PersonId)
            .NotEmpty()
            .WithMessage(L("PersonId_Required"));

        RuleFor(x => x.PropertyUnitId)
            .NotEmpty()
            .WithMessage(L("PropertyUnitId_Required"));

        // RelationType enum validation (int field)
        RuleFor(x => x.RelationType)
            .Must(v => vocabService.IsValidCode("relation_type", v))
            .WithMessage(L("RelationType_InvalidWithValues"));

        // OccupancyType enum validation (optional int field)
        RuleFor(x => x.OccupancyType)
            .Must(v => vocabService.IsValidCode("occupancy_type", v!.Value))
            .When(x => x.OccupancyType.HasValue)
            .WithMessage(L("OccupancyType_Invalid"));

        // Ownership share required for Owner
        RuleFor(x => x.OwnershipShare)
            .NotNull()
            .When(x => x.RelationType == (int)RelationType.Owner)
            .WithMessage(L("OwnershipShare_Required"));

        RuleFor(x => x.OwnershipShare)
            .GreaterThan(0)
            .When(x => x.RelationType == (int)RelationType.Owner && x.OwnershipShare.HasValue)
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
