using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Person entity
/// Updated to include Email and renamed phone fields for frontend form
/// </summary>
public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        // Table name
        builder.ToTable("Persons");

        // Primary Key
        builder.HasKey(p => p.Id);

        // ==================== ARABIC NAMES (NULLABLE FOR OFFICE SURVEY) ====================

        builder.Property(p => p.FamilyNameArabic)
            .IsRequired(false)
            .HasMaxLength(100)
            .HasComment("الكنية - Family/Last name in Arabic");

        builder.Property(p => p.FirstNameArabic)
            .IsRequired(false)
            .HasMaxLength(100)
            .HasComment("الاسم الأول - First name in Arabic");

        builder.Property(p => p.FatherNameArabic)
            .IsRequired(false)
            .HasMaxLength(100)
            .HasComment("اسم الأب - Father's name in Arabic");

        builder.Property(p => p.MotherNameArabic)
            .HasMaxLength(100)
            .HasComment("الاسم الأم - Mother's name in Arabic");

        // ==================== IDENTIFICATION ====================

        builder.Property(p => p.NationalId)
            .HasMaxLength(50)
            .HasComment("الرقم الوطني - National ID or identification number");

        builder.Property(p => p.DateOfBirth)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false)
            .HasComment("تاريخ الميلاد - Date of birth (full date or year-only)");

        // ==================== CONTACT INFORMATION ====================

        builder.Property(p => p.Email)
            .HasMaxLength(255)
            .HasComment("البريد الالكتروني - Email address");

        builder.Property(p => p.MobileNumber)
            .HasMaxLength(20)
            .HasComment("رقم الموبايل - Mobile phone number");

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(20)
            .HasComment("رقم الهاتف - Landline phone number");

        // ==================== LEGACY FIELDS (for future expansion) ====================

        builder.Property(p => p.FullNameEnglish)
            .HasMaxLength(300)
            .HasComment("Full name in English (optional)");

        builder.Property(p => p.Gender)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired(false)
            .HasComment("Gender (enum converted to string)");

        builder.Property(p => p.Nationality)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired(false)
            .HasComment("Nationality (enum converted to string)");

        builder.Property(p => p.IsContactPerson)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicates if this person is the main contact");

        // ==================== HOUSEHOLD RELATIONSHIP ====================

        builder.Property(p => p.HouseholdId)
            .HasComment("Foreign key to household (nullable)");

        builder.Property(p => p.RelationshipToHead)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired(false)
            .HasComment("Relationship to head of household (enum converted to string)");

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

        // Index for mobile number searches
        builder.HasIndex(p => p.MobileNumber)
            .HasDatabaseName("IX_Person_MobileNumber");

        // Index for email searches
        builder.HasIndex(p => p.Email)
            .HasDatabaseName("IX_Person_Email");

        // Index for household relationships
        builder.HasIndex(p => p.HouseholdId)
            .HasDatabaseName("IX_Person_HouseholdId");

        // Index for soft delete queries
        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("IX_Person_IsDeleted");

        // ==================== RELATIONSHIPS ====================

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
    }
}