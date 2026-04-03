CREATE TABLE "Buildings" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "BuildingId" varchar(17) UNIQUE NOT NULL,
  "GovernorateCode" varchar(2) NOT NULL,
  "DistrictCode" varchar(2) NOT NULL,
  "SubDistrictCode" varchar(2) NOT NULL,
  "CommunityCode" varchar(3) NOT NULL,
  "NeighborhoodCode" varchar(3) NOT NULL,
  "BuildingNumber" varchar(5) NOT NULL,
  "GovernorateName" varchar NOT NULL,
  "DistrictName" varchar NOT NULL,
  "SubDistrictName" varchar NOT NULL,
  "CommunityName" varchar NOT NULL,
  "NeighborhoodName" varchar NOT NULL,
  "BuildingType" int NOT NULL,
  "Status" int NOT NULL,
  "NumberOfPropertyUnits" int NOT NULL,
  "NumberOfApartments" int NOT NULL,
  "NumberOfShops" int NOT NULL,
  "BuildingGeometry" geometry,
  "Latitude" decimal,
  "Longitude" decimal,
  "Notes" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "BuildingDocuments" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "BuildingId" uuid NOT NULL,
  "Description" text,
  "OriginalFileName" varchar NOT NULL,
  "FilePath" varchar NOT NULL,
  "FileSizeBytes" bigint NOT NULL,
  "MimeType" varchar NOT NULL,
  "FileHash" varchar(64),
  "Notes" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "PropertyUnits" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "BuildingId" uuid NOT NULL,
  "UnitIdentifier" varchar NOT NULL,
  "FloorNumber" int,
  "UnitType" int NOT NULL,
  "Status" int NOT NULL,
  "AreaSquareMeters" decimal,
  "NumberOfRooms" int,
  "Description" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "Persons" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "FamilyNameArabic" varchar,
  "FirstNameArabic" varchar,
  "FatherNameArabic" varchar,
  "MotherNameArabic" varchar,
  "NationalId" varchar,
  "DateOfBirth" timestamp,
  "Gender" int,
  "Nationality" int,
  "Email" varchar,
  "MobileNumber" varchar,
  "PhoneNumber" varchar,
  "IsContactPerson" boolean NOT NULL DEFAULT false,
  "HouseholdId" uuid,
  "RelationshipToHead" int,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "Households" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "PropertyUnitId" uuid NOT NULL,
  "HeadOfHouseholdPersonId" uuid,
  "HeadOfHouseholdName" varchar,
  "HouseholdSize" int,
  "OccupancyType" int,
  "OccupancyNature" int,
  "MaleCount" int,
  "FemaleCount" int,
  "MaleChildCount" int,
  "FemaleChildCount" int,
  "MaleElderlyCount" int,
  "FemaleElderlyCount" int,
  "MaleDisabledCount" int,
  "FemaleDisabledCount" int,
  "ChildCount" int NOT NULL DEFAULT 0,
  "ElderlyCount" int NOT NULL DEFAULT 0,
  "PersonsWithDisabilitiesCount" int NOT NULL DEFAULT 0,
  "Notes" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "PersonPropertyRelations" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "PersonId" uuid NOT NULL,
  "PropertyUnitId" uuid NOT NULL,
  "SurveyId" uuid,
  "RelationType" int NOT NULL,
  "OccupancyType" int,
  "HasEvidence" boolean NOT NULL DEFAULT false,
  "OwnershipShare" decimal,
  "ContractDetails" text,
  "Notes" text,
  "IsActive" boolean NOT NULL DEFAULT true,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "Claims" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "ClaimNumber" varchar UNIQUE NOT NULL,
  "PropertyUnitId" uuid NOT NULL,
  "PrimaryClaimantId" uuid,
  "OriginatingSurveyId" uuid,
  "ClaimType" int NOT NULL,
  "ClaimSource" int NOT NULL,
  "CaseStatus" int NOT NULL,
  "TenureContractType" int,
  "OwnershipShare" int,
  "SubmittedDate" timestamp,
  "SubmittedByUserId" uuid,
  "ClaimDescription" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "Evidences" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "EvidenceType" int NOT NULL,
  "Description" text NOT NULL,
  "OriginalFileName" varchar NOT NULL,
  "FilePath" varchar NOT NULL,
  "FileSizeBytes" bigint NOT NULL,
  "MimeType" varchar NOT NULL,
  "FileHash" varchar(64),
  "DocumentIssuedDate" timestamp,
  "DocumentExpiryDate" timestamp,
  "IssuingAuthority" varchar,
  "DocumentReferenceNumber" varchar,
  "Notes" text,
  "VersionNumber" int NOT NULL DEFAULT 1,
  "PreviousVersionId" uuid,
  "IsCurrentVersion" boolean NOT NULL DEFAULT true,
  "PersonId" uuid,
  "ClaimId" uuid,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "EvidenceRelations" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "EvidenceId" uuid NOT NULL,
  "PersonPropertyRelationId" uuid NOT NULL,
  "LinkReason" text,
  "LinkedAtUtc" timestamp NOT NULL,
  "LinkedBy" uuid NOT NULL,
  "IsActive" boolean NOT NULL DEFAULT true,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "Surveys" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "ReferenceCode" varchar UNIQUE NOT NULL,
  "BuildingId" uuid NOT NULL,
  "PropertyUnitId" uuid,
  "FieldCollectorId" uuid NOT NULL,
  "Type" int NOT NULL,
  "Source" int NOT NULL,
  "SurveyDate" timestamp NOT NULL,
  "Status" int NOT NULL,
  "GpsCoordinates" varchar,
  "Notes" text,
  "DurationMinutes" int,
  "OfficeLocation" varchar,
  "RegistrationNumber" varchar,
  "AppointmentReference" varchar,
  "ContactPhone" varchar,
  "ContactEmail" varchar,
  "InPersonVisit" boolean,
  "ClaimId" uuid,
  "ClaimCreatedDate" timestamp,
  "ContactPersonId" uuid,
  "ContactPersonFullName" varchar,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "BuildingAssignments" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "BuildingId" uuid NOT NULL,
  "FieldCollectorId" uuid NOT NULL,
  "AssignedByUserId" uuid,
  "AssignedDate" timestamp NOT NULL,
  "TargetCompletionDate" timestamp,
  "ActualCompletionDate" timestamp,
  "TransferStatus" int NOT NULL,
  "TransferredToTabletDate" timestamp,
  "SynchronizedFromTabletDate" timestamp,
  "UnitsForRevisit" jsonb,
  "RevisitReason" text,
  "IsRevisit" boolean NOT NULL DEFAULT false,
  "OriginalAssignmentId" uuid,
  "Priority" varchar NOT NULL DEFAULT 'Normal',
  "AssignmentNotes" text,
  "IsActive" boolean NOT NULL DEFAULT true,
  "TotalPropertyUnits" int NOT NULL,
  "CompletedPropertyUnits" int NOT NULL DEFAULT 0,
  "TransferErrorMessage" text,
  "TransferRetryCount" int NOT NULL DEFAULT 0,
  "LastTransferAttemptDate" timestamp,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "SyncSessions" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "FieldCollectorId" uuid NOT NULL,
  "DeviceId" varchar NOT NULL,
  "ServerIpAddress" varchar,
  "SessionStatus" int NOT NULL,
  "StartedAtUtc" timestamp NOT NULL,
  "CompletedAtUtc" timestamp,
  "PackagesUploaded" int NOT NULL DEFAULT 0,
  "PackagesFailed" int NOT NULL DEFAULT 0,
  "AssignmentsDownloaded" int NOT NULL DEFAULT 0,
  "AssignmentsAcknowledged" int NOT NULL DEFAULT 0,
  "VocabularyVersionsSent" jsonb,
  "ErrorMessage" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "ImportPackages" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "PackageId" uuid NOT NULL,
  "PackageNumber" varchar UNIQUE NOT NULL,
  "FileName" varchar NOT NULL,
  "FileSizeBytes" bigint NOT NULL,
  "PackageCreatedDate" timestamp NOT NULL,
  "PackageExportedDate" timestamp NOT NULL,
  "ExportedByUserId" uuid NOT NULL,
  "DeviceId" varchar,
  "Status" int NOT NULL,
  "ImportedDate" timestamp,
  "ImportedByUserId" uuid,
  "ValidationStartedDate" timestamp,
  "ValidationCompletedDate" timestamp,
  "CommittedDate" timestamp,
  "CommittedByUserId" uuid,
  "Checksum" varchar(64) NOT NULL,
  "DigitalSignature" text,
  "IsSignatureValid" boolean NOT NULL DEFAULT false,
  "IsChecksumValid" boolean NOT NULL DEFAULT false,
  "SurveyCount" int NOT NULL DEFAULT 0,
  "BuildingCount" int NOT NULL DEFAULT 0,
  "PropertyUnitCount" int NOT NULL DEFAULT 0,
  "PersonCount" int NOT NULL DEFAULT 0,
  "ClaimCount" int NOT NULL DEFAULT 0,
  "DocumentCount" int NOT NULL DEFAULT 0,
  "TotalAttachmentSizeBytes" bigint NOT NULL DEFAULT 0,
  "VocabularyVersions" jsonb,
  "IsVocabularyCompatible" boolean NOT NULL DEFAULT true,
  "VocabularyCompatibilityIssues" text,
  "SchemaVersion" varchar,
  "IsSchemaValid" boolean NOT NULL DEFAULT false,
  "ValidationErrors" jsonb,
  "ValidationWarnings" jsonb,
  "ValidationErrorCount" int NOT NULL DEFAULT 0,
  "ValidationWarningCount" int NOT NULL DEFAULT 0,
  "PersonDuplicateCount" int NOT NULL DEFAULT 0,
  "PropertyDuplicateCount" int NOT NULL DEFAULT 0,
  "ConflictCount" int NOT NULL DEFAULT 0,
  "AreConflictsResolved" boolean NOT NULL DEFAULT false,
  "SuccessfulImportCount" int NOT NULL DEFAULT 0,
  "FailedImportCount" int NOT NULL DEFAULT 0,
  "SkippedRecordCount" int NOT NULL DEFAULT 0,
  "ImportSummary" text,
  "CommitReportJson" jsonb,
  "ErrorMessage" text,
  "ErrorLog" jsonb,
  "ArchivePath" varchar,
  "IsArchived" boolean NOT NULL DEFAULT false,
  "ArchivedDate" timestamp,
  "UploadedFilePath" varchar,
  "ProcessingNotes" text,
  "ImportMethod" varchar,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "ConflictResolutions" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "ConflictNumber" varchar UNIQUE NOT NULL,
  "ConflictType" varchar NOT NULL,
  "EntityType" varchar NOT NULL,
  "FirstEntityId" uuid NOT NULL,
  "SecondEntityId" uuid NOT NULL,
  "FirstEntityIdentifier" varchar,
  "SecondEntityIdentifier" varchar,
  "ImportPackageId" uuid,
  "SimilarityScore" decimal NOT NULL,
  "ConfidenceLevel" varchar NOT NULL,
  "ConflictDescription" text NOT NULL,
  "MatchingCriteria" jsonb,
  "DataComparison" jsonb,
  "Status" varchar NOT NULL DEFAULT 'PendingReview',
  "ResolutionAction" int,
  "DetectedDate" timestamp NOT NULL,
  "DetectedByUserId" uuid,
  "AssignedDate" timestamp,
  "AssignedToUserId" uuid,
  "ResolvedDate" timestamp,
  "ResolvedByUserId" uuid,
  "ResolutionReason" text,
  "ResolutionNotes" text,
  "MergedEntityId" uuid,
  "DiscardedEntityId" uuid,
  "MergeMapping" jsonb,
  "Priority" varchar NOT NULL DEFAULT 'Normal',
  "TargetResolutionHours" int,
  "IsOverdue" boolean NOT NULL DEFAULT false,
  "IsAutoDetected" boolean NOT NULL DEFAULT false,
  "IsAutoResolved" boolean NOT NULL DEFAULT false,
  "AutoResolutionRule" varchar,
  "IsEscalated" boolean NOT NULL DEFAULT false,
  "EscalationReason" text,
  "EscalatedDate" timestamp,
  "EscalatedByUserId" uuid,
  "ReviewAttemptCount" int NOT NULL DEFAULT 0,
  "ReviewHistory" jsonb,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "Governorates" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "Code" varchar(2) UNIQUE NOT NULL,
  "NameArabic" varchar NOT NULL,
  "NameEnglish" varchar NOT NULL,
  "IsActive" boolean NOT NULL DEFAULT true,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "Districts" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "Code" varchar(2) NOT NULL,
  "GovernorateCode" varchar(2) NOT NULL,
  "NameArabic" varchar NOT NULL,
  "NameEnglish" varchar NOT NULL,
  "IsActive" boolean NOT NULL DEFAULT true,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid,
  UNIQUE ("GovernorateCode", "Code")
);

CREATE TABLE "SubDistricts" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "Code" varchar(2) NOT NULL,
  "GovernorateCode" varchar(2) NOT NULL,
  "DistrictCode" varchar(2) NOT NULL,
  "NameArabic" varchar NOT NULL,
  "NameEnglish" varchar NOT NULL,
  "IsActive" boolean NOT NULL DEFAULT true,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid,
  UNIQUE ("GovernorateCode", "DistrictCode", "Code")
);

CREATE TABLE "Communities" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "Code" varchar(3) NOT NULL,
  "GovernorateCode" varchar(2) NOT NULL,
  "DistrictCode" varchar(2) NOT NULL,
  "SubDistrictCode" varchar(2) NOT NULL,
  "NameArabic" varchar NOT NULL,
  "NameEnglish" varchar NOT NULL,
  "IsActive" boolean NOT NULL DEFAULT true,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid,
  UNIQUE ("GovernorateCode", "DistrictCode", "SubDistrictCode", "Code")
);

CREATE TABLE "Neighborhoods" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "GovernorateCode" varchar(2) NOT NULL,
  "DistrictCode" varchar(2) NOT NULL,
  "SubDistrictCode" varchar(2) NOT NULL,
  "CommunityCode" varchar(3) NOT NULL,
  "NeighborhoodCode" varchar(3) NOT NULL,
  "FullCode" varchar(12) UNIQUE NOT NULL,
  "NameArabic" varchar NOT NULL,
  "NameEnglish" varchar,
  "CenterPoint" geometry,
  "CenterLatitude" decimal NOT NULL,
  "CenterLongitude" decimal NOT NULL,
  "BoundaryGeometry" geometry,
  "AreaSquareKm" double precision,
  "ZoomLevel" int NOT NULL,
  "IsActive" boolean NOT NULL DEFAULT true,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "Landmarks" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "Identifier" int NOT NULL,
  "Name" varchar NOT NULL,
  "Type" int NOT NULL,
  "Location" geometry,
  "Latitude" decimal NOT NULL,
  "Longitude" decimal NOT NULL,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "LandmarkTypeIcons" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "Type" int UNIQUE NOT NULL,
  "SvgContent" text NOT NULL,
  "DisplayNameArabic" varchar NOT NULL,
  "DisplayNameEnglish" varchar NOT NULL,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "Streets" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "Identifier" int NOT NULL,
  "Name" varchar NOT NULL,
  "Geometry" geometry,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "Users" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "Username" varchar UNIQUE NOT NULL,
  "Email" varchar UNIQUE,
  "PasswordHash" varchar NOT NULL,
  "PasswordSalt" varchar NOT NULL,
  "FullNameArabic" varchar NOT NULL,
  "FullNameEnglish" varchar,
  "EmployeeId" varchar,
  "PhoneNumber" varchar,
  "Organization" varchar,
  "JobTitle" varchar,
  "Role" int NOT NULL,
  "AdditionalRoles" jsonb,
  "HasMobileAccess" boolean NOT NULL DEFAULT false,
  "HasDesktopAccess" boolean NOT NULL DEFAULT false,
  "IsActive" boolean NOT NULL DEFAULT true,
  "IsLockedOut" boolean NOT NULL DEFAULT false,
  "LockoutEndDate" timestamp,
  "FailedLoginAttempts" int NOT NULL DEFAULT 0,
  "LastFailedLoginDate" timestamp,
  "LastLoginDate" timestamp,
  "LastPasswordChangeDate" timestamp,
  "MustChangePassword" boolean NOT NULL DEFAULT false,
  "PasswordExpiryDate" timestamp,
  "AssignedTabletId" varchar,
  "TabletAssignedDate" timestamp,
  "IsAvailable" boolean NOT NULL DEFAULT true,
  "SupervisorUserId" uuid,
  "TeamName" varchar,
  "PreferredLanguage" varchar NOT NULL DEFAULT 'ar',
  "Preferences" jsonb,
  "SecurityStamp" varchar NOT NULL,
  "TwoFactorEnabled" boolean NOT NULL DEFAULT false,
  "RefreshToken" text,
  "RefreshTokenExpiryDate" timestamp,
  "Notes" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "UserPermissions" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "UserId" uuid NOT NULL,
  "Permission" int NOT NULL,
  "GrantReason" text,
  "GrantedAtUtc" timestamp NOT NULL,
  "GrantedBy" uuid NOT NULL,
  "ExpiresAtUtc" timestamp,
  "IsActive" boolean NOT NULL DEFAULT true,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "AuditLogs" (
  "Id" uuid PRIMARY KEY,
  "AuditLogNumber" BIGINT GENERATED BY DEFAULT AS IDENTITY UNIQUE NOT NULL,
  "Timestamp" timestamp NOT NULL,
  "ActionType" int NOT NULL,
  "ActionDescription" varchar NOT NULL,
  "ActionResult" varchar NOT NULL DEFAULT 'Success',
  "UserId" uuid NOT NULL,
  "Username" varchar NOT NULL,
  "UserRole" varchar NOT NULL,
  "UserFullName" varchar NOT NULL,
  "EntityType" varchar,
  "EntityId" uuid,
  "EntityIdentifier" varchar,
  "OldValues" jsonb,
  "NewValues" jsonb,
  "ChangedFields" jsonb,
  "IpAddress" varchar,
  "UserAgent" varchar,
  "SourceApplication" varchar,
  "DeviceId" varchar,
  "SessionId" varchar,
  "AdditionalData" jsonb,
  "ErrorMessage" text,
  "StackTrace" text,
  "CorrelationId" uuid,
  "ParentAuditLogId" uuid,
  "IsSecuritySensitive" boolean NOT NULL DEFAULT false,
  "RequiresLegalRetention" boolean NOT NULL DEFAULT false,
  "RetentionEndDate" timestamp
);

