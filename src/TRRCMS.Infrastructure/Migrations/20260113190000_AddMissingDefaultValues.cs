using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingDefaultValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ==================== DOCUMENTS TABLE ====================

            migrationBuilder.Sql(@"
                ALTER TABLE ""Documents"" 
                ALTER COLUMN ""IsVerified"" SET DEFAULT false;
                
                ALTER TABLE ""Documents"" 
                ALTER COLUMN ""IsLegallyValid"" SET DEFAULT true;
                
                ALTER TABLE ""Documents"" 
                ALTER COLUMN ""IsOriginal"" SET DEFAULT true;
                
                ALTER TABLE ""Documents"" 
                ALTER COLUMN ""IsNotarized"" SET DEFAULT false;
                
                ALTER TABLE ""Documents"" 
                ALTER COLUMN ""IsDeleted"" SET DEFAULT false;
            ");

            // ==================== EVIDENCES TABLE ====================

            migrationBuilder.Sql(@"
                ALTER TABLE ""Evidences"" 
                ALTER COLUMN ""IsCurrentVersion"" SET DEFAULT true;
                
                ALTER TABLE ""Evidences"" 
                ALTER COLUMN ""VersionNumber"" SET DEFAULT 1;
                
                ALTER TABLE ""Evidences"" 
                ALTER COLUMN ""IsDeleted"" SET DEFAULT false;
            ");

            // ==================== CLAIMS TABLE ====================

            migrationBuilder.Sql(@"
                ALTER TABLE ""Claims"" 
                ALTER COLUMN ""IsDeleted"" SET DEFAULT false;
                
                ALTER TABLE ""Claims"" 
                ALTER COLUMN ""HasConflicts"" SET DEFAULT false;
                
                ALTER TABLE ""Claims"" 
                ALTER COLUMN ""AllRequiredDocumentsSubmitted"" SET DEFAULT false;
                
                ALTER TABLE ""Claims"" 
                ALTER COLUMN ""ConflictCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Claims"" 
                ALTER COLUMN ""EvidenceCount"" SET DEFAULT 0;
            ");

            // ==================== REFERRALS TABLE ====================

            migrationBuilder.Sql(@"
                ALTER TABLE ""Referrals"" 
                ALTER COLUMN ""IsDeleted"" SET DEFAULT false;
                
                ALTER TABLE ""Referrals"" 
                ALTER COLUMN ""IsEscalation"" SET DEFAULT false;
                
                ALTER TABLE ""Referrals"" 
                ALTER COLUMN ""IsOverdue"" SET DEFAULT false;
            ");

            // ==================== PERSONS TABLE ====================

            migrationBuilder.Sql(@"
                ALTER TABLE ""Persons"" 
                ALTER COLUMN ""IsDeleted"" SET DEFAULT false;
                
                ALTER TABLE ""Persons"" 
                ALTER COLUMN ""IsContactPerson"" SET DEFAULT false;
                
                ALTER TABLE ""Persons"" 
                ALTER COLUMN ""HasIdentificationDocument"" SET DEFAULT false;
            ");

            // ==================== PERSONPROPERTYRELATIONS TABLE ====================

            migrationBuilder.Sql(@"
                ALTER TABLE ""PersonPropertyRelations"" 
                ALTER COLUMN ""IsDeleted"" SET DEFAULT false;
                
                ALTER TABLE ""PersonPropertyRelations"" 
                ALTER COLUMN ""IsActive"" SET DEFAULT true;
            ");

            // ==================== HOUSEHOLDS TABLE ====================

            migrationBuilder.Sql(@"
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""IsDeleted"" SET DEFAULT false;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""HouseholdSize"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""MaleCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""FemaleCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""InfantCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""ChildCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""MinorCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""AdultCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""ElderlyCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""PersonsWithDisabilitiesCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""IsFemaleHeaded"" SET DEFAULT false;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""WidowCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""OrphanCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""SingleParentCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""EmployedPersonsCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""UnemployedPersonsCount"" SET DEFAULT 0;
                
                ALTER TABLE ""Households"" 
                ALTER COLUMN ""IsDisplaced"" SET DEFAULT false;
            ");

            // ==================== PROPERTYUNITS TABLE ====================

            migrationBuilder.Sql(@"
                ALTER TABLE ""PropertyUnits"" 
                ALTER COLUMN ""IsDeleted"" SET DEFAULT false;
            ");

            // ==================== BUILDINGS TABLE ====================

            migrationBuilder.Sql(@"
                ALTER TABLE ""Buildings"" 
                ALTER COLUMN ""IsDeleted"" SET DEFAULT false;
            ");

            // ==================== USERS TABLE ====================

            migrationBuilder.Sql(@"
                ALTER TABLE ""Users"" 
                ALTER COLUMN ""IsDeleted"" SET DEFAULT false;
                
                ALTER TABLE ""Users"" 
                ALTER COLUMN ""IsActive"" SET DEFAULT true;
                
                ALTER TABLE ""Users"" 
                ALTER COLUMN ""IsLockedOut"" SET DEFAULT false;
                
                ALTER TABLE ""Users"" 
                ALTER COLUMN ""FailedLoginAttempts"" SET DEFAULT 0;
                
                ALTER TABLE ""Users"" 
                ALTER COLUMN ""MustChangePassword"" SET DEFAULT false;
                
                ALTER TABLE ""Users"" 
                ALTER COLUMN ""TwoFactorEnabled"" SET DEFAULT false;
            ");

            // NOTE: AuditLogs table does NOT have IsDeleted column, so it's excluded
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove all defaults
            migrationBuilder.Sql(@"
                -- Documents
                ALTER TABLE ""Documents"" ALTER COLUMN ""IsVerified"" DROP DEFAULT;
                ALTER TABLE ""Documents"" ALTER COLUMN ""IsLegallyValid"" DROP DEFAULT;
                ALTER TABLE ""Documents"" ALTER COLUMN ""IsOriginal"" DROP DEFAULT;
                ALTER TABLE ""Documents"" ALTER COLUMN ""IsNotarized"" DROP DEFAULT;
                ALTER TABLE ""Documents"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;
                
                -- Evidences
                ALTER TABLE ""Evidences"" ALTER COLUMN ""IsCurrentVersion"" DROP DEFAULT;
                ALTER TABLE ""Evidences"" ALTER COLUMN ""VersionNumber"" DROP DEFAULT;
                ALTER TABLE ""Evidences"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;
                
                -- Claims
                ALTER TABLE ""Claims"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;
                ALTER TABLE ""Claims"" ALTER COLUMN ""HasConflicts"" DROP DEFAULT;
                ALTER TABLE ""Claims"" ALTER COLUMN ""AllRequiredDocumentsSubmitted"" DROP DEFAULT;
                ALTER TABLE ""Claims"" ALTER COLUMN ""ConflictCount"" DROP DEFAULT;
                ALTER TABLE ""Claims"" ALTER COLUMN ""EvidenceCount"" DROP DEFAULT;
                
                -- Referrals
                ALTER TABLE ""Referrals"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;
                ALTER TABLE ""Referrals"" ALTER COLUMN ""IsEscalation"" DROP DEFAULT;
                ALTER TABLE ""Referrals"" ALTER COLUMN ""IsOverdue"" DROP DEFAULT;
                
                -- Persons
                ALTER TABLE ""Persons"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;
                ALTER TABLE ""Persons"" ALTER COLUMN ""IsContactPerson"" DROP DEFAULT;
                ALTER TABLE ""Persons"" ALTER COLUMN ""HasIdentificationDocument"" DROP DEFAULT;
                
                -- PersonPropertyRelations
                ALTER TABLE ""PersonPropertyRelations"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;
                ALTER TABLE ""PersonPropertyRelations"" ALTER COLUMN ""IsActive"" DROP DEFAULT;
                
                -- Households
                ALTER TABLE ""Households"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""HouseholdSize"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""MaleCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""FemaleCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""InfantCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""ChildCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""MinorCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""AdultCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""ElderlyCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""PersonsWithDisabilitiesCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""IsFemaleHeaded"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""WidowCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""OrphanCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""SingleParentCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""EmployedPersonsCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""UnemployedPersonsCount"" DROP DEFAULT;
                ALTER TABLE ""Households"" ALTER COLUMN ""IsDisplaced"" DROP DEFAULT;
                
                -- PropertyUnits
                ALTER TABLE ""PropertyUnits"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;
                
                -- Buildings
                ALTER TABLE ""Buildings"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;
                
                -- Users
                ALTER TABLE ""Users"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;
                ALTER TABLE ""Users"" ALTER COLUMN ""IsActive"" DROP DEFAULT;
                ALTER TABLE ""Users"" ALTER COLUMN ""IsLockedOut"" DROP DEFAULT;
                ALTER TABLE ""Users"" ALTER COLUMN ""FailedLoginAttempts"" DROP DEFAULT;
                ALTER TABLE ""Users"" ALTER COLUMN ""MustChangePassword"" DROP DEFAULT;
                ALTER TABLE ""Users"" ALTER COLUMN ""TwoFactorEnabled"" DROP DEFAULT;
            ");
        }
    }
}