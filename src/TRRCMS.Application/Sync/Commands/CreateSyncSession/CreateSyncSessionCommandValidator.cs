using FluentValidation;

namespace TRRCMS.Application.Sync.Commands.CreateSyncSession;

public sealed class CreateSyncSessionCommandValidator : AbstractValidator<CreateSyncSessionCommand>
{
    public CreateSyncSessionCommandValidator()
    {
        RuleFor(x => x.Data.FieldCollectorId).NotEmpty();
        RuleFor(x => x.Data.DeviceId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Data.ServerIpAddress).MaximumLength(64);
    }
}
