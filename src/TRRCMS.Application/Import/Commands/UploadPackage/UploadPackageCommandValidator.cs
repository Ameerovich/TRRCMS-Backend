using FluentValidation;
using TRRCMS.Application.Import.Models;
using Microsoft.Extensions.Options;

namespace TRRCMS.Application.Import.Commands.UploadPackage;

/// <summary>
/// Validator for UploadPackageCommand.
/// Checks: file required, .uhc extension, max size (from config).
/// Runs before the handler via MediatR ValidationBehavior pipeline.
/// </summary>
public class UploadPackageCommandValidator : AbstractValidator<UploadPackageCommand>
{
    public UploadPackageCommandValidator(IOptions<ImportPipelineSettings> settings)
    {
        var config = settings.Value;

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File is required (.uhc package)");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .Must(name => name.EndsWith(".uhc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only .uhc package files are accepted");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("File cannot be empty")
            .LessThanOrEqualTo(config.MaxUploadSizeBytes)
            .WithMessage($"File size cannot exceed {config.MaxUploadSizeMB} MB");

        RuleFor(x => x.ImportMethod)
            .NotEmpty()
            .WithMessage("Import method is required")
            .Must(m => m is "Manual" or "NetworkSync" or "WatchedFolder")
            .WithMessage("Import method must be Manual, NetworkSync, or WatchedFolder");
    }
}
