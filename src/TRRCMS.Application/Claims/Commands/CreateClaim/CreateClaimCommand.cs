using MediatR;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Claims.Commands.CreateClaim;

/// <summary>
/// Command to create a new claim
/// Supports manual claim creation (typically from office workflow)
/// </summary>
public class CreateClaimCommand : IRequest<ClaimDto>
{
    // ==================== REQUIRED FIELDS ====================
    
    /// <summary>
    /// Property unit ID this claim is for (required)
    /// </summary>
    public Guid PropertyUnitId { get; set; }
    
    /// <summary>
    /// Claim type (required) - e.g., "Ownership Claim", "Occupancy Claim"
    /// </summary>
    public string ClaimType { get; set; } = string.Empty;
    
    /// <summary>
    /// How the claim entered the system (required)
    /// </summary>
    public int ClaimSource { get; set; }
    
    /// <summary>
    /// User ID creating this claim (required)
    /// </summary>
    public Guid CreatedByUserId { get; set; }
    
    // ==================== OPTIONAL FIELDS ====================
    
    /// <summary>
    /// Primary claimant (person) ID
    /// </summary>
    public Guid? PrimaryClaimantId { get; set; }
    
    /// <summary>
    /// Priority level (defaults to Normal if not specified)
    /// </summary>
    public int Priority { get; set; } = (int)CasePriority.Normal;
    
    /// <summary>
    /// Type of tenure contract
    /// </summary>
    public int? TenureContractType { get; set; }
    
    /// <summary>
    /// Ownership share (fraction out of 2400)
    /// </summary>
    public int? OwnershipShare { get; set; }
    
    /// <summary>
    /// Date from which tenure/occupancy started
    /// </summary>
    public DateTime? TenureStartDate { get; set; }
    
    /// <summary>
    /// Date when tenure/occupancy ended
    /// </summary>
    public DateTime? TenureEndDate { get; set; }
    
    /// <summary>
    /// Detailed description of the claim
    /// </summary>
    public string? ClaimDescription { get; set; }
    
    /// <summary>
    /// Legal basis for the claim
    /// </summary>
    public string? LegalBasis { get; set; }
    
    /// <summary>
    /// Supporting narrative or story
    /// </summary>
    public string? SupportingNarrative { get; set; }
    
    /// <summary>
    /// Target completion date
    /// </summary>
    public DateTime? TargetCompletionDate { get; set; }
    
    /// <summary>
    /// Processing notes
    /// </summary>
    public string? ProcessingNotes { get; set; }
    
    /// <summary>
    /// Public remarks
    /// </summary>
    public string? PublicRemarks { get; set; }
}
