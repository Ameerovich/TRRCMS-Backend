using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingPerson entity.
/// Mirrors the Person production table in an isolated staging area.
/// </summary>
public class StagingPersonConfiguration : IEntityTypeConfiguration<StagingPerson>
{
    public void Configure(EntityTypeBuilder<StagingPerson> builder)
    {
        builder.ToTable("StagingPersons");

        // Primary Key
        builder.HasKey(p => p.Id);

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

        builder.Property(p => p.NationalId)
            .HasMaxLength(50)
            .HasComment("Primary key for duplicate detection");

        builder.Property(p => p.DateOfBirth)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false)
            .HasComment("Date of birth — used in duplicate detection composite with name+gender");

        builder.Property(p => p.Email)
            .HasMaxLength(256);

        builder.Property(p => p.MobileNumber)
            .HasMaxLength(20);

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(p => p.Gender)
            .IsRequired(false)
            .HasComment("الجنس - Gender enum stored as integer");

        builder.Property(p => p.Nationality)
            .IsRequired(false)
            .HasComment("الجنسية - Nationality enum stored as integer");

        builder.Property(p => p.OriginalHouseholdId)
            .HasComment("Original Household UUID from .uhc — not a FK to production Households");

        builder.Property(p => p.RelationshipToHead)
            .IsRequired(false)
            .HasComment("صلة القرابة برب الأسرة - Relationship to head of household enum stored as integer");

        builder.Property(p => p.IsContactPerson)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(p => p.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.ImportPackageId)
            .HasDatabaseName("IX_StagingPersons_ImportPackageId");

        builder.HasIndex(p => new { p.ImportPackageId, p.ValidationStatus })
            .HasDatabaseName("IX_StagingPersons_ImportPackageId_ValidationStatus");

        builder.HasIndex(p => new { p.ImportPackageId, p.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingPersons_ImportPackageId_OriginalEntityId");

        // NationalId for intra-batch duplicate detection
        builder.HasIndex(p => p.NationalId)
            .HasDatabaseName("IX_StagingPersons_NationalId");

        // For household structure validation: find persons by their household
        builder.HasIndex(p => new { p.ImportPackageId, p.OriginalHouseholdId })
            .HasDatabaseName("IX_StagingPersons_ImportPackageId_OriginalHouseholdId");
    }
}
