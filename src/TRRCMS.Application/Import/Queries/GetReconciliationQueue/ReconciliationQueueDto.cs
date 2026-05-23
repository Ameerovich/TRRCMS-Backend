namespace TRRCMS.Application.Import.Queries.GetReconciliationQueue;

/// <summary>
/// Response payload for <see cref="GetReconciliationQueueQuery"/>.
/// </summary>
public class ReconciliationQueueDto
{
    public List<PersonNationalIdReconciliationDto> Persons { get; set; } = new();
    public int PersonsTotalCount { get; set; }

    public List<PropertyUnitIdentifierReconciliationDto> PropertyUnits { get; set; } = new();
    public int PropertyUnitsTotalCount { get; set; }

    public int Page { get; set; }
    public int PageSize { get; set; }
}

/// <summary>A person committed with its National ID cleared, awaiting reconciliation.</summary>
public class PersonNationalIdReconciliationDto
{
    public Guid PersonId { get; set; }
    public string FullNameArabic { get; set; } = string.Empty;

    /// <summary>The National ID removed at commit time, preserved for the reviewer to verify/restore.</summary>
    public string? PreservedNationalId { get; set; }

    public string? MobileNumber { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}

/// <summary>A property unit committed with a suffix-adjusted identifier, awaiting reconciliation.</summary>
public class PropertyUnitIdentifierReconciliationDto
{
    public Guid PropertyUnitId { get; set; }
    public Guid BuildingId { get; set; }

    /// <summary>17-digit building code, when the parent building is loaded.</summary>
    public string? BuildingCode { get; set; }

    /// <summary>The identifier currently stored (with the appended suffix).</summary>
    public string CurrentUnitIdentifier { get; set; } = string.Empty;

    /// <summary>The identifier as received in the package before the suffix was appended.</summary>
    public string? OriginalUnitIdentifier { get; set; }

    public DateTime? LastModifiedAtUtc { get; set; }
}
