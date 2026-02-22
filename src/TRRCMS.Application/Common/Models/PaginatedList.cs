namespace TRRCMS.Application.Common.Models;

/// <summary>
/// Factory for creating <see cref="PagedResult{T}"/> instances from pre-fetched data.
/// Repositories handle the actual Skip/Take against the database;
/// this helper normalises inputs and builds the response envelope.
/// </summary>
public static class PaginatedList
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    /// <summary>
    /// Build a <see cref="PagedResult{T}"/> from items that have already been
    /// sliced by the repository (i.e. one page of data).
    /// </summary>
    public static PagedResult<T> Create<T>(
        IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Paginate an in-memory collection (useful for small reference-data sets).
    /// For database queries, prefer repository-level Skip/Take instead.
    /// </summary>
    public static PagedResult<T> FromEnumerable<T>(
        IEnumerable<T> source, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var materialised = source as IReadOnlyList<T> ?? source.ToList();
        var totalCount = materialised.Count;
        var items = materialised
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Normalise raw page parameters (clamp to valid ranges).
    /// Call from handlers/repositories before building the database query.
    /// </summary>
    public static (int Page, int Size) Normalise(
        int pageNumber, int pageSize, int maxPageSize = MaxPageSize)
    {
        return (Math.Max(1, pageNumber), Math.Clamp(pageSize, 1, maxPageSize));
    }
}
