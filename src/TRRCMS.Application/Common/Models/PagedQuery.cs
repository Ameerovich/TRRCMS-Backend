namespace TRRCMS.Application.Common.Models;

public enum SortDirection
{
    Ascending,
    Descending
}

/// <summary>
/// Base class for paginated MediatR queries.
/// PageSize is clamped to MaxPageSize (100) and PageNumber to minimum 1.
/// </summary>
public abstract class PagedQuery
{
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 20;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? DefaultPageSize : value > MaxPageSize ? MaxPageSize : value;
    }

    public string? SortBy { get; init; }
    public SortDirection SortDirection { get; init; } = SortDirection.Ascending;

    /// <summary>
    /// Clamp a page size value to the allowed range. Use this in standalone queries
    /// that don't inherit from PagedQuery.
    /// </summary>
    public static int ClampPageSize(int pageSize, int maxOverride = MaxPageSize)
        => pageSize < 1 ? DefaultPageSize : pageSize > maxOverride ? maxOverride : pageSize;

    /// <summary>
    /// Clamp a page number to minimum 1.
    /// </summary>
    public static int ClampPageNumber(int pageNumber)
        => pageNumber < 1 ? 1 : pageNumber;
}
