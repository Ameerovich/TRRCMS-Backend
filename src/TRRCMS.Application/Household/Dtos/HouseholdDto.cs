namespace TRRCMS.Application.Households.Dtos;

/// <summary>
/// Data transfer object for Household entity
/// </summary>
public class HouseholdDto
{
    // ==================== IDENTIFIERS ====================

    public Guid Id { get; set; }
    public Guid PropertyUnitId { get; set; }

    // ==================== BASIC INFORMATION ====================

    public string HeadOfHouseholdName { get; set; } = string.Empty;
    public Guid? HeadOfHouseholdPersonId { get; set; }
    public int HouseholdSize { get; set; }

    // ==================== GENDER COMPOSITION ====================

    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }

    // ==================== AGE COMPOSITION ====================

    public int InfantCount { get; set; }
    public int ChildCount { get; set; }
    public int MinorCount { get; set; }
    public int AdultCount { get; set; }
    public int ElderlyCount { get; set; }

    // ==================== VULNERABILITY INDICATORS ====================

    public int PersonsWithDisabilitiesCount { get; set; }
    public bool IsFemaleHeaded { get; set; }
    public int WidowCount { get; set; }
    public int OrphanCount { get; set; }
    public int SingleParentCount { get; set; }

    // ==================== ECONOMIC INDICATORS ====================

    public int EmployedPersonsCount { get; set; }
    public int UnemployedPersonsCount { get; set; }
    public string? PrimaryIncomeSource { get; set; }
    public decimal? MonthlyIncomeEstimate { get; set; }

    // ==================== DISPLACEMENT & ORIGIN ====================

    public bool IsDisplaced { get; set; }
    public string? OriginLocation { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public string? DisplacementReason { get; set; }

    // ==================== ADDITIONAL INFORMATION ====================

    public string? Notes { get; set; }
    public string? SpecialNeeds { get; set; }

    // ==================== AUDIT FIELDS ====================

    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }

    // ==================== COMPUTED PROPERTIES ====================

    /// <summary>
    /// Calculated dependency ratio (dependents / working age adults)
    /// </summary>
    public decimal DependencyRatio { get; set; }

    /// <summary>
    /// Indicates if household is vulnerable based on indicators
    /// </summary>
    public bool IsVulnerable { get; set; }
}
