using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using TRRCMS.Application.Common.Localization;
using TRRCMS.Application.Import.Models;
using TRRCMS.Application.Resources;

namespace TRRCMS.Application.Import.Commands.UploadPackage;

/// <summary>
/// Validator for UploadPackageCommand.
/// Checks: file required, .uhc extension, max size (from config).
/// Runs before the handler via MediatR ValidationBehavior pipeline.
/// </summary>
public class UploadPackageCommandValidator : LocalizedValidator<UploadPackageCommand>
{
    public UploadPackageCommandValidator(IStringLocalizer<ValidationMessages> localizer, IOptions<ImportPipelineSettings> settings) : base(localizer)
    {
        var config = settings.Value;

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage(L("UhcFile_Required"));

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage(L("FileName_Required_Import"))
            .Must(name => name.EndsWith(".uhc", StringComparison.OrdinalIgnoreCase))
            .WithMessage(L("UhcFile_ExtensionOnly"));

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage(L("File_CannotBeEmpty"))
            .LessThanOrEqualTo(config.MaxUploadSizeBytes)
            .WithMessage(L("File_SizeExceedsMax", config.MaxUploadSizeMB));

        RuleFor(x => x.ImportMethod)
            .NotEmpty()
            .WithMessage(L("ImportMethod_Required"))
            .Must(m => m is "Manual" or "NetworkSync" or "WatchedFolder" or "Sync")
            .WithMessage(L("ImportMethod_Invalid"));
    }
}
