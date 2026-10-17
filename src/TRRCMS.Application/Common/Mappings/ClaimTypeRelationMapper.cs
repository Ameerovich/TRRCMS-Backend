using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Mappings;

public static class ClaimTypeRelationMapper
{
    public static ClaimType ToClaimType(RelationType relationType)
    {
        return relationType is RelationType.Owner or RelationType.Heir
            ? ClaimType.OwnershipClaim
            : ClaimType.OccupancyClaim;
    }

    public static RelationType ToDefaultRelationType(ClaimType claimType)
    {
        return claimType switch
        {
            ClaimType.OwnershipClaim => RelationType.Owner,
            ClaimType.OccupancyClaim => RelationType.Occupant,
            _ => throw new ArgumentOutOfRangeException(
                nameof(claimType),
                claimType,
                "Unsupported claim type.")
        };
    }

    public static bool IsCompatible(
        RelationType relationType,
        ClaimType claimType)
    {
        return ToClaimType(relationType) == claimType;
    }

    public static RelationType ResolveForUpdate(
        RelationType currentRelationType,
        ClaimType claimType)
    {
        if (IsCompatible(currentRelationType, claimType))
            return currentRelationType;

        return ToDefaultRelationType(claimType);
    }
}