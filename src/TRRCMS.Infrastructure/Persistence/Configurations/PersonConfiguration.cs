using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Configurations
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            // Table name
            builder.ToTable("Persons");

            // Primary Key
            builder.HasKey(p => p.Id);

            // ==================== REQUIRED ARABIC NAMES ====================

            builder.Property(p => p.FirstNameArabic)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("First name in Arabic (الاسم الأول)");

            builder.Property(p => p.FatherNameArabic)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Father's name in Arabic (اسم الأب)");

            builder.Property(p => p.FamilyNameArabic)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Family/Last name in Arabic (اسم العائلة)");

            builder.Property(p => p.MotherNameArabic)
                .HasMaxLength(100)
                .HasComment("Mother's name in Arabic (اسم الأم)");

            // ==================== OPTIONAL ENGLISH NAME ====================

            builder.Property(p => p.FullNameEnglish)
                .HasMaxLength(300)
                .HasComment("Full name in English (optional)");

            // ==================== IDENTIFICATION ====================

            builder.Property(p => p.NationalId)
                .HasMaxLength(50)
                .HasComment("National ID or identification number");

            // ==================== DEMOGRAPHICS ====================

            builder.Property(p => p.YearOfBirth)
                .HasComment("Year of birth (integer)");

            builder.Property(p => p.Gender)
                .HasMaxLength(20)
                .HasComment("Gender (controlled vocabulary: M/F)");

            builder.Property(p => p.Nationality)
                .HasMaxLength(100)
                .HasComment("Nationality (controlled vocabulary)");

            // ==================== CONTACT INFORMATION ====================

            builder.Property(p => p.PrimaryPhoneNumber)
                .HasMaxLength(20)
                .HasComment("Primary phone number");

            builder.Property(p => p.SecondaryPhoneNumber)
                .HasMaxLength(20)
                .HasComment("Secondary phone number");

            builder.Property(p => p.IsContactPerson)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Indicates if this person is the main contact");

            // ==================== HOUSEHOLD RELATIONSHIP ====================

            builder.Property(p => p.HouseholdId)
                .HasComment("Foreign key to household (nullable)");

            builder.Property(p => p.RelationshipToHead)
                .HasMaxLength(50)
                .HasComment("Relationship to head of household");

            // ==================== IDENTIFICATION DOCUMENT ====================

            builder.Property(p => p.HasIdentificationDocument)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Flag indicating if ID document was uploaded");

            // ==================== AUDIT FIELDS ====================

            builder.Property(p => p.CreatedAtUtc)
                .IsRequired()
                .HasComment("Creation timestamp (UTC)");

            builder.Property(p => p.CreatedBy)
                .IsRequired()
                .HasComment("User who created this record");

            builder.Property(p => p.LastModifiedAtUtc)
                .HasComment("Last modification timestamp (UTC)");

            builder.Property(p => p.LastModifiedBy)
                .HasComment("User who last modified this record");

            builder.Property(p => p.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Soft delete flag");

            builder.Property(p => p.DeletedAtUtc)
                .HasComment("Deletion timestamp (UTC)");

            builder.Property(p => p.DeletedBy)
                .HasComment("User who deleted this record");

            // ==================== ROW VERSION (Concurrency Token) ====================

            builder.Property(p => p.RowVersion)
                .IsRowVersion()
                .HasComment("Concurrency token");

            // ==================== INDEXES ====================

            // Index for NationalId searches
            builder.HasIndex(p => p.NationalId)
                .HasDatabaseName("IX_Person_NationalId");

            // Composite index for name searches
            builder.HasIndex(p => new { p.FirstNameArabic, p.FatherNameArabic, p.FamilyNameArabic })
                .HasDatabaseName("IX_Person_FullNameArabic");

            // Index for phone number searches
            builder.HasIndex(p => p.PrimaryPhoneNumber)
                .HasDatabaseName("IX_Person_PrimaryPhoneNumber");

            // Index for household relationships
            builder.HasIndex(p => p.HouseholdId)
                .HasDatabaseName("IX_Person_HouseholdId");

            // Index for soft delete queries
            builder.HasIndex(p => p.IsDeleted)
                .HasDatabaseName("IX_Person_IsDeleted");

            // ==================== RELATIONSHIPS ====================

            // Relationship to Household (as a member) - configured in HouseholdConfiguration

            // Relationship to PersonPropertyRelation (one-to-many)
            builder.HasMany(p => p.PropertyRelations)
                .WithOne(ppr => ppr.Person)
                .HasForeignKey(ppr => ppr.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship to Evidence (one-to-many)
            builder.HasMany(p => p.Evidences)
                .WithOne(e => e.Person)
                .HasForeignKey(e => e.PersonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Note: Claims relationship (Person as PrimaryClaimant) is configured in ClaimConfiguration
            // Note: HeadOfHouseholdPerson relationship is configured in HouseholdConfiguration
        }
    }
}