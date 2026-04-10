using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Sync.Commands.UploadSyncPackage;

public sealed class UploadSyncPackageCommandValidator : LocalizedValidator<UploadSyncPackageCommand>
{
    public UploadSyncPackageCommandValidator(IStringLocalizer<ValidationMessages> localizer) : base(localizer)
    {
        RuleFor(x => x.Manifest.SyncSessionId).NotEmpty();
        RuleFor(x => x.Manifest.Sha256Checksum).MaximumLength(64);

        RuleFor(x => x.PackageStream).NotNull();
    }
}
