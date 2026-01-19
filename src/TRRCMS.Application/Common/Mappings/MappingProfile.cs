using AutoMapper;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Application.Documents.Dtos;
using TRRCMS.Application.Users.Dtos;

namespace TRRCMS.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Building mappings
        CreateMap<Building, BuildingDto>()
            .ForMember(dest => dest.BuildingType, opt => opt.MapFrom(src => src.BuildingType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DamageLevel, opt => opt.MapFrom(src => src.DamageLevel.HasValue ? src.DamageLevel.ToString() : null));

        // PropertyUnit mappings
        CreateMap<PropertyUnit, PropertyUnitDto>()
            .ForMember(dest => dest.UnitType, opt => opt.MapFrom(src => src.UnitType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DamageLevel, opt => opt.MapFrom(src => src.DamageLevel.HasValue ? src.DamageLevel.ToString() : null))
            .ForMember(dest => dest.OccupancyType, opt => opt.MapFrom(src => src.OccupancyType.HasValue ? src.OccupancyType.ToString() : null))
            .ForMember(dest => dest.OccupancyNature, opt => opt.MapFrom(src => src.OccupancyNature.HasValue ? src.OccupancyNature.ToString() : null));

        // Person mappings
        CreateMap<Person, PersonDto>();

        // Household mappings
        CreateMap<Household, HouseholdDto>()
            .ForMember(dest => dest.DependencyRatio, opt => opt.MapFrom(src => src.CalculateDependencyRatio()))
            .ForMember(dest => dest.IsVulnerable, opt => opt.MapFrom(src => src.IsVulnerable()));

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

    }

}