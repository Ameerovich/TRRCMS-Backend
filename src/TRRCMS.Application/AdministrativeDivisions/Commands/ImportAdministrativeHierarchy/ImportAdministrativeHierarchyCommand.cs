using MediatR;

namespace TRRCMS.Application.AdministrativeDivisions.Commands.ImportAdministrativeHierarchy;

/// <summary>
/// Command to import administrative hierarchy data from JSON
/// </summary>
public record ImportAdministrativeHierarchyCommand : IRequest<ImportAdministrativeHierarchyResult>
{
    /// <summary>
    /// JSON content containing administrative hierarchy data
    /// </summary>
    public string JsonContent { get; init; } = string.Empty;

    /// <summary>
    /// Whether to auto-generate placeholder neighborhoods for each community
    /// </summary>
    public bool GeneratePlaceholderNeighborhoods { get; init; } = true;

    /// <summary>
    /// User ID for audit trail
    /// </summary>
    public Guid ImportedByUserId { get; init; }
}

/// <summary>
/// Result of administrative hierarchy import operation
/// </summary>
public class ImportAdministrativeHierarchyResult
{
    public int GovernoratesImported { get; set; }
    public int DistrictsImported { get; set; }
    public int SubDistrictsImported { get; set; }
    public int CommunitiesImported { get; set; }
    public int NeighborhoodsGenerated { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
