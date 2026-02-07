using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingPerson entity.
/// Mirrors the Person production table in an isolated staging area.
/// Central to duplicate detection per FSD FR-D-5 (Person Matching).
/// Referenced in UC-003 Stage 2 and UC-008 (Resolve Person Duplicates).
/// </summary>
public class StagingPersonConfiguration : IEntityTypeConfiguration<StagingPerson>
{
    public void Configure(EntityTypeBuilder<StagingPerson> builder)
    {
        builder.ToTable("StagingPersons");

        // Primary Key
        builder.HasKey(p => p.Id);

        // ==================== STAGING METADATA (from BaseStagingEntity) ====================

        builder.Property(p => p.ImportPackageId)
            .IsRequired();

        builder.Property(p => p.OriginalEntityId)
            .IsRequired();

        builder.Property(p => p.ValidationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StagingValidationStatus.Pending);

        builder.Property(p => p.ValidationErrors)
            .HasMaxLength(8000)
            .HasComment("JSON array of blocking validation error messages");

        builder.Property(p => p.ValidationWarnings)
            .HasMaxLength(8000)
            .HasComment("JSON array of non-blocking validation warning messages");

        builder.Property(p => p.IsApprovedForCommit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.CommittedEntityId);

        builder.Property(p => p.StagedAtUtc)
            .IsRequired();

        // ==================== NAME COMPONENTS ====================

        builder.Property(p => p.FamilyNameArabic)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.FirstNameArabic)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.FatherNameArabic)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.MotherNameArabic)
            .HasMaxLength(100);

        // ==================== IDENTIFICATION ====================

        builder.Property(p => p.NationalId)
            .HasMaxLength(50)
            .HasComment("Primary key for duplicate detection (FR-D-5, §12.2.4)");

        builder.Property(p => p.YearOfBirth)
            .HasComment("Year of birth — used in duplicate detection composite with name+gender");

        // ==================== CONTACT ====================

        builder.Property(p => p.Email)
            .HasMaxLength(256);

        builder.Property(p => p.MobileNumber)
            .HasMaxLength(20);

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(20);

        // ==================== ADDITIONAL DETAILS ====================

        builder.Property(p => p.FullNameEnglish)
            .HasMaxLength(200);

        builder.Property(p => p.Gender)
            .HasMaxLength(20);

        builder.Property(p => p.Nationality)
            .HasMaxLength(100);

        // ==================== HOUSEHOLD LINK ====================

        builder.Property(p => p.OriginalHouseholdId)
            .HasComment("Original Household UUID from .uhc — not a FK to production Households");

        builder.Property(p => p.RelationshipToHead)
            .HasMaxLength(50);

        // ==================== CONCURRENCY ====================

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // ==================== RELATIONSHIPS ====================

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(p => p.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==================== INDEXES ====================

        builder.HasIndex(p => p.ImportPackageId)
            .HasDatabaseName("IX_StagingPersons_ImportPackageId");

        builder.HasIndex(p => new { p.ImportPackageId, p.ValidationStatus })
            .HasDatabaseName("IX_StagingPersons_ImportPackageId_ValidationStatus");

        builder.HasIndex(p => new { p.ImportPackageId, p.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingPersons_ImportPackageId_OriginalEntityId");

        // NationalId for intra-batch duplicate detection (§12.2.4) and cross-production matching (FR-D-5)
        builder.HasIndex(p => p.NationalId)
            .HasDatabaseName("IX_StagingPersons_NationalId");

        // For household structure validation: find persons by their household
        builder.HasIndex(p => new { p.ImportPackageId, p.OriginalHouseholdId })
            .HasDatabaseName("IX_StagingPersons_ImportPackageId_OriginalHouseholdId");
    }
}
