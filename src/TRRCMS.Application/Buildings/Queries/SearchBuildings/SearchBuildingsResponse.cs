using TRRCMS.Application.Buildings.Dtos;

namespace TRRCMS.Application.Buildings.Queries.SearchBuildings;

/// <summary>
/// Paginated search response for buildings
/// </summary>
public class SearchBuildingsResponse
{
    public List<BuildingDto> Buildings { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}