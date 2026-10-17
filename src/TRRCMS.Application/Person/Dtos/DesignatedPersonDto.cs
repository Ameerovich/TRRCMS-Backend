namespace TRRCMS.Application.Persons.Dtos;

/// <summary>
/// A person in the context of a property/case with their designations, so a desk operator
/// can tell who to call (Contact Point) and who holds rights claims (Rights Claimant).
/// A person can hold both designations.
/// </summary>
public class DesignatedPersonDto
{
    public Guid PersonId { get; set; }
    public string FirstNameArabic { get; set; } = string.Empty;
    public string FatherNameArabic { get; set; } = string.Empty;
    public string FamilyNameArabic { get; set; } = string.Empty;
    public string? MotherNameArabic { get; set; }

    /// <summary>
    /// "First Father Family" in Arabic
    /// </summary>
    public string FullNameArabic { get; set; } = string.Empty;

    public string? NationalId { get; set; }
    public string? MobileNumber { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? HouseholdId { get; set; }

    /// <summary>
    /// جهة الاتصال - the person interviewed, to be contacted in the future
    /// </summary>
    public bool IsContactPoint { get; set; }

    /// <summary>
    /// صاحب حق - the person has a rights claim (an active person–property relation) on the unit
    /// </summary>
    public bool IsRightsClaimant { get; set; }

    /// <summary>
    /// Relation type on the unit (null when the person is only a contact point)
    /// </summary>
    public int? RelationType { get; set; }

    /// <summary>
    /// Claim type derived from the relation: 1 = Ownership, 2 = Occupancy (null when not a claimant)
    /// </summary>
    public int? ClaimType { get; set; }

    public Guid? RelationId { get; set; }
}
