using MediatR;
using TRRCMS.Application.Claims.Dtos;

namespace TRRCMS.Application.Claims.Commands.UpdateClaim;

/// <summary>
/// Update existing claim with audit trail
/// UC-006: Update Existing Claim
/// </summary>
public class UpdateClaimCommand : IRequest<ClaimDto>
{
    public Guid ClaimId { get; set; }

    // Personal Information Updates
    public Guid? PrimaryClaimantId { get; set; }

    // Claim Classification
    public string? ClaimType { get; set; }
    public int? Priority { get; set; }

    // Tenure Details
    public int? TenureContractType { get; set; }
    public string? TenureContractDetails { get; set; }

    // Status (optional - careful with state machine)
    public int? Status { get; set; }

    // Additional Information
    public string? ProcessingNotes { get; set; }
    public string? PublicRemarks { get; set; }

    // REQUIRED: Audit trail
    /// <summary>
    /// Mandatory reason for modification (audit requirement)
    /// </summary>
    public string ReasonForModification { get; set; } = string.Empty;
}