namespace TRRCMS.Application.Common.Models;

public enum SortDirection
{
    Ascending,
    Descending
}

/// <summary>
/// Base class for paginated MediatR queries.
/// Handlers should clamp values and apply defaults where needed.
/// </summary>
public abstract class PagedQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;
}
