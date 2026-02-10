using AutoMapper;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Documents.Dtos;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Building mappings
        CreateMap<Building, BuildingDto>()
           .ForMember(dest => dest.BuildingType, opt => opt.MapFrom(src => src.BuildingType.ToString()))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
           .ForMember(dest => dest.DamageLevel, opt => opt.MapFrom(src => src.DamageLevel.HasValue ? src.DamageLevel.ToString() : null))
           .ForMember(dest => dest.LocationDescription, opt => opt.MapFrom(src => src.LocationDescription));

        // PropertyUnit mapping

        CreateMap<PropertyUnit, PropertyUnitDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.BuildingId, opt => opt.MapFrom(src => src.BuildingId))
            .ForMember(dest => dest.BuildingNumber, opt => opt.Ignore()) // Set in handler
            .ForMember(dest => dest.UnitIdentifier, opt => opt.MapFrom(src => src.UnitIdentifier))
            .ForMember(dest => dest.FloorNumber, opt => opt.MapFrom(src => src.FloorNumber))
            .ForMember(dest => dest.UnitType, opt => opt.MapFrom(src => src.UnitType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AreaSquareMeters, opt => opt.MapFrom(src => src.AreaSquareMeters))
            .ForMember(dest => dest.NumberOfRooms, opt => opt.MapFrom(src => src.NumberOfRooms))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.MapFrom(src => src.CreatedAtUtc))
            .ForMember(dest => dest.LastModifiedAtUtc, opt => opt.MapFrom(src => src.LastModifiedAtUtc));

        CreateMap<Person, PersonDto>()
                    // Identifiers
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))

                    // Personal information
                    .ForMember(dest => dest.FamilyNameArabic, opt => opt.MapFrom(src => src.FamilyNameArabic))
                    .ForMember(dest => dest.FirstNameArabic, opt => opt.MapFrom(src => src.FirstNameArabic))
                    .ForMember(dest => dest.FatherNameArabic, opt => opt.MapFrom(src => src.FatherNameArabic))
                    .ForMember(dest => dest.MotherNameArabic, opt => opt.MapFrom(src => src.MotherNameArabic))
                    .ForMember(dest => dest.NationalId, opt => opt.MapFrom(src => src.NationalId))
                    .ForMember(dest => dest.YearOfBirth, opt => opt.MapFrom(src => src.YearOfBirth))

                    // Contact information
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                    .ForMember(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.MobileNumber))
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))

                    // Household context
                    .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.HouseholdId))
                    .ForMember(dest => dest.RelationshipToHead, opt => opt.MapFrom(src => src.RelationshipToHead))

                    // Audit fields
                    .ForMember(dest => dest.CreatedAtUtc, opt => opt.MapFrom(src => src.CreatedAtUtc))
                    .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                    .ForMember(dest => dest.LastModifiedAtUtc, opt => opt.MapFrom(src => src.LastModifiedAtUtc))
                    .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.LastModifiedBy))
                    .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                    .ForMember(dest => dest.DeletedAtUtc, opt => opt.MapFrom(src => src.DeletedAtUtc))
                    .ForMember(dest => dest.DeletedBy, opt => opt.MapFrom(src => src.DeletedBy));
        CreateMap<Household, HouseholdDto>()
       // Identifiers
       .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
       .ForMember(dest => dest.PropertyUnitId, opt => opt.MapFrom(src => src.PropertyUnitId))
       .ForMember(dest => dest.PropertyUnitIdentifier, opt => opt.Ignore()) // Set in handler

       // Basic info
       .ForMember(dest => dest.HeadOfHouseholdName, opt => opt.MapFrom(src => src.HeadOfHouseholdName))
       .ForMember(dest => dest.HeadOfHouseholdPersonId, opt => opt.MapFrom(src => src.HeadOfHouseholdPersonId))
       .ForMember(dest => dest.HouseholdSize, opt => opt.MapFrom(src => src.HouseholdSize))
       .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))

       // Adults composition
       .ForMember(dest => dest.MaleCount, opt => opt.MapFrom(src => src.MaleCount))
       .ForMember(dest => dest.FemaleCount, opt => opt.MapFrom(src => src.FemaleCount))

       // Children composition
       .ForMember(dest => dest.MaleChildCount, opt => opt.MapFrom(src => src.MaleChildCount))
       .ForMember(dest => dest.FemaleChildCount, opt => opt.MapFrom(src => src.FemaleChildCount))

       // Elderly composition
       .ForMember(dest => dest.MaleElderlyCount, opt => opt.MapFrom(src => src.MaleElderlyCount))
       .ForMember(dest => dest.FemaleElderlyCount, opt => opt.MapFrom(src => src.FemaleElderlyCount))

       // Disabled composition
       .ForMember(dest => dest.MaleDisabledCount, opt => opt.MapFrom(src => src.MaleDisabledCount))
       .ForMember(dest => dest.FemaleDisabledCount, opt => opt.MapFrom(src => src.FemaleDisabledCount))

       // Audit fields
       .ForMember(dest => dest.CreatedAtUtc, opt => opt.MapFrom(src => src.CreatedAtUtc))
       .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
       .ForMember(dest => dest.LastModifiedAtUtc, opt => opt.MapFrom(src => src.LastModifiedAtUtc))
       .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.LastModifiedBy))
       .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
       .ForMember(dest => dest.DeletedAtUtc, opt => opt.MapFrom(src => src.DeletedAtUtc))
       .ForMember(dest => dest.DeletedBy, opt => opt.MapFrom(src => src.DeletedBy));


        // PersonPropertyRelation mappings
        CreateMap<PersonPropertyRelation, PersonPropertyRelationDto>()
            .ForMember(dest => dest.DurationInDays, opt => opt.MapFrom(src =>
                src.StartDate.HasValue && src.EndDate.HasValue
                    ? (src.EndDate.Value - src.StartDate.Value).Days
                    : (int?)null))
            .ForMember(dest => dest.IsOngoing, opt => opt.MapFrom(src =>
                src.StartDate.HasValue && !src.EndDate.HasValue));

        // Evidence mappings
        CreateMap<Evidence, EvidenceDto>()
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired()));

        // Document mappings
        CreateMap<Document, DocumentDto>()
            .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => src.DocumentType.ToString()))
            .ForMember(dest => dest.VerificationStatus, opt => opt.MapFrom(src => src.VerificationStatus.ToString()))
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired()))
            .ForMember(dest => dest.IsExpiringSoon, opt => opt.MapFrom(src => src.IsExpiringSoon()));

        // Claim mappings
        CreateMap<Claim, TRRCMS.Application.Claims.Dtos.ClaimDto>()
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.IsOverdue()))
            .ForMember(dest => dest.DaysUntilDeadline, opt => opt.MapFrom(src =>
                src.TargetCompletionDate.HasValue
                    ? (int?)(src.TargetCompletionDate.Value - DateTime.UtcNow).Days
                    : null))
            .ForMember(dest => dest.DaysSinceSubmission, opt => opt.MapFrom(src =>
                src.SubmittedDate.HasValue
                    ? (int?)(DateTime.UtcNow - src.SubmittedDate.Value).Days
                    : null))
            .ForMember(dest => dest.HasEvidence, opt => opt.MapFrom(src => src.EvidenceCount > 0))
            .ForMember(dest => dest.IsPendingVerification, opt => opt.MapFrom(src =>
                src.VerificationStatus == Domain.Enums.VerificationStatus.Pending))
            .ForMember(dest => dest.RequiresAction, opt => opt.MapFrom(src =>
                (src.HasConflicts && src.LifecycleStage != Domain.Enums.LifecycleStage.InAdjudication)
                || (!src.AllRequiredDocumentsSubmitted && src.LifecycleStage != Domain.Enums.LifecycleStage.AwaitingDocuments)
                || (src.VerificationStatus == Domain.Enums.VerificationStatus.Pending && src.LifecycleStage == Domain.Enums.LifecycleStage.UnderReview)
                || (src.IsOverdue() && !src.DecisionDate.HasValue)))
            .ForMember(dest => dest.PropertyUnitCode, opt => opt.MapFrom(src =>
                src.PropertyUnit != null ? src.PropertyUnit.UnitIdentifier : null))
            .ForMember(dest => dest.PrimaryClaimantName, opt => opt.MapFrom(src =>
                src.PrimaryClaimant != null ? $"{src.PrimaryClaimant.FirstNameArabic} {src.PrimaryClaimant.FatherNameArabic} {src.PrimaryClaimant.FamilyNameArabic}" : null))
            .ForMember(dest => dest.AssignedToUserName, opt => opt.Ignore()); // Will be populated from user service later

        // User mappings - Base DTO
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.SupervisorName, opt => opt.MapFrom(src =>
                src.Supervisor != null ? src.Supervisor.FullNameArabic : null));

        // User mappings - List DTO (lightweight for GetAllUsers)
        CreateMap<User, UserListDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.ToString()));

        // User mappings - Detail DTO (includes permissions for GetUser)
        CreateMap<User, UserDetailDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.SupervisorName, opt => opt.MapFrom(src =>
                src.Supervisor != null ? src.Supervisor.FullNameArabic : null))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                src.Permissions.Where(p => p.IsActive).Select(p => p.Permission.ToString()).ToList()))
            .ForMember(dest => dest.ActivePermissionsCount, opt => opt.MapFrom(src =>
                src.Permissions.Count(p => p.IsActive)));
        // AuditLog mappings (for GetUserAuditLog)
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.ActionType.ToString()))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.ActionDescription))
            .ForMember(dest => dest.Changes, opt => opt.MapFrom(src =>
                !string.IsNullOrWhiteSpace(src.ChangedFields)
                    ? $"Changed Fields: {src.ChangedFields}"
                    : (!string.IsNullOrWhiteSpace(src.OldValues) || !string.IsNullOrWhiteSpace(src.NewValues))
                        ? $"{(string.IsNullOrWhiteSpace(src.OldValues) ? "null" : "old")} => {(string.IsNullOrWhiteSpace(src.NewValues) ? "null" : "new")}"
                        : src.ActionDescription
            ));
        // Survey mappings
        CreateMap<Survey, SurveyDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src =>
                src.Building != null ? src.Building.BuildingNumber : null))
            .ForMember(dest => dest.BuildingAddress, opt => opt.MapFrom(src =>
                src.Building != null ? src.Building.Address : null))
            .ForMember(dest => dest.UnitIdentifier, opt => opt.MapFrom(src =>
                src.PropertyUnit != null ? src.PropertyUnit.UnitIdentifier : null))
            .ForMember(dest => dest.FieldCollectorName, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByName, opt => opt.Ignore());

        // Office Survey Detail mappings
        CreateMap<Survey, OfficeSurveyDetailDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src =>
                src.Building != null ? src.Building.BuildingNumber : null))
            .ForMember(dest => dest.BuildingAddress, opt => opt.MapFrom(src =>
                src.Building != null ? src.Building.Address : null))
            .ForMember(dest => dest.UnitIdentifier, opt => opt.MapFrom(src =>
                src.PropertyUnit != null ? src.PropertyUnit.UnitIdentifier : null))
            .ForMember(dest => dest.FieldCollectorName, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByName, opt => opt.Ignore())
            // Office-specific fields
            .ForMember(dest => dest.OfficeLocation, opt => opt.MapFrom(src => src.OfficeLocation))
            .ForMember(dest => dest.RegistrationNumber, opt => opt.MapFrom(src => src.RegistrationNumber))
            .ForMember(dest => dest.AppointmentReference, opt => opt.MapFrom(src => src.AppointmentReference))
            .ForMember(dest => dest.ContactPhone, opt => opt.MapFrom(src => src.ContactPhone))
            .ForMember(dest => dest.ContactEmail, opt => opt.MapFrom(src => src.ContactEmail))
            .ForMember(dest => dest.InPersonVisit, opt => opt.MapFrom(src => src.InPersonVisit))
            // Claim linking
            .ForMember(dest => dest.ClaimId, opt => opt.MapFrom(src => src.ClaimId))
            .ForMember(dest => dest.ClaimCreatedDate, opt => opt.MapFrom(src => src.ClaimCreatedDate))
            .ForMember(dest => dest.ClaimNumber, opt => opt.Ignore()) // Set manually in handler
                                                                      // Related data - set manually in handler
            .ForMember(dest => dest.Households, opt => opt.Ignore())
            .ForMember(dest => dest.Relations, opt => opt.Ignore())
            .ForMember(dest => dest.Evidence, opt => opt.Ignore())
            .ForMember(dest => dest.DataSummary, opt => opt.Ignore());

            // ImportPackage
        CreateMap<ImportPackage, ImportPackageDto>()
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
           .ForMember(dest => dest.SuccessRate, opt => opt.MapFrom(src => src.GetSuccessRate()));


    }

}