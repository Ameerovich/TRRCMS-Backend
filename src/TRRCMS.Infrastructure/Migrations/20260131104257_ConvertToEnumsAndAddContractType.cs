using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertToEnumsAndAddContractType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =====================================================================
            // STEP 1: Convert PersonPropertyRelations.RelationType from string to int
            // =====================================================================

            // Add temporary integer column
            migrationBuilder.AddColumn<int>(
                name: "RelationType_Temp",
                table: "PersonPropertyRelations",
                type: "integer",
                nullable: true);

            // Convert string values to integers
            migrationBuilder.Sql(@"
                UPDATE ""PersonPropertyRelations"" SET ""RelationType_Temp"" = 
                    CASE ""RelationType""
                        WHEN 'Owner' THEN 1
                        WHEN 'Occupant' THEN 2
                        WHEN 'Tenant' THEN 3
                        WHEN 'Guest' THEN 4
                        WHEN 'Heir' THEN 5
                        WHEN 'Other' THEN 99
                        ELSE 99
                    END;
            ");

            // Drop old string column
            migrationBuilder.DropColumn(
                name: "RelationType",
                table: "PersonPropertyRelations");

            // Rename temp column to RelationType
            migrationBuilder.RenameColumn(
                name: "RelationType_Temp",
                table: "PersonPropertyRelations",
                newName: "RelationType");

            // Make it NOT NULL with proper comment
            migrationBuilder.AlterColumn<int>(
                name: "RelationType",
                table: "PersonPropertyRelations",
                type: "integer",
                nullable: false,
                defaultValue: 99,
                comment: "نوع العلاقة - Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99");

            // =====================================================================
            // STEP 2: Convert Evidences.EvidenceType from string to int
            // =====================================================================

            // Add temporary integer column
            migrationBuilder.AddColumn<int>(
                name: "EvidenceType_Temp",
                table: "Evidences",
                type: "integer",
                nullable: true);

            // Convert string values to integers
            migrationBuilder.Sql(@"
                UPDATE ""Evidences"" SET ""EvidenceType_Temp"" = 
                    CASE ""EvidenceType""
                        WHEN 'IdentificationDocument' THEN 1
                        WHEN 'OwnershipDeed' THEN 2
                        WHEN 'RentalContract' THEN 3
                        WHEN 'UtilityBill' THEN 4
                        WHEN 'Photo' THEN 5
                        WHEN 'PropertyPhoto' THEN 5
                        WHEN 'OfficialLetter' THEN 6
                        WHEN 'CourtOrder' THEN 7
                        WHEN 'InheritanceDocument' THEN 8
                        WHEN 'TaxReceipt' THEN 9
                        WHEN 'TenureDocument' THEN 2
                        WHEN 'Other' THEN 99
                        ELSE 99
                    END;
            ");

            // Drop old string column
            migrationBuilder.DropColumn(
                name: "EvidenceType",
                table: "Evidences");

            // Rename temp column to EvidenceType
            migrationBuilder.RenameColumn(
                name: "EvidenceType_Temp",
                table: "Evidences",
                newName: "EvidenceType");

            // Make it NOT NULL with proper comment
            migrationBuilder.AlterColumn<int>(
                name: "EvidenceType",
                table: "Evidences",
                type: "integer",
                nullable: false,
                defaultValue: 99,
                comment: "نوع الدليل - IdentificationDocument=1, OwnershipDeed=2, RentalContract=3, etc.");

            // =====================================================================
            // STEP 3: Add new ContractType columns
            // =====================================================================

            migrationBuilder.AddColumn<int>(
                name: "ContractType",
                table: "PersonPropertyRelations",
                type: "integer",
                nullable: true,
                comment: "نوع العقد - FullOwnership=1, SharedOwnership=2, LongTermRental=3, etc.");

            migrationBuilder.AddColumn<string>(
                name: "ContractTypeOtherDesc",
                table: "PersonPropertyRelations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Description when contract type is 'Other'");

            // =====================================================================
            // STEP 4: Update other column comments and max lengths
            // =====================================================================

            migrationBuilder.AlterColumn<string>(
                name: "RelationTypeOtherDesc",
                table: "PersonPropertyRelations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Description when relation type is 'Other'",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "تاريخ بدء العلاقة - Start date of the relation",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OwnershipShare",
                table: "PersonPropertyRelations",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                comment: "حصة الملكية - Ownership share (0.0 to 1.0)",
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PersonPropertyRelations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "ملاحظاتك - Additional notes about this relation",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            // =====================================================================
            // STEP 5: Create indexes
            // =====================================================================

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelation_ContractType",
                table: "PersonPropertyRelations",
                column: "ContractType");

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelation_RelationType",
                table: "PersonPropertyRelations",
                column: "RelationType");

            migrationBuilder.CreateIndex(
                name: "IX_Evidences_EvidenceType",
                table: "Evidences",
                column: "EvidenceType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_Evidences_EvidenceType",
                table: "Evidences");

            migrationBuilder.DropIndex(
                name: "IX_PersonPropertyRelation_RelationType",
                table: "PersonPropertyRelations");

            migrationBuilder.DropIndex(
                name: "IX_PersonPropertyRelation_ContractType",
                table: "PersonPropertyRelations");

            // Remove ContractType columns
            migrationBuilder.DropColumn(
                name: "ContractType",
                table: "PersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "ContractTypeOtherDesc",
                table: "PersonPropertyRelations");

            // =====================================================================
            // Revert EvidenceType back to string
            // =====================================================================

            migrationBuilder.AddColumn<string>(
                name: "EvidenceType_Temp",
                table: "Evidences",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Evidences"" SET ""EvidenceType_Temp"" = 
                    CASE ""EvidenceType""
                        WHEN 1 THEN 'IdentificationDocument'
                        WHEN 2 THEN 'OwnershipDeed'
                        WHEN 3 THEN 'RentalContract'
                        WHEN 4 THEN 'UtilityBill'
                        WHEN 5 THEN 'Photo'
                        WHEN 6 THEN 'OfficialLetter'
                        WHEN 7 THEN 'CourtOrder'
                        WHEN 8 THEN 'InheritanceDocument'
                        WHEN 9 THEN 'TaxReceipt'
                        WHEN 99 THEN 'Other'
                        ELSE 'Other'
                    END;
            ");

            migrationBuilder.DropColumn(
                name: "EvidenceType",
                table: "Evidences");

            migrationBuilder.RenameColumn(
                name: "EvidenceType_Temp",
                table: "Evidences",
                newName: "EvidenceType");

            migrationBuilder.AlterColumn<string>(
                name: "EvidenceType",
                table: "Evidences",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Other",
                comment: "Evidence type (controlled vocabulary)");

            // =====================================================================
            // Revert RelationType back to string
            // =====================================================================

            migrationBuilder.AddColumn<string>(
                name: "RelationType_Temp",
                table: "PersonPropertyRelations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""PersonPropertyRelations"" SET ""RelationType_Temp"" = 
                    CASE ""RelationType""
                        WHEN 1 THEN 'Owner'
                        WHEN 2 THEN 'Occupant'
                        WHEN 3 THEN 'Tenant'
                        WHEN 4 THEN 'Guest'
                        WHEN 5 THEN 'Heir'
                        WHEN 99 THEN 'Other'
                        ELSE 'Other'
                    END;
            ");

            migrationBuilder.DropColumn(
                name: "RelationType",
                table: "PersonPropertyRelations");

            migrationBuilder.RenameColumn(
                name: "RelationType_Temp",
                table: "PersonPropertyRelations",
                newName: "RelationType");

            migrationBuilder.AlterColumn<string>(
                name: "RelationType",
                table: "PersonPropertyRelations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Other",
                comment: "Type of relation (owner, tenant, occupant, guest, heir, other, etc.)");

            // Revert other column changes
            migrationBuilder.AlterColumn<string>(
                name: "RelationTypeOtherDesc",
                table: "PersonPropertyRelations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}