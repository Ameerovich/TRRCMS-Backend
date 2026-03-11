namespace TRRCMS.Domain.Enums;

/// <summary>
/// Claim type classification (نوع المطالبة)
/// Describes the nature of the tenure claim.
/// </summary>
public enum ClaimType
{
    /// <summary>
    /// Ownership claim — claimant asserts property ownership (مطالبة ملكية)
    /// </summary>
    [ArabicLabel("مطالبة ملكية")]
    OwnershipClaim = 1,

    /// <summary>
    /// Occupancy claim — claimant is an occupant (tenant, guest, etc.) (مطالبة إشغال)
    /// </summary>
    [ArabicLabel("مطالبة إشغال")]
    OccupancyClaim = 2
}
