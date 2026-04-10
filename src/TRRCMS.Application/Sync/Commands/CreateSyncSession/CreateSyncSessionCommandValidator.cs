using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Sync.Commands.CreateSyncSession;

public sealed class CreateSyncSessionCommandValidator : LocalizedValidator<CreateSyncSessionCommand>
{
    public CreateSyncSessionCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Data.FieldCollectorId).NotEmpty();
        RuleFor(x => x.Data.DeviceId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Data.ServerIpAddress).MaximumLength(64);
    }
}
