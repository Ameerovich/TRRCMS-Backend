namespace TRRCMS.Application.Common.Models;

/// <summary>
/// Result of a bulk insert/update operation.
/// </summary>
public class BulkOperationResult<T>
{
    public int TotalReceived { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public List<T> CreatedItems { get; set; } = new();
    public List<BulkErrorItem> Errors { get; set; } = new();
}

public class BulkErrorItem
{
    public int Index { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
