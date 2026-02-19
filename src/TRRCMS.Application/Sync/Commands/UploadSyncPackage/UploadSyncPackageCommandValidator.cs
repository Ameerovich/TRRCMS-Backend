using FluentValidation;

namespace TRRCMS.Application.Sync.Commands.UploadSyncPackage;

public sealed class UploadSyncPackageCommandValidator : AbstractValidator<UploadSyncPackageCommand>
{
    public UploadSyncPackageCommandValidator()
    {
        RuleFor(x => x.Manifest.SyncSessionId).NotEmpty();
        RuleFor(x => x.Manifest.PackageId).NotEmpty();
        RuleFor(x => x.Manifest.DeviceId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Manifest.CreatedUtc).NotEmpty();
        RuleFor(x => x.Manifest.SchemaVersion).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Manifest.AppVersion).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Manifest.FormSchemaVersion).MaximumLength(64);
        RuleFor(x => x.Manifest.Sha256Checksum).NotEmpty().MaximumLength(64);

        RuleFor(x => x.PackageStream).NotNull();
    }
}
