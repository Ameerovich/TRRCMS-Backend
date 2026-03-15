using FluentValidation;

namespace TRRCMS.Application.Sync.Commands.UploadSyncPackage;

public sealed class UploadSyncPackageCommandValidator : AbstractValidator<UploadSyncPackageCommand>
{
    public UploadSyncPackageCommandValidator()
    {
        RuleFor(x => x.Manifest.SyncSessionId).NotEmpty();
        RuleFor(x => x.Manifest.Sha256Checksum).MaximumLength(64);

        RuleFor(x => x.PackageStream).NotNull();
    }
}
