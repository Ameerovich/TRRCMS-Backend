using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStringEnumsToIntegers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =====================================================================
            // PRODUCTION TABLES — Convert string enum columns to integer
            // Uses PostgreSQL ALTER COLUMN ... TYPE integer USING (CASE ...)
            // Safe for both existing DBs (converts data) and fresh DBs (0 rows).
            // =====================================================================

            // --- Persons: Gender, Nationality, RelationshipToHead ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""Persons""
                    ALTER COLUMN ""Gender"" TYPE integer USING (
                        CASE ""Gender""
                            WHEN 'Male' THEN 1 WHEN 'Female' THEN 2
                            WHEN 'M' THEN 1 WHEN 'F' THEN 2
                            ELSE NULL END),
                    ALTER COLUMN ""Nationality"" TYPE integer USING (
                        CASE ""Nationality""
                            WHEN 'Syrian' THEN 1 WHEN 'Palestinian' THEN 2 WHEN 'Iraqi' THEN 3
                            WHEN 'Lebanese' THEN 4 WHEN 'Jordanian' THEN 5 WHEN 'Egyptian' THEN 6
                            WHEN 'Turkish' THEN 7 WHEN 'Saudi' THEN 8 WHEN 'Yemeni' THEN 9
                            WHEN 'Sudanese' THEN 10 WHEN 'Iranian' THEN 11
                            WHEN 'Stateless' THEN 97 WHEN 'Refugee' THEN 98 WHEN 'Other' THEN 99
                            ELSE NULL END),
                    ALTER COLUMN ""RelationshipToHead"" TYPE integer USING (
                        CASE ""RelationshipToHead""
                            WHEN 'Head' THEN 1 WHEN 'Spouse' THEN 2 WHEN 'Son' THEN 3
                            WHEN 'Daughter' THEN 4 WHEN 'Father' THEN 5 WHEN 'Mother' THEN 6
                            WHEN 'Brother' THEN 7 WHEN 'Sister' THEN 8 WHEN 'Grandfather' THEN 9
                            WHEN 'Grandmother' THEN 10 WHEN 'Grandson' THEN 11 WHEN 'Granddaughter' THEN 12
                            WHEN 'Uncle' THEN 13 WHEN 'Aunt' THEN 14 WHEN 'Nephew' THEN 15
                            WHEN 'Niece' THEN 16 WHEN 'Cousin' THEN 17 WHEN 'SonInLaw' THEN 18
                            WHEN 'DaughterInLaw' THEN 19 WHEN 'FatherInLaw' THEN 20 WHEN 'MotherInLaw' THEN 21
                            WHEN 'BrotherInLaw' THEN 22 WHEN 'SisterInLaw' THEN 23 WHEN 'Stepfather' THEN 24
                            WHEN 'Stepmother' THEN 25 WHEN 'Stepson' THEN 26 WHEN 'Stepdaughter' THEN 27
                            WHEN 'AdoptedChild' THEN 28 WHEN 'FosterChild' THEN 29
                            WHEN 'NonRelative' THEN 97 WHEN 'DomesticWorker' THEN 98 WHEN 'Other' THEN 99
                            ELSE NULL END);
            ");

            // --- Buildings: BuildingType, Status, DamageLevel ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""Buildings""
                    ALTER COLUMN ""BuildingType"" TYPE integer USING (
                        CASE ""BuildingType""
                            WHEN 'Residential' THEN 1 WHEN 'Commercial' THEN 2
                            WHEN 'MixedUse' THEN 3 WHEN 'Industrial' THEN 4
                            ELSE 1 END),
                    ALTER COLUMN ""Status"" TYPE integer USING (
                        CASE ""Status""
                            WHEN 'Intact' THEN 1 WHEN 'MinorDamage' THEN 2 WHEN 'ModerateDamage' THEN 3
                            WHEN 'MajorDamage' THEN 4 WHEN 'SeverelyDamaged' THEN 5 WHEN 'Destroyed' THEN 6
                            WHEN 'UnderConstruction' THEN 7 WHEN 'Abandoned' THEN 8 WHEN 'Unknown' THEN 99
                            ELSE 99 END),
                    ALTER COLUMN ""DamageLevel"" TYPE integer USING (
                        CASE ""DamageLevel""
                            WHEN 'NoDamage' THEN 0 WHEN 'Minor' THEN 1 WHEN 'Moderate' THEN 2
                            WHEN 'Major' THEN 3 WHEN 'Severe' THEN 4 WHEN 'CompleteDestruction' THEN 5
                            WHEN 'AssessmentPending' THEN 98 WHEN 'Unknown' THEN 99
                            ELSE NULL END);
            ");

            // --- PropertyUnits: UnitType, Status, DamageLevel, OccupancyType, OccupancyNature ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""PropertyUnits""
                    ALTER COLUMN ""UnitType"" TYPE integer USING (
                        CASE ""UnitType""
                            WHEN 'Apartment' THEN 1 WHEN 'Shop' THEN 2 WHEN 'Office' THEN 3
                            WHEN 'Warehouse' THEN 4 WHEN 'Other' THEN 5
                            ELSE 1 END),
                    ALTER COLUMN ""Status"" TYPE integer USING (
                        CASE ""Status""
                            WHEN 'Occupied' THEN 1 WHEN 'Vacant' THEN 2 WHEN 'Damaged' THEN 3
                            WHEN 'UnderRenovation' THEN 4 WHEN 'Uninhabitable' THEN 5
                            WHEN 'Locked' THEN 6 WHEN 'Unknown' THEN 99
                            ELSE 99 END),
                    ALTER COLUMN ""DamageLevel"" TYPE integer USING (
                        CASE ""DamageLevel""
                            WHEN 'NoDamage' THEN 0 WHEN 'Minor' THEN 1 WHEN 'Moderate' THEN 2
                            WHEN 'Major' THEN 3 WHEN 'Severe' THEN 4 WHEN 'CompleteDestruction' THEN 5
                            WHEN 'AssessmentPending' THEN 98 WHEN 'Unknown' THEN 99
                            ELSE NULL END),
                    ALTER COLUMN ""OccupancyType"" TYPE integer USING (
                        CASE ""OccupancyType""
                            WHEN 'OwnerOccupied' THEN 1 WHEN 'TenantOccupied' THEN 2
                            WHEN 'FamilyOccupied' THEN 3 WHEN 'MixedOccupancy' THEN 4
                            WHEN 'Vacant' THEN 5 WHEN 'TemporarySeasonal' THEN 6
                            WHEN 'CommercialUse' THEN 7 WHEN 'Abandoned' THEN 8
                            WHEN 'Disputed' THEN 9 WHEN 'Unknown' THEN 99
                            ELSE NULL END),
                    ALTER COLUMN ""OccupancyNature"" TYPE integer USING (
                        CASE ""OccupancyNature""
                            WHEN 'LegalFormal' THEN 1 WHEN 'Informal' THEN 2 WHEN 'Customary' THEN 3
                            WHEN 'TemporaryEmergency' THEN 4 WHEN 'Authorized' THEN 5
                            WHEN 'Unauthorized' THEN 6 WHEN 'PendingRegularization' THEN 7
                            WHEN 'Contested' THEN 8 WHEN 'Unknown' THEN 99
                            ELSE NULL END);
            ");

            // --- Households: OccupancyType, OccupancyNature ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""Households""
                    ALTER COLUMN ""OccupancyType"" TYPE integer USING (
                        CASE ""OccupancyType""
                            WHEN 'OwnerOccupied' THEN 1 WHEN 'TenantOccupied' THEN 2
                            WHEN 'FamilyOccupied' THEN 3 WHEN 'MixedOccupancy' THEN 4
                            WHEN 'Vacant' THEN 5 WHEN 'TemporarySeasonal' THEN 6
                            WHEN 'CommercialUse' THEN 7 WHEN 'Abandoned' THEN 8
                            WHEN 'Disputed' THEN 9 WHEN 'Unknown' THEN 99
                            ELSE NULL END),
                    ALTER COLUMN ""OccupancyNature"" TYPE integer USING (
                        CASE ""OccupancyNature""
                            WHEN 'LegalFormal' THEN 1 WHEN 'Informal' THEN 2 WHEN 'Customary' THEN 3
                            WHEN 'TemporaryEmergency' THEN 4 WHEN 'Authorized' THEN 5
                            WHEN 'Unauthorized' THEN 6 WHEN 'PendingRegularization' THEN 7
                            WHEN 'Contested' THEN 8 WHEN 'Unknown' THEN 99
                            ELSE NULL END);
            ");

            // --- PersonPropertyRelations: OccupancyType ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""PersonPropertyRelations""
                    ALTER COLUMN ""OccupancyType"" TYPE integer USING (
                        CASE ""OccupancyType""
                            WHEN 'OwnerOccupied' THEN 1 WHEN 'TenantOccupied' THEN 2
                            WHEN 'FamilyOccupied' THEN 3 WHEN 'MixedOccupancy' THEN 4
                            WHEN 'Vacant' THEN 5 WHEN 'TemporarySeasonal' THEN 6
                            WHEN 'CommercialUse' THEN 7 WHEN 'Abandoned' THEN 8
                            WHEN 'Disputed' THEN 9 WHEN 'Unknown' THEN 99
                            ELSE NULL END);
            ");

            // --- ImportPackages: Status ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""ImportPackages""
                    ALTER COLUMN ""Status"" TYPE integer USING (
                        CASE ""Status""
                            WHEN 'Pending' THEN 1 WHEN 'Validating' THEN 2 WHEN 'Staging' THEN 3
                            WHEN 'ValidationFailed' THEN 4 WHEN 'Quarantined' THEN 5
                            WHEN 'ReviewingConflicts' THEN 6 WHEN 'ReadyToCommit' THEN 7
                            WHEN 'Committing' THEN 8 WHEN 'Completed' THEN 9 WHEN 'Failed' THEN 10
                            WHEN 'PartiallyCompleted' THEN 11 WHEN 'Cancelled' THEN 12
                            ELSE 1 END);
            ");

            // --- ConflictResolutions: ResolutionAction ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""ConflictResolutions""
                    ALTER COLUMN ""ResolutionAction"" TYPE integer USING (
                        CASE ""ResolutionAction""
                            WHEN 'PendingReview' THEN 1 WHEN 'KeepBoth' THEN 2 WHEN 'Merge' THEN 3
                            WHEN 'KeepFirst' THEN 4 WHEN 'KeepSecond' THEN 5
                            WHEN 'MarkAsDuplicate' THEN 6 WHEN 'Escalate' THEN 7
                            WHEN 'Resolved' THEN 8 WHEN 'Ignored' THEN 9
                            ELSE NULL END);
            ");

            // =====================================================================
            // STAGING TABLES — Same conversion for import pipeline staging area
            // =====================================================================

            // --- StagingSurveys: Type, Source, Status ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingSurveys""
                    ALTER COLUMN ""Type"" TYPE integer USING (
                        CASE ""Type""
                            WHEN 'Field' THEN 1 WHEN 'Office' THEN 2
                            ELSE NULL END),
                    ALTER COLUMN ""Source"" TYPE integer USING (
                        CASE ""Source""
                            WHEN 'FieldCollection' THEN 1 WHEN 'OfficeSubmission' THEN 2
                            WHEN 'DataMigration' THEN 3 WHEN 'BulkImport' THEN 4
                            ELSE NULL END),
                    ALTER COLUMN ""Status"" TYPE integer USING (
                        CASE ""Status""
                            WHEN 'Draft' THEN 1 WHEN 'Completed' THEN 2 WHEN 'Finalized' THEN 3
                            WHEN 'Exported' THEN 4 WHEN 'Imported' THEN 5 WHEN 'Validated' THEN 6
                            WHEN 'RequiresRevision' THEN 7 WHEN 'Cancelled' THEN 8 WHEN 'Archived' THEN 99
                            ELSE NULL END);
            ");

            // --- StagingBuildings: BuildingType, Status, DamageLevel ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingBuildings""
                    ALTER COLUMN ""BuildingType"" TYPE integer USING (
                        CASE ""BuildingType""
                            WHEN 'Residential' THEN 1 WHEN 'Commercial' THEN 2
                            WHEN 'MixedUse' THEN 3 WHEN 'Industrial' THEN 4
                            ELSE 1 END),
                    ALTER COLUMN ""Status"" TYPE integer USING (
                        CASE ""Status""
                            WHEN 'Intact' THEN 1 WHEN 'MinorDamage' THEN 2 WHEN 'ModerateDamage' THEN 3
                            WHEN 'MajorDamage' THEN 4 WHEN 'SeverelyDamaged' THEN 5 WHEN 'Destroyed' THEN 6
                            WHEN 'UnderConstruction' THEN 7 WHEN 'Abandoned' THEN 8 WHEN 'Unknown' THEN 99
                            ELSE 99 END),
                    ALTER COLUMN ""DamageLevel"" TYPE integer USING (
                        CASE ""DamageLevel""
                            WHEN 'NoDamage' THEN 0 WHEN 'Minor' THEN 1 WHEN 'Moderate' THEN 2
                            WHEN 'Major' THEN 3 WHEN 'Severe' THEN 4 WHEN 'CompleteDestruction' THEN 5
                            WHEN 'AssessmentPending' THEN 98 WHEN 'Unknown' THEN 99
                            ELSE NULL END);
            ");

            // --- StagingPropertyUnits: UnitType, Status, DamageLevel, OccupancyType, OccupancyNature ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingPropertyUnits""
                    ALTER COLUMN ""UnitType"" TYPE integer USING (
                        CASE ""UnitType""
                            WHEN 'Apartment' THEN 1 WHEN 'Shop' THEN 2 WHEN 'Office' THEN 3
                            WHEN 'Warehouse' THEN 4 WHEN 'Other' THEN 5
                            ELSE 1 END),
                    ALTER COLUMN ""Status"" TYPE integer USING (
                        CASE ""Status""
                            WHEN 'Occupied' THEN 1 WHEN 'Vacant' THEN 2 WHEN 'Damaged' THEN 3
                            WHEN 'UnderRenovation' THEN 4 WHEN 'Uninhabitable' THEN 5
                            WHEN 'Locked' THEN 6 WHEN 'Unknown' THEN 99
                            ELSE 99 END),
                    ALTER COLUMN ""DamageLevel"" TYPE integer USING (
                        CASE ""DamageLevel""
                            WHEN 'NoDamage' THEN 0 WHEN 'Minor' THEN 1 WHEN 'Moderate' THEN 2
                            WHEN 'Major' THEN 3 WHEN 'Severe' THEN 4 WHEN 'CompleteDestruction' THEN 5
                            WHEN 'AssessmentPending' THEN 98 WHEN 'Unknown' THEN 99
                            ELSE NULL END),
                    ALTER COLUMN ""OccupancyType"" TYPE integer USING (
                        CASE ""OccupancyType""
                            WHEN 'OwnerOccupied' THEN 1 WHEN 'TenantOccupied' THEN 2
                            WHEN 'FamilyOccupied' THEN 3 WHEN 'MixedOccupancy' THEN 4
                            WHEN 'Vacant' THEN 5 WHEN 'TemporarySeasonal' THEN 6
                            WHEN 'CommercialUse' THEN 7 WHEN 'Abandoned' THEN 8
                            WHEN 'Disputed' THEN 9 WHEN 'Unknown' THEN 99
                            ELSE NULL END),
                    ALTER COLUMN ""OccupancyNature"" TYPE integer USING (
                        CASE ""OccupancyNature""
                            WHEN 'LegalFormal' THEN 1 WHEN 'Informal' THEN 2 WHEN 'Customary' THEN 3
                            WHEN 'TemporaryEmergency' THEN 4 WHEN 'Authorized' THEN 5
                            WHEN 'Unauthorized' THEN 6 WHEN 'PendingRegularization' THEN 7
                            WHEN 'Contested' THEN 8 WHEN 'Unknown' THEN 99
                            ELSE NULL END);
            ");

            // --- StagingPersonPropertyRelations: RelationType, ContractType ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingPersonPropertyRelations""
                    ALTER COLUMN ""RelationType"" TYPE integer USING (
                        CASE ""RelationType""
                            WHEN 'Owner' THEN 1 WHEN 'Occupant' THEN 2 WHEN 'Tenant' THEN 3
                            WHEN 'Guest' THEN 4 WHEN 'Heir' THEN 5 WHEN 'Other' THEN 99
                            ELSE 99 END),
                    ALTER COLUMN ""ContractType"" TYPE integer USING (
                        CASE ""ContractType""
                            WHEN 'FullOwnership' THEN 1 WHEN 'SharedOwnership' THEN 2
                            WHEN 'LongTermRental' THEN 3 WHEN 'ShortTermRental' THEN 4
                            WHEN 'InformalTenure' THEN 5 WHEN 'UnauthorizedOccupation' THEN 6
                            WHEN 'CustomaryRights' THEN 7 WHEN 'InheritanceBased' THEN 8
                            WHEN 'HostedGuest' THEN 9 WHEN 'TemporaryShelter' THEN 10
                            WHEN 'GovernmentAllocation' THEN 11 WHEN 'Usufruct' THEN 12
                            WHEN 'Other' THEN 99
                            ELSE NULL END);
            ");

            // --- StagingEvidences: EvidenceType ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingEvidences""
                    ALTER COLUMN ""EvidenceType"" TYPE integer USING (
                        CASE ""EvidenceType""
                            WHEN 'IdentificationDocument' THEN 1 WHEN 'OwnershipDeed' THEN 2
                            WHEN 'RentalContract' THEN 3 WHEN 'UtilityBill' THEN 4
                            WHEN 'Photo' THEN 5 WHEN 'OfficialLetter' THEN 6
                            WHEN 'CourtOrder' THEN 7 WHEN 'InheritanceDocument' THEN 8
                            WHEN 'TaxReceipt' THEN 9 WHEN 'Other' THEN 99
                            ELSE 99 END);
            ");

            // --- StagingClaims: Drop string DEFAULT on Priority before type conversion ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingClaims"" ALTER COLUMN ""Priority"" DROP DEFAULT;
            ");

            // --- StagingClaims: ClaimSource, Priority, LifecycleStage, Status, TenureContractType, VerificationStatus ---
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingClaims""
                    ALTER COLUMN ""ClaimSource"" TYPE integer USING (
                        CASE ""ClaimSource""
                            WHEN 'FieldCollection' THEN 1 WHEN 'OfficeSubmission' THEN 2
                            WHEN 'SystemImport' THEN 3 WHEN 'Migration' THEN 4
                            WHEN 'OnlinePortal' THEN 5 WHEN 'ApiIntegration' THEN 6
                            WHEN 'ManualEntry' THEN 7 WHEN 'Other' THEN 99
                            ELSE 1 END),
                    ALTER COLUMN ""Priority"" TYPE integer USING (
                        CASE ""Priority""
                            WHEN 'Low' THEN 1 WHEN 'Normal' THEN 2 WHEN 'Medium' THEN 3
                            WHEN 'High' THEN 4 WHEN 'Critical' THEN 5
                            WHEN 'VIP' THEN 6 WHEN 'Escalated' THEN 7
                            ELSE 2 END),
                    ALTER COLUMN ""LifecycleStage"" TYPE integer USING (
                        CASE ""LifecycleStage""
                            WHEN 'DraftPendingSubmission' THEN 1 WHEN 'Submitted' THEN 2
                            WHEN 'InitialScreening' THEN 3 WHEN 'UnderReview' THEN 4
                            WHEN 'AwaitingDocuments' THEN 5 WHEN 'ConflictDetected' THEN 6
                            WHEN 'InAdjudication' THEN 7 WHEN 'PendingApproval' THEN 8
                            WHEN 'Approved' THEN 9 WHEN 'Rejected' THEN 10
                            WHEN 'OnHold' THEN 11 WHEN 'Reassigned' THEN 12
                            WHEN 'CertificateIssued' THEN 13 WHEN 'Archived' THEN 99
                            ELSE NULL END),
                    ALTER COLUMN ""Status"" TYPE integer USING (
                        CASE ""Status""
                            WHEN 'Draft' THEN 1 WHEN 'Finalized' THEN 2 WHEN 'UnderReview' THEN 3
                            WHEN 'Approved' THEN 4 WHEN 'Rejected' THEN 5
                            WHEN 'PendingEvidence' THEN 6 WHEN 'Disputed' THEN 7 WHEN 'Archived' THEN 99
                            ELSE NULL END),
                    ALTER COLUMN ""TenureContractType"" TYPE integer USING (
                        CASE ""TenureContractType""
                            WHEN 'FullOwnership' THEN 1 WHEN 'SharedOwnership' THEN 2
                            WHEN 'LongTermRental' THEN 3 WHEN 'ShortTermRental' THEN 4
                            WHEN 'InformalTenure' THEN 5 WHEN 'UnauthorizedOccupation' THEN 6
                            WHEN 'CustomaryRights' THEN 7 WHEN 'InheritanceBased' THEN 8
                            WHEN 'HostedGuest' THEN 9 WHEN 'TemporaryShelter' THEN 10
                            WHEN 'GovernmentAllocation' THEN 11 WHEN 'Usufruct' THEN 12
                            WHEN 'Other' THEN 99
                            ELSE NULL END),
                    ALTER COLUMN ""VerificationStatus"" TYPE integer USING (
                        CASE ""VerificationStatus""
                            WHEN 'Pending' THEN 1 WHEN 'UnderReview' THEN 2
                            WHEN 'Verified' THEN 3 WHEN 'Rejected' THEN 4
                            WHEN 'RequiresAdditionalInfo' THEN 5 WHEN 'Expired' THEN 6
                            ELSE NULL END);
            ");

            // Set default for StagingClaims.Priority (was 'Normal', now integer 2)
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingClaims"" ALTER COLUMN ""Priority"" SET DEFAULT 2;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "StagingSurveys",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Optional — auto-set (Field/Office) during commit",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Optional — auto-set (Field/Office) during commit");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "StagingSurveys",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Optional — auto-set to Draft during commit",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Optional — auto-set to Draft during commit");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "StagingSurveys",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Optional — auto-set during commit",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Optional — auto-set during commit");

            migrationBuilder.AlterColumn<string>(
                name: "UnitType",
                table: "StagingPropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "StagingPropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "OccupancyType",
                table: "StagingPropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OccupancyNature",
                table: "StagingPropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DamageLevel",
                table: "StagingPropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RelationType",
                table: "StagingPersonPropertyRelations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ContractType",
                table: "StagingPersonPropertyRelations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EvidenceType",
                table: "StagingEvidences",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "VerificationStatus",
                table: "StagingClaims",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Optional — auto-set to Pending during commit",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Optional — auto-set to Pending during commit");

            migrationBuilder.AlterColumn<string>(
                name: "TenureContractType",
                table: "StagingClaims",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "StagingClaims",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Optional — auto-set to Draft during commit",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Optional — auto-set to Draft during commit");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "StagingClaims",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Normal",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 2);

            migrationBuilder.AlterColumn<string>(
                name: "LifecycleStage",
                table: "StagingClaims",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Optional — auto-set to DraftPendingSubmission during commit",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Optional — auto-set to DraftPendingSubmission during commit");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimSource",
                table: "StagingClaims",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "StagingBuildings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "DamageLevel",
                table: "StagingBuildings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BuildingType",
                table: "StagingBuildings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "UnitType",
                table: "PropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "OccupancyType",
                table: "PropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OccupancyNature",
                table: "PropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DamageLevel",
                table: "PropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RelationshipToHead",
                table: "Persons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Relationship to head of household (enum converted to string)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "صلة القرابة برب الأسرة - Relationship to head of household enum stored as integer");

            migrationBuilder.AlterColumn<string>(
                name: "Nationality",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "Nationality (enum converted to string)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "الجنسية - Nationality enum stored as integer");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "Gender (enum converted to string)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "الجنس - Gender enum stored as integer");

            migrationBuilder.AlterColumn<string>(
                name: "OccupancyType",
                table: "PersonPropertyRelations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "نوع الإشغال - Occupancy type (enum): OwnerOccupied=1, TenantOccupied=2, etc.",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "نوع الإشغال - Occupancy type enum stored as integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ImportPackages",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                comment: "Current import workflow status",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Current import workflow status - stored as integer");

            migrationBuilder.AlterColumn<string>(
                name: "OccupancyType",
                table: "Households",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "نوع الإشغال - Occupancy type (enum converted to string): OwnerOccupied, TenantOccupied, etc.",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "نوع الإشغال - Occupancy type enum stored as integer");

            migrationBuilder.AlterColumn<string>(
                name: "OccupancyNature",
                table: "Households",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "طبيعة الإشغال - Occupancy nature (enum converted to string): LegalFormal, Informal, Customary, etc.",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "طبيعة الإشغال - Occupancy nature enum stored as integer");

            migrationBuilder.AlterColumn<string>(
                name: "ResolutionAction",
                table: "ConflictResolutions",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                comment: "KeepBoth, Merge, KeepFirst, KeepSecond, Ignored, etc.",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "KeepBoth, Merge, KeepFirst, KeepSecond, Ignored, etc. - stored as integer");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Buildings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "DamageLevel",
                table: "Buildings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BuildingType",
                table: "Buildings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
