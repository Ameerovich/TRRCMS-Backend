using AutoMapper;
using TRRCMS.Application.AdministrativeDivisions.Dtos;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Application.Landmarks.Dtos;
using TRRCMS.Application.Streets.Dtos;
using TRRCMS.Application.Users.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;

namespace TRRCMS.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Building mappings
        CreateMap<Building, BuildingDto>()
           .ForMember(dest => dest.BuildingType, opt => opt.MapFrom(src => (int)src.BuildingType))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
           .ForMember(dest => dest.BuildingDocumentIds, opt => opt.MapFrom(src => src.BuildingDocuments.Select(d => d.Id).ToList()));

        // BuildingDocument mappings
        CreateMap<BuildingDocument, BuildingDocumentDto>();

        // PropertyUnit mapping

        CreateMap<PropertyUnit, PropertyUnitDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.BuildingId, opt => opt.MapFrom(src => src.BuildingId))
            .ForMember(dest => dest.BuildingNumber, opt => opt.Ignore()) // Set in handler
            .ForMember(dest => dest.UnitIdentifier, opt => opt.MapFrom(src => src.UnitIdentifier))
            .ForMember(dest => dest.FloorNumber, opt => opt.MapFrom(src => src.FloorNumber))
            .ForMember(dest => dest.UnitType, opt => opt.MapFrom(src => (int)src.UnitType))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
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
                    .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.HasValue ? (int?)src.Gender : null))
                    .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality.HasValue ? (int?)src.Nationality : null))
                    .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))

                    // Contact information
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                    .ForMember(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.MobileNumber))
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))

                    // Contact person
                    .ForMember(dest => dest.IsContactPerson, opt => opt.MapFrom(src => src.IsContactPerson))

                    // Household context
                    .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.HouseholdId))
                    .ForMember(dest => dest.RelationshipToHead, opt => opt.MapFrom(src => src.RelationshipToHead.HasValue ? (int?)src.RelationshipToHead : null))

                    // Audit fields
                    .ForMember(dest => dest.CreatedAtUtc, opt => opt.MapFrom(src => src.CreatedAtUtc))
                    .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                    .ForMember(dest => dest.LastModifiedAtUtc, opt => opt.MapFrom(src => src.LastModifiedAtUtc))
                    .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.LastModifiedBy))
                    .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                    .ForMember(dest => dest.DeletedAtUtc, opt => opt.MapFrom(src => src.DeletedAtUtc))
                    .ForMember(dest => dest.DeletedBy, opt => opt.MapFrom(src => src.DeletedBy));

        // ─────────────────────────────────────────────────────────────────────
        // Staging → production DTO mappings.
        // Used by the conflict-resolution review endpoint to render staging rows
        // with the same shape as their production counterparts so the UI can
        // diff field-by-field with a single per-entity-type renderer.
        // ─────────────────────────────────────────────────────────────────────

        CreateMap<StagingPerson, PersonDto>()
            // OriginalEntityId is the logical identity that becomes the production Id on commit.
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OriginalEntityId))
            // OriginalHouseholdId is the staging UUID — frontend renders as a reference.
            .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.OriginalHouseholdId))
            // Staging records use StagedAtUtc instead of CreatedAtUtc/By.
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.MapFrom(src => src.StagedAtUtc))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedAtUtc, opt => opt.MapFrom(src => (DateTime?)null))
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.DeletedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.HasValue ? (int?)src.Gender : null))
            .ForMember(dest => dest.Nationality, opt => opt.MapFrom(src => src.Nationality.HasValue ? (int?)src.Nationality : null))
            .ForMember(dest => dest.RelationshipToHead, opt => opt.MapFrom(src => src.RelationshipToHead.HasValue ? (int?)src.RelationshipToHead : null));

        CreateMap<StagingPropertyUnit, PropertyUnitDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OriginalEntityId))
            // OriginalBuildingId is the staging UUID — frontend renders as a reference.
            .ForMember(dest => dest.BuildingId, opt => opt.MapFrom(src => src.OriginalBuildingId))
            .ForMember(dest => dest.BuildingNumber, opt => opt.Ignore())
            .ForMember(dest => dest.UnitType, opt => opt.MapFrom(src => (int)src.UnitType))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.MapFrom(src => src.StagedAtUtc))
            .ForMember(dest => dest.LastModifiedAtUtc, opt => opt.MapFrom(src => (DateTime?)null));

        CreateMap<StagingBuilding, BuildingDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OriginalEntityId))
            // BuildingId is nullable on staging (computed from admin codes during commit) but
            // non-nullable on the DTO — coalesce to empty string to satisfy the contract.
            .ForMember(dest => dest.BuildingId, opt => opt.MapFrom(src => src.BuildingId ?? string.Empty))
            .ForMember(dest => dest.GovernorateName, opt => opt.MapFrom(src => src.GovernorateName ?? string.Empty))
            .ForMember(dest => dest.DistrictName, opt => opt.MapFrom(src => src.DistrictName ?? string.Empty))
            .ForMember(dest => dest.SubDistrictName, opt => opt.MapFrom(src => src.SubDistrictName ?? string.Empty))
            .ForMember(dest => dest.CommunityName, opt => opt.MapFrom(src => src.CommunityName ?? string.Empty))
            .ForMember(dest => dest.NeighborhoodName, opt => opt.MapFrom(src => src.NeighborhoodName ?? string.Empty))
            .ForMember(dest => dest.BuildingType, opt => opt.MapFrom(src => (int)src.BuildingType))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.BuildingDocumentIds, opt => opt.MapFrom(src => new List<Guid>()))
            .ForMember(dest => dest.IsAssigned, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.MapFrom(src => src.StagedAtUtc))
            .ForMember(dest => dest.LastModifiedAtUtc, opt => opt.MapFrom(src => (DateTime?)null));

        CreateMap<Household, HouseholdDto>()
       // Identifiers
       .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
       .ForMember(dest => dest.PropertyUnitId, opt => opt.MapFrom(src => src.PropertyUnitId))
       .ForMember(dest => dest.PropertyUnitIdentifier, opt => opt.Ignore()) // Set in handler

       // Basic info
       .ForMember(dest => dest.HouseholdSize, opt => opt.MapFrom(src => src.HouseholdSize))
       .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))

       // Occupancy information
       .ForMember(dest => dest.OccupancyNature, opt => opt.MapFrom(src => src.OccupancyNature.HasValue ? (int?)src.OccupancyNature : null))
       .ForMember(dest => dest.OccupancyStartDate, opt => opt.MapFrom(src => src.OccupancyStartDate))

       // Composition (canonical v1.9 — ungendered)
       .ForMember(dest => dest.MaleCount, opt => opt.MapFrom(src => src.MaleCount))
       .ForMember(dest => dest.FemaleCount, opt => opt.MapFrom(src => src.FemaleCount))
       .ForMember(dest => dest.AdultCount, opt => opt.MapFrom(src => src.AdultCount))
       .ForMember(dest => dest.ChildCount, opt => opt.MapFrom(src => src.ChildCount))
       .ForMember(dest => dest.ElderlyCount, opt => opt.MapFrom(src => src.ElderlyCount))
       .ForMember(dest => dest.DisabledCount, opt => opt.MapFrom(src => src.DisabledCount))

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
            .ForMember(dest => dest.RelationType, opt => opt.MapFrom(src => (int)src.RelationType))
            .ForMember(dest => dest.OccupancyType, opt => opt.MapFrom(src => src.OccupancyType.HasValue ? (int?)src.OccupancyType : null))
            .ForMember(dest => dest.HasEvidence, opt => opt.MapFrom(src => src.HasEvidence))
            .ForMember(dest => dest.IsOngoing, opt => opt.MapFrom(src => src.IsActive));

        // Evidence mappings
        CreateMap<Evidence, EvidenceDto>()
            .ForMember(dest => dest.EvidenceType, opt => opt.MapFrom(src => (int)src.EvidenceType))
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired()))
            .ForMember(dest => dest.EvidenceRelations, opt => opt.MapFrom(src => src.EvidenceRelations));

        // EvidenceRelation mappings (many-to-many join entity)
        CreateMap<EvidenceRelation, EvidenceRelationDto>();

        // IdentificationDocument mappings
        CreateMap<IdentificationDocument, TRRCMS.Application.IdentificationDocuments.Dtos.IdentificationDocumentDto>()
            .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => src.DocumentType.HasValue ? (int?)src.DocumentType.Value : null))
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired()));

        // Claim mappings
        CreateMap<Claim, TRRCMS.Application.Claims.Dtos.ClaimDto>()
            .ForMember(dest => dest.ClaimType, opt => opt.MapFrom(src => (int)src.ClaimType))
            .ForMember(dest => dest.ClaimSource, opt => opt.MapFrom(src => (int)src.ClaimSource))
            .ForMember(dest => dest.CaseStatus, opt => opt.MapFrom(src => (int)src.CaseStatus))
            .ForMember(dest => dest.TenureContractType, opt => opt.MapFrom(src => src.TenureContractType.HasValue ? (int?)src.TenureContractType : null))
            .ForMember(dest => dest.HasEvidence, opt => opt.MapFrom(src => src.Evidences != null && src.Evidences.Any()))
            .ForMember(dest => dest.PropertyUnitCode, opt => opt.MapFrom(src =>
                src.PropertyUnit != null ? src.PropertyUnit.UnitIdentifier : null))
            .ForMember(dest => dest.PrimaryClaimantName, opt => opt.MapFrom(src =>
                src.PrimaryClaimant != null ? $"{src.PrimaryClaimant.FirstNameArabic} {src.PrimaryClaimant.FatherNameArabic} {src.PrimaryClaimant.FamilyNameArabic}" : null))
            .ForMember(dest => dest.EvidenceIds, opt => opt.Ignore())
            .ForMember(dest => dest.SourceRelationId, opt => opt.Ignore());

        // User mappings - Base DTO
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => (int)src.Role))
            .ForMember(dest => dest.SupervisorName, opt => opt.MapFrom(src =>
                src.Supervisor != null ? src.Supervisor.FullNameArabic : null));

        // User mappings - List DTO (lightweight for GetAllUsers)
        CreateMap<User, UserListDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => (int)src.Role));

        // User mappings - Detail DTO (includes permissions for GetUser)
        CreateMap<User, UserDetailDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => (int)src.Role))
            .ForMember(dest => dest.SupervisorName, opt => opt.MapFrom(src =>
                src.Supervisor != null ? src.Supervisor.FullNameArabic : null))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                src.Permissions.Where(p => p.IsActive).Select(p => (int)p.Permission).ToList()))
            .ForMember(dest => dest.ActivePermissionsCount, opt => opt.MapFrom(src =>
                src.Permissions.Count(p => p.IsActive)));
        // AuditLog mappings (for GetUserAuditLog)
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => (int)src.ActionType))
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
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.SurveyType, opt => opt.MapFrom(src => (int)src.Type))
            .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src =>
                src.Building != null ? src.Building.BuildingNumber : null))
            .ForMember(dest => dest.UnitIdentifier, opt => opt.MapFrom(src =>
                src.PropertyUnit != null ? src.PropertyUnit.UnitIdentifier : null))
            .ForMember(dest => dest.FieldCollectorName, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByName, opt => opt.Ignore())
            .ForMember(dest => dest.ContactPersonId, opt => opt.MapFrom(src => src.ContactPersonId))
            .ForMember(dest => dest.ContactPersonFullName, opt => opt.MapFrom(src => src.ContactPersonFullName));

        // Office Survey Detail mappings
        CreateMap<Survey, OfficeSurveyDetailDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src =>
                src.Building != null ? src.Building.BuildingNumber : null))
            .ForMember(dest => dest.UnitIdentifier, opt => opt.MapFrom(src =>
                src.PropertyUnit != null ? src.PropertyUnit.UnitIdentifier : null))
            .ForMember(dest => dest.FieldCollectorName, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByName, opt => opt.Ignore())
            .ForMember(dest => dest.ContactPersonId, opt => opt.MapFrom(src => src.ContactPersonId))
            .ForMember(dest => dest.ContactPersonFullName, opt => opt.MapFrom(src => src.ContactPersonFullName))
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
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
           .ForMember(dest => dest.SuccessRate, opt => opt.MapFrom(src => src.GetSuccessRate()));

        // Administrative Hierarchy mappings
        CreateMap<Governorate, GovernorateDto>();
        CreateMap<District, DistrictDto>();
        CreateMap<SubDistrict, SubDistrictDto>();
        CreateMap<Community, CommunityDto>();

        // Landmark mappings
        CreateMap<Landmark, LandmarkDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<Landmark, LandmarkMapDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()));

        // Landmark type icon mappings
        CreateMap<LandmarkTypeIcon, LandmarkTypeIconDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type.ToString()));

        // Street mappings
        CreateMap<Street, StreetDto>()
            .ForMember(dest => dest.GeometryWkt, opt => opt.MapFrom(src => src.GeometryWkt));

        CreateMap<Street, StreetMapDto>()
            .ForMember(dest => dest.GeometryWkt, opt => opt.MapFrom(src => src.GeometryWkt));
    }

}