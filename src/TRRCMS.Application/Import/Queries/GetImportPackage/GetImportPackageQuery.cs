using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Queries.GetImportPackage;

/// <summary>
/// Query to retrieve a single import package by its surrogate ID.
/// </summary>
public record GetImportPackageQuery : IRequest<ImportPackageDto?>
{
    public Guid Id { get; init; }
}
