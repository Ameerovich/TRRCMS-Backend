using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Import.Queries.GetQuarantineReport;

public class GetQuarantineReportQueryHandler
    : IRequestHandler<GetQuarantineReportQuery, QuarantineReportDto>
{
    private readonly IImportPackageRepository _importPackageRepository;

    public GetQuarantineReportQueryHandler(IImportPackageRepository importPackageRepository)
    {
        _importPackageRepository = importPackageRepository;
    }

    public async Task<QuarantineReportDto> Handle(
        GetQuarantineReportQuery request, CancellationToken cancellationToken)
    {
        var package = await _importPackageRepository.GetByIdAsync(request.ImportPackageId, cancellationToken)
            ?? throw new NotFoundException($"ImportPackage with ID '{request.ImportPackageId}' was not found.");

        if (package.Status != ImportStatus.Quarantined)
            throw new ConflictException(
                $"Package '{package.PackageNumber}' is not quarantined. Current status: {package.Status}.");

        var category = DeriveCategory(package);

        return new QuarantineReportDto
        {
            Id = package.Id,
            PackageNumber = package.PackageNumber,
            FileName = package.FileName,
            CurrentStatus = (int)package.Status,
            QuarantineReason = package.ErrorMessage,
            QuarantineCategory = category,
            IsChecksumValid = package.IsChecksumValid,
            IsSignatureValid = package.IsSignatureValid,
            IsVocabularyCompatible = package.IsVocabularyCompatible,
            VocabularyCompatibilityIssues = package.VocabularyCompatibilityIssues,
            IsSchemaValid = package.IsSchemaValid,
            SchemaVersion = package.SchemaVersion,
            ErrorLog = package.ErrorLog,
            LastModifiedAtUtc = package.LastModifiedAtUtc
        };
    }

    private static string DeriveCategory(Domain.Entities.ImportPackage package)
    {
        if (!package.IsChecksumValid) return "ChecksumFailure";
        if (!package.IsSignatureValid) return "SignatureFailure";
        if (!package.IsVocabularyCompatible) return "VocabularyVersionMismatch";
        if (!package.IsSchemaValid) return "SchemaInvalid";
        return "ManualQuarantine";
    }
}
