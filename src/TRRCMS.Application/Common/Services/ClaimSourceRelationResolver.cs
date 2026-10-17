using TRRCMS.Application.Common.Mappings;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Services;

/// <summary>
/// Finds the person–property relation a claim was produced from. Claims do not store that link,
/// and a claimant can hold several relations on the same unit (e.g. owner and occupant — one claim
/// each), so picking "the" relation by (claimant, unit) alone is ambiguous.
/// Rule: active relations first; among them the one whose claim type matches the claim's type;
/// among equals the latest created at or before the claim, else the latest.
/// </summary>
public static class ClaimSourceRelationResolver
{
    public static PersonPropertyRelation? Resolve(Claim claim, IEnumerable<PersonPropertyRelation>? candidates)
    {
        if (candidates == null)
            return null;

        return candidates
            .Where(r => !r.IsDeleted
                        && r.PersonId == claim.PrimaryClaimantId
                        && r.PropertyUnitId == claim.PropertyUnitId)
            .OrderByDescending(r => r.IsActive)
            .ThenByDescending(r => ClaimTypeRelationMapper.ToClaimType(r.RelationType) == claim.ClaimType)
            .ThenByDescending(r => r.CreatedAtUtc <= claim.CreatedAtUtc)
            .ThenByDescending(r => r.CreatedAtUtc)
            .ThenBy(r => r.Id)
            .FirstOrDefault();
    }
}