CREATE TABLE "Vocabularies" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "VocabularyName" varchar NOT NULL,
  "DisplayNameArabic" varchar NOT NULL,
  "DisplayNameEnglish" varchar,
  "Description" text,
  "Version" varchar NOT NULL,
  "MajorVersion" int NOT NULL,
  "MinorVersion" int NOT NULL,
  "PatchVersion" int NOT NULL,
  "VersionDate" timestamp NOT NULL,
  "IsCurrentVersion" boolean NOT NULL DEFAULT true,
  "PreviousVersionId" uuid,
  "ValuesJson" jsonb NOT NULL,
  "ValueCount" int NOT NULL DEFAULT 0,
  "Category" varchar,
  "IsSystemVocabulary" boolean NOT NULL DEFAULT false,
  "AllowCustomValues" boolean NOT NULL DEFAULT false,
  "IsMandatory" boolean NOT NULL DEFAULT false,
  "IsActive" boolean NOT NULL DEFAULT true,
  "MinimumCompatibleVersion" varchar,
  "ChangeLog" text,
  "LastUsedDate" timestamp,
  "UsageCount" int NOT NULL DEFAULT 0,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "SecurityPolicies" (
  "Id" uuid PRIMARY KEY,
  "RowVersion" bytea,
  "Version" int UNIQUE NOT NULL,
  "IsActive" boolean NOT NULL DEFAULT false,
  "EffectiveFromUtc" timestamp NOT NULL,
  "EffectiveToUtc" timestamp,
  "Password_MinLength" int NOT NULL DEFAULT 8,
  "Password_RequireUppercase" boolean NOT NULL DEFAULT true,
  "Password_RequireLowercase" boolean NOT NULL DEFAULT true,
  "Password_RequireDigit" boolean NOT NULL DEFAULT true,
  "Password_RequireSpecialCharacter" boolean NOT NULL DEFAULT true,
  "Password_ExpiryDays" int NOT NULL DEFAULT 90,
  "Password_ReuseHistory" int NOT NULL DEFAULT 5,
  "Session_TimeoutMinutes" int NOT NULL DEFAULT 30,
  "Session_MaxFailedLoginAttempts" int NOT NULL DEFAULT 5,
  "Session_LockoutDurationMinutes" int NOT NULL DEFAULT 15,
  "Access_AllowPasswordAuth" boolean NOT NULL DEFAULT true,
  "Access_AllowSsoAuth" boolean NOT NULL DEFAULT false,
  "Access_AllowTokenAuth" boolean NOT NULL DEFAULT true,
  "Access_EnforceIpAllowlist" boolean NOT NULL DEFAULT false,
  "Access_IpAllowlist" varchar(2000),
  "Access_IpDenylist" varchar(2000),
  "Access_RestrictByEnvironment" boolean NOT NULL DEFAULT false,
  "Access_AllowedEnvironments" varchar(500),
  "ChangeDescription" varchar(1000),
  "AppliedByUserId" uuid NOT NULL,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "StagingBuildings" (
  "Id" uuid PRIMARY KEY,
  "ImportPackageId" uuid NOT NULL,
  "OriginalEntityId" uuid,
  "StagingValidationStatus" int,
  "ValidationErrors" jsonb,
  "ValidationWarnings" jsonb,
  "IsApprovedForCommit" boolean NOT NULL DEFAULT false,
  "CommittedEntityId" uuid,
  "StagedAtUtc" timestamp,
  "BuildingId" varchar(17),
  "GovernorateCode" varchar(2) NOT NULL,
  "DistrictCode" varchar(2) NOT NULL,
  "SubDistrictCode" varchar(2) NOT NULL,
  "CommunityCode" varchar(3) NOT NULL,
  "NeighborhoodCode" varchar(3) NOT NULL,
  "BuildingNumber" varchar(5) NOT NULL,
  "GovernorateName" varchar,
  "DistrictName" varchar,
  "SubDistrictName" varchar,
  "CommunityName" varchar,
  "NeighborhoodName" varchar,
  "BuildingType" int NOT NULL,
  "Status" int NOT NULL,
  "NumberOfPropertyUnits" int,
  "NumberOfApartments" int,
  "NumberOfShops" int,
  "BuildingGeometryWkt" text,
  "Latitude" decimal,
  "Longitude" decimal,
  "Notes" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "StagingBuildingDocuments" (
  "Id" uuid PRIMARY KEY,
  "ImportPackageId" uuid NOT NULL,
  "OriginalEntityId" uuid,
  "StagingValidationStatus" int,
  "ValidationErrors" jsonb,
  "ValidationWarnings" jsonb,
  "IsApprovedForCommit" boolean NOT NULL DEFAULT false,
  "CommittedEntityId" uuid,
  "StagedAtUtc" timestamp,
  "OriginalBuildingId" uuid NOT NULL,
  "Description" text,
  "OriginalFileName" varchar NOT NULL,
  "FilePath" varchar NOT NULL,
  "FileSizeBytes" bigint NOT NULL,
  "MimeType" varchar NOT NULL,
  "FileHash" varchar(64),
  "Notes" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "StagingPropertyUnits" (
  "Id" uuid PRIMARY KEY,
  "ImportPackageId" uuid NOT NULL,
  "OriginalEntityId" uuid,
  "StagingValidationStatus" int,
  "ValidationErrors" jsonb,
  "ValidationWarnings" jsonb,
  "IsApprovedForCommit" boolean NOT NULL DEFAULT false,
  "CommittedEntityId" uuid,
  "StagedAtUtc" timestamp,
  "OriginalBuildingId" uuid NOT NULL,
  "UnitIdentifier" varchar NOT NULL,
  "UnitType" int NOT NULL,
  "Status" int NOT NULL,
  "FloorNumber" int,
  "NumberOfRooms" int,
  "AreaSquareMeters" decimal,
  "Description" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "StagingPersons" (
  "Id" uuid PRIMARY KEY,
  "ImportPackageId" uuid NOT NULL,
  "OriginalEntityId" uuid,
  "StagingValidationStatus" int,
  "ValidationErrors" jsonb,
  "ValidationWarnings" jsonb,
  "IsApprovedForCommit" boolean NOT NULL DEFAULT false,
  "CommittedEntityId" uuid,
  "StagedAtUtc" timestamp,
  "FamilyNameArabic" varchar NOT NULL,
  "FirstNameArabic" varchar NOT NULL,
  "FatherNameArabic" varchar NOT NULL,
  "MotherNameArabic" varchar,
  "NationalId" varchar,
  "DateOfBirth" timestamp,
  "Email" varchar,
  "MobileNumber" varchar,
  "PhoneNumber" varchar,
  "Gender" int,
  "Nationality" int,
  "OriginalHouseholdId" uuid,
  "RelationshipToHead" int,
  "IsContactPerson" boolean NOT NULL DEFAULT false,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "StagingHouseholds" (
  "Id" uuid PRIMARY KEY,
  "ImportPackageId" uuid NOT NULL,
  "OriginalEntityId" uuid,
  "StagingValidationStatus" int,
  "ValidationErrors" jsonb,
  "ValidationWarnings" jsonb,
  "IsApprovedForCommit" boolean NOT NULL DEFAULT false,
  "CommittedEntityId" uuid,
  "StagedAtUtc" timestamp,
  "OriginalPropertyUnitId" uuid NOT NULL,
  "OriginalHeadOfHouseholdPersonId" uuid,
  "HeadOfHouseholdName" varchar NOT NULL,
  "HouseholdSize" int NOT NULL,
  "MaleCount" int NOT NULL DEFAULT 0,
  "FemaleCount" int NOT NULL DEFAULT 0,
  "MaleChildCount" int NOT NULL DEFAULT 0,
  "FemaleChildCount" int NOT NULL DEFAULT 0,
  "MaleElderlyCount" int NOT NULL DEFAULT 0,
  "FemaleElderlyCount" int NOT NULL DEFAULT 0,
  "MaleDisabledCount" int NOT NULL DEFAULT 0,
  "FemaleDisabledCount" int NOT NULL DEFAULT 0,
  "Notes" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "StagingPersonPropertyRelations" (
  "Id" uuid PRIMARY KEY,
  "ImportPackageId" uuid NOT NULL,
  "OriginalEntityId" uuid,
  "StagingValidationStatus" int,
  "ValidationErrors" jsonb,
  "ValidationWarnings" jsonb,
  "IsApprovedForCommit" boolean NOT NULL DEFAULT false,
  "CommittedEntityId" uuid,
  "StagedAtUtc" timestamp,
  "OriginalPersonId" uuid NOT NULL,
  "OriginalPropertyUnitId" uuid NOT NULL,
  "RelationType" int NOT NULL,
  "OwnershipShare" decimal,
  "ContractDetails" text,
  "Notes" text,
  "IsActive" boolean NOT NULL DEFAULT true,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "StagingClaims" (
  "Id" uuid PRIMARY KEY,
  "ImportPackageId" uuid NOT NULL,
  "OriginalEntityId" uuid,
  "StagingValidationStatus" int,
  "ValidationErrors" jsonb,
  "ValidationWarnings" jsonb,
  "IsApprovedForCommit" boolean NOT NULL DEFAULT false,
  "CommittedEntityId" uuid,
  "StagedAtUtc" timestamp,
  "OriginalPropertyUnitId" uuid NOT NULL,
  "OriginalPrimaryClaimantId" uuid,
  "ClaimNumber" varchar,
  "ClaimType" varchar NOT NULL,
  "ClaimSource" int NOT NULL,
  "CaseStatus" int,
  "TenureContractType" int,
  "OwnershipShare" decimal,
  "ClaimDescription" text,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "StagingEvidences" (
  "Id" uuid PRIMARY KEY,
  "ImportPackageId" uuid NOT NULL,
  "OriginalEntityId" uuid,
  "StagingValidationStatus" int,
  "ValidationErrors" jsonb,
  "ValidationWarnings" jsonb,
  "IsApprovedForCommit" boolean NOT NULL DEFAULT false,
  "CommittedEntityId" uuid,
  "StagedAtUtc" timestamp,
  "OriginalPersonId" uuid,
  "OriginalPersonPropertyRelationId" uuid,
  "OriginalClaimId" uuid,
  "EvidenceType" int NOT NULL,
  "Description" text NOT NULL,
  "OriginalFileName" varchar NOT NULL,
  "FilePath" varchar NOT NULL,
  "FileSizeBytes" bigint NOT NULL,
  "MimeType" varchar NOT NULL,
  "FileHash" varchar(64),
  "IssuingAuthority" varchar,
  "DocumentIssuedDate" timestamp,
  "DocumentExpiryDate" timestamp,
  "DocumentReferenceNumber" varchar,
  "Notes" text,
  "VersionNumber" int NOT NULL DEFAULT 1,
  "OriginalPreviousVersionId" uuid,
  "IsCurrentVersion" boolean NOT NULL DEFAULT true,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

CREATE TABLE "StagingSurveys" (
  "Id" uuid PRIMARY KEY,
  "ImportPackageId" uuid NOT NULL,
  "OriginalEntityId" uuid,
  "StagingValidationStatus" int,
  "ValidationErrors" jsonb,
  "ValidationWarnings" jsonb,
  "IsApprovedForCommit" boolean NOT NULL DEFAULT false,
  "CommittedEntityId" uuid,
  "StagedAtUtc" timestamp,
  "OriginalBuildingId" uuid NOT NULL,
  "OriginalPropertyUnitId" uuid,
  "OriginalFieldCollectorId" uuid,
  "OriginalClaimId" uuid,
  "OriginalContactPersonId" uuid,
  "ReferenceCode" varchar,
  "Type" int,
  "Source" int,
  "SurveyDate" timestamp NOT NULL,
  "Status" int,
  "GpsCoordinates" varchar,
  "IntervieweeName" varchar,
  "IntervieweeRelationship" varchar,
  "Notes" text,
  "OfficeLocation" varchar,
  "RegistrationNumber" varchar,
  "AppointmentReference" varchar,
  "ContactPhone" varchar,
  "ContactEmail" varchar,
  "CreatedAtUtc" timestamp NOT NULL,
  "CreatedBy" uuid NOT NULL,
  "LastModifiedAtUtc" timestamp,
  "LastModifiedBy" uuid,
  "IsDeleted" boolean NOT NULL DEFAULT false,
  "DeletedAtUtc" timestamp,
  "DeletedBy" uuid
);

COMMENT ON COLUMN "Buildings"."BuildingId" IS 'Composite code: Gov+Dist+SubDist+Comm+Neigh+BuildingNo';

COMMENT ON COLUMN "Buildings"."BuildingType" IS 'BuildingType enum';

COMMENT ON COLUMN "Buildings"."Status" IS 'BuildingStatus enum';

COMMENT ON COLUMN "Buildings"."BuildingGeometry" IS 'PostGIS POLYGON/MULTIPOLYGON SRID 4326';

COMMENT ON COLUMN "BuildingDocuments"."FileHash" IS 'SHA-256';

COMMENT ON COLUMN "PropertyUnits"."UnitType" IS 'PropertyUnitType enum';

COMMENT ON COLUMN "PropertyUnits"."Status" IS 'PropertyUnitStatus enum';

COMMENT ON COLUMN "Persons"."Gender" IS 'Gender enum';

COMMENT ON COLUMN "Persons"."Nationality" IS 'Nationality enum';

COMMENT ON COLUMN "Persons"."RelationshipToHead" IS 'RelationshipToHead enum';

COMMENT ON COLUMN "Households"."OccupancyType" IS 'OccupancyType enum';

COMMENT ON COLUMN "Households"."OccupancyNature" IS 'OccupancyNature enum';

COMMENT ON COLUMN "Households"."ChildCount" IS 'Computed: MaleChildCount + FemaleChildCount';

COMMENT ON COLUMN "Households"."ElderlyCount" IS 'Computed: MaleElderlyCount + FemaleElderlyCount';

COMMENT ON COLUMN "Households"."PersonsWithDisabilitiesCount" IS 'Computed: MaleDisabledCount + FemaleDisabledCount';

COMMENT ON COLUMN "PersonPropertyRelations"."RelationType" IS 'RelationType enum';

COMMENT ON COLUMN "PersonPropertyRelations"."OccupancyType" IS 'OccupancyType enum';

COMMENT ON COLUMN "Claims"."ClaimNumber" IS 'Auto-generated';

COMMENT ON COLUMN "Claims"."ClaimType" IS 'ClaimType enum';

COMMENT ON COLUMN "Claims"."ClaimSource" IS 'ClaimSource enum';

COMMENT ON COLUMN "Claims"."CaseStatus" IS 'CaseStatus enum (Open/Closed)';

COMMENT ON COLUMN "Claims"."TenureContractType" IS 'TenureContractType enum';

COMMENT ON COLUMN "Evidences"."EvidenceType" IS 'EvidenceType enum';

COMMENT ON COLUMN "Evidences"."FileHash" IS 'SHA-256';

COMMENT ON COLUMN "Surveys"."ReferenceCode" IS 'ALG-YYYY-NNNNN or OFC-YYYY-NNNNN';

COMMENT ON COLUMN "Surveys"."Type" IS 'SurveyType enum';

COMMENT ON COLUMN "Surveys"."Source" IS 'SurveySource enum';

COMMENT ON COLUMN "Surveys"."Status" IS 'SurveyStatus enum';

COMMENT ON COLUMN "BuildingAssignments"."TransferStatus" IS 'TransferStatus enum';

COMMENT ON COLUMN "BuildingAssignments"."UnitsForRevisit" IS 'Array of PropertyUnit UUIDs';

COMMENT ON COLUMN "SyncSessions"."SessionStatus" IS 'SyncSessionStatus enum';

COMMENT ON COLUMN "ImportPackages"."PackageId" IS 'From .uhc manifest';

COMMENT ON COLUMN "ImportPackages"."PackageNumber" IS 'PKG-YYYY-NNNN';

COMMENT ON COLUMN "ImportPackages"."Status" IS 'ImportStatus enum';

COMMENT ON COLUMN "ImportPackages"."Checksum" IS 'SHA-256';

COMMENT ON COLUMN "ImportPackages"."CommitReportJson" IS 'Full commit report snapshot';

COMMENT ON COLUMN "ConflictResolutions"."ConflictNumber" IS 'CNF-YYYY-NNNN';

COMMENT ON COLUMN "ConflictResolutions"."ConflictType" IS 'PersonDuplicate/PropertyDuplicate/ClaimConflict';

COMMENT ON COLUMN "ConflictResolutions"."EntityType" IS 'Person/PropertyUnit/Building';

COMMENT ON COLUMN "ConflictResolutions"."SimilarityScore" IS '0-100%';

COMMENT ON COLUMN "ConflictResolutions"."ConfidenceLevel" IS 'Low/Medium/High';

COMMENT ON COLUMN "ConflictResolutions"."Status" IS 'PendingReview/Resolved/Ignored';

COMMENT ON COLUMN "ConflictResolutions"."ResolutionAction" IS 'ConflictResolutionAction enum';

COMMENT ON COLUMN "Neighborhoods"."FullCode" IS 'Composite: Gov+Dist+SubDist+Comm+Neigh';

COMMENT ON COLUMN "Neighborhoods"."CenterPoint" IS 'PostGIS POINT SRID 4326';

COMMENT ON COLUMN "Neighborhoods"."BoundaryGeometry" IS 'PostGIS POLYGON/MULTIPOLYGON SRID 4326';

COMMENT ON COLUMN "Landmarks"."Type" IS 'LandmarkType enum';

COMMENT ON COLUMN "Landmarks"."Location" IS 'PostGIS POINT SRID 4326';

COMMENT ON COLUMN "LandmarkTypeIcons"."Type" IS 'LandmarkType enum';

COMMENT ON COLUMN "Streets"."Geometry" IS 'PostGIS LINESTRING SRID 4326';

COMMENT ON COLUMN "Users"."Role" IS 'UserRole enum';

COMMENT ON COLUMN "UserPermissions"."Permission" IS 'Permission enum';

COMMENT ON COLUMN "AuditLogs"."Id" IS 'Inherits BaseEntity only (no soft delete)';

COMMENT ON COLUMN "AuditLogs"."ActionType" IS 'AuditActionType enum';

COMMENT ON COLUMN "Vocabularies"."Version" IS 'Semantic: MAJOR.MINOR.PATCH';

COMMENT ON COLUMN "Vocabularies"."ValuesJson" IS 'Array of {code, labelAr, labelEn, displayOrder}';

COMMENT ON COLUMN "StagingBuildings"."StagingValidationStatus" IS 'StagingValidationStatus enum';

COMMENT ON COLUMN "StagingBuildings"."BuildingType" IS 'BuildingType enum';

COMMENT ON COLUMN "StagingBuildings"."Status" IS 'BuildingStatus enum';

COMMENT ON COLUMN "StagingBuildings"."BuildingGeometryWkt" IS 'WKT string, not PostGIS geometry';

COMMENT ON COLUMN "StagingBuildingDocuments"."OriginalBuildingId" IS 'Refs StagingBuilding by OriginalEntityId';

COMMENT ON COLUMN "StagingPropertyUnits"."OriginalBuildingId" IS 'Refs StagingBuilding by OriginalEntityId';

COMMENT ON COLUMN "StagingPropertyUnits"."UnitType" IS 'PropertyUnitType enum';

COMMENT ON COLUMN "StagingPropertyUnits"."Status" IS 'PropertyUnitStatus enum';

COMMENT ON COLUMN "StagingPersons"."Gender" IS 'Gender enum';

COMMENT ON COLUMN "StagingPersons"."Nationality" IS 'Nationality enum';

COMMENT ON COLUMN "StagingPersons"."OriginalHouseholdId" IS 'Refs StagingHousehold by OriginalEntityId';

COMMENT ON COLUMN "StagingPersons"."RelationshipToHead" IS 'RelationshipToHead enum';

COMMENT ON COLUMN "StagingHouseholds"."OriginalPropertyUnitId" IS 'Refs StagingPropertyUnit';

COMMENT ON COLUMN "StagingHouseholds"."OriginalHeadOfHouseholdPersonId" IS 'Refs StagingPerson';

COMMENT ON COLUMN "StagingPersonPropertyRelations"."OriginalPersonId" IS 'Refs StagingPerson';

COMMENT ON COLUMN "StagingPersonPropertyRelations"."OriginalPropertyUnitId" IS 'Refs StagingPropertyUnit';

COMMENT ON COLUMN "StagingPersonPropertyRelations"."RelationType" IS 'RelationType enum';

COMMENT ON COLUMN "StagingClaims"."OriginalPropertyUnitId" IS 'Refs StagingPropertyUnit';

COMMENT ON COLUMN "StagingClaims"."OriginalPrimaryClaimantId" IS 'Refs StagingPerson';

COMMENT ON COLUMN "StagingClaims"."ClaimSource" IS 'ClaimSource enum';

COMMENT ON COLUMN "StagingClaims"."CaseStatus" IS 'CaseStatus enum';

COMMENT ON COLUMN "StagingClaims"."TenureContractType" IS 'TenureContractType enum';

COMMENT ON COLUMN "StagingEvidences"."OriginalPersonId" IS 'Refs StagingPerson';

COMMENT ON COLUMN "StagingEvidences"."OriginalPersonPropertyRelationId" IS 'Refs StagingPersonPropertyRelation';

COMMENT ON COLUMN "StagingEvidences"."OriginalClaimId" IS 'Refs StagingClaim';

COMMENT ON COLUMN "StagingEvidences"."EvidenceType" IS 'EvidenceType enum';

COMMENT ON COLUMN "StagingSurveys"."OriginalBuildingId" IS 'Refs StagingBuilding';

COMMENT ON COLUMN "StagingSurveys"."OriginalPropertyUnitId" IS 'Refs StagingPropertyUnit';

COMMENT ON COLUMN "StagingSurveys"."OriginalFieldCollectorId" IS 'Refs User';

COMMENT ON COLUMN "StagingSurveys"."OriginalClaimId" IS 'Refs StagingClaim';

COMMENT ON COLUMN "StagingSurveys"."OriginalContactPersonId" IS 'Refs StagingPerson';

COMMENT ON COLUMN "StagingSurveys"."Type" IS 'SurveyType enum';

COMMENT ON COLUMN "StagingSurveys"."Source" IS 'SurveySource enum';

COMMENT ON COLUMN "StagingSurveys"."Status" IS 'SurveyStatus enum';

ALTER TABLE "BuildingDocuments" ADD FOREIGN KEY ("BuildingId") REFERENCES "Buildings" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "PropertyUnits" ADD FOREIGN KEY ("BuildingId") REFERENCES "Buildings" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Persons" ADD FOREIGN KEY ("HouseholdId") REFERENCES "Households" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Households" ADD FOREIGN KEY ("PropertyUnitId") REFERENCES "PropertyUnits" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Households" ADD FOREIGN KEY ("HeadOfHouseholdPersonId") REFERENCES "Persons" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "PersonPropertyRelations" ADD FOREIGN KEY ("PersonId") REFERENCES "Persons" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "PersonPropertyRelations" ADD FOREIGN KEY ("PropertyUnitId") REFERENCES "PropertyUnits" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "PersonPropertyRelations" ADD FOREIGN KEY ("SurveyId") REFERENCES "Surveys" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Claims" ADD FOREIGN KEY ("PropertyUnitId") REFERENCES "PropertyUnits" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Claims" ADD FOREIGN KEY ("PrimaryClaimantId") REFERENCES "Persons" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Claims" ADD FOREIGN KEY ("OriginatingSurveyId") REFERENCES "Surveys" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Evidences" ADD FOREIGN KEY ("PreviousVersionId") REFERENCES "Evidences" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Evidences" ADD FOREIGN KEY ("PersonId") REFERENCES "Persons" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Evidences" ADD FOREIGN KEY ("ClaimId") REFERENCES "Claims" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "EvidenceRelations" ADD FOREIGN KEY ("EvidenceId") REFERENCES "Evidences" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "EvidenceRelations" ADD FOREIGN KEY ("PersonPropertyRelationId") REFERENCES "PersonPropertyRelations" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Surveys" ADD FOREIGN KEY ("BuildingId") REFERENCES "Buildings" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Surveys" ADD FOREIGN KEY ("PropertyUnitId") REFERENCES "PropertyUnits" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Surveys" ADD FOREIGN KEY ("FieldCollectorId") REFERENCES "Users" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Surveys" ADD FOREIGN KEY ("ClaimId") REFERENCES "Claims" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Surveys" ADD FOREIGN KEY ("ContactPersonId") REFERENCES "Persons" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "BuildingAssignments" ADD FOREIGN KEY ("BuildingId") REFERENCES "Buildings" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "BuildingAssignments" ADD FOREIGN KEY ("FieldCollectorId") REFERENCES "Users" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "BuildingAssignments" ADD FOREIGN KEY ("AssignedByUserId") REFERENCES "Users" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "BuildingAssignments" ADD FOREIGN KEY ("OriginalAssignmentId") REFERENCES "BuildingAssignments" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "SyncSessions" ADD FOREIGN KEY ("FieldCollectorId") REFERENCES "Users" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "ConflictResolutions" ADD FOREIGN KEY ("ImportPackageId") REFERENCES "ImportPackages" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Districts" ADD FOREIGN KEY ("GovernorateCode") REFERENCES "Governorates" ("Code") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "SubDistricts" ADD FOREIGN KEY ("GovernorateCode", "DistrictCode") REFERENCES "Districts" ("GovernorateCode", "Code") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Communities" ADD FOREIGN KEY ("GovernorateCode", "DistrictCode", "SubDistrictCode") REFERENCES "SubDistricts" ("GovernorateCode", "DistrictCode", "Code") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Users" ADD FOREIGN KEY ("SupervisorUserId") REFERENCES "Users" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "UserPermissions" ADD FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "AuditLogs" ADD FOREIGN KEY ("ParentAuditLogId") REFERENCES "AuditLogs" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "Vocabularies" ADD FOREIGN KEY ("PreviousVersionId") REFERENCES "Vocabularies" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "StagingBuildings" ADD FOREIGN KEY ("ImportPackageId") REFERENCES "ImportPackages" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "StagingBuildingDocuments" ADD FOREIGN KEY ("ImportPackageId") REFERENCES "ImportPackages" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "StagingPropertyUnits" ADD FOREIGN KEY ("ImportPackageId") REFERENCES "ImportPackages" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "StagingPersons" ADD FOREIGN KEY ("ImportPackageId") REFERENCES "ImportPackages" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "StagingHouseholds" ADD FOREIGN KEY ("ImportPackageId") REFERENCES "ImportPackages" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "StagingPersonPropertyRelations" ADD FOREIGN KEY ("ImportPackageId") REFERENCES "ImportPackages" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "StagingClaims" ADD FOREIGN KEY ("ImportPackageId") REFERENCES "ImportPackages" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "StagingEvidences" ADD FOREIGN KEY ("ImportPackageId") REFERENCES "ImportPackages" ("Id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "StagingSurveys" ADD FOREIGN KEY ("ImportPackageId") REFERENCES "ImportPackages" ("Id") DEFERRABLE INITIALLY IMMEDIATE;
