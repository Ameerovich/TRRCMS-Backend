using MediatR;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Import.Queries.GetImportPackages;

/// <summary>
/// Query to retrieve import packages with filtering, sorting, and pagination.
/// </summary>
public class GetImportPackagesQuery : IRequest<GetImportPackagesResponse>
{
    public ImportStatus? Status { get; set; }
    public Guid? ExportedByUserId { get; set; }
    public Guid? ImportedByUserId { get; set; }
    public DateTime? ImportedAfter { get; set; }
    public DateTime? ImportedBefore { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

public class GetImportPackagesResponse
{
    public List<ImportPackageDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
