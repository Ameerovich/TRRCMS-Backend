namespace TRRCMS.Application.Common.Models;

/// <summary>
/// Standard wrapper for list/collection responses.
/// Provides consistent structure with items array and total count.
/// </summary>
public class ListResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }

    public static ListResponse<T> From(IEnumerable<T> items)
    {
        var list = items.ToList();
        return new ListResponse<T>
        {
            Items = list,
            TotalCount = list.Count
        };
    }
}
