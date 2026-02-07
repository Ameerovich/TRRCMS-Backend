using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations.Staging;

/// <summary>
/// EF Core configuration for StagingSurvey entity.
/// Mirrors the Survey production table in an isolated staging area.
/// Records are validated before commit to production (FSD FR-D-4).
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingSurveyConfiguration : IEntityTypeConfiguration<StagingSurvey>
{
    public void Configure(EntityTypeBuilder<StagingSurvey> builder)
    {
        builder.ToTable("StagingSurveys");

        // Primary Key
        builder.HasKey(s => s.Id);

        // ==================== STAGING METADATA (from BaseStagingEntity) ====================

        builder.Property(s => s.ImportPackageId)
            .IsRequired();

        builder.Property(s => s.OriginalEntityId)
            .IsRequired();

        builder.Property(s => s.ValidationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(StagingValidationStatus.Pending);

        builder.Property(s => s.ValidationErrors)
            .HasMaxLength(8000)
            .HasComment("JSON array of blocking validation error messages");

        builder.Property(s => s.ValidationWarnings)
            .HasMaxLength(8000)
            .HasComment("JSON array of non-blocking validation warning messages");

        builder.Property(s => s.IsApprovedForCommit)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.CommittedEntityId);

        builder.Property(s => s.StagedAtUtc)
            .IsRequired();

        // ==================== RELATIONSHIPS (original UUIDs from .uhc) ====================

        builder.Property(s => s.OriginalBuildingId)
            .IsRequired()
            .HasComment("Original Building UUID from .uhc — not a FK to production Buildings");

        builder.Property(s => s.OriginalPropertyUnitId)
            .HasComment("Original PropertyUnit UUID from .uhc");

        builder.Property(s => s.OriginalFieldCollectorId)
            .HasComment("Optional — derived from user context during import, not from package");

        builder.Property(s => s.OriginalClaimId)
            .HasComment("Original Claim UUID from .uhc");

        // ==================== SURVEY IDENTIFICATION ====================

        builder.Property(s => s.ReferenceCode)
            .HasMaxLength(50)
            .HasComment("Optional — auto-generated during commit");

        // ==================== SURVEY CLASSIFICATION ====================

        builder.Property(s => s.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasComment("Optional — auto-set (Field/Office) during commit");

        builder.Property(s => s.Source)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasComment("Optional — auto-set during commit");

        builder.Property(s => s.SurveyTypeName)
            .HasMaxLength(200)
            .HasComment("Optional — auto-set during commit");

        // ==================== SURVEY DETAILS ====================

        builder.Property(s => s.SurveyDate)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasComment("Optional — auto-set to Draft during commit");

        builder.Property(s => s.GpsCoordinates)
            .HasMaxLength(100);

        builder.Property(s => s.IntervieweeName)
            .HasMaxLength(200);

        builder.Property(s => s.IntervieweeRelationship)
            .HasMaxLength(100);

        builder.Property(s => s.Notes)
            .HasMaxLength(4000);

        // ==================== OFFICE SURVEY SPECIFIC ====================

        builder.Property(s => s.OfficeLocation)
            .HasMaxLength(200);

        builder.Property(s => s.RegistrationNumber)
            .HasMaxLength(50);

        builder.Property(s => s.AppointmentReference)
            .HasMaxLength(50);

        builder.Property(s => s.ContactPhone)
            .HasMaxLength(20);

        builder.Property(s => s.ContactEmail)
            .HasMaxLength(256);

        // ==================== CONCURRENCY ====================

        builder.Property(s => s.RowVersion)
            .IsRowVersion();

        // ==================== RELATIONSHIPS ====================

        builder.HasOne<ImportPackage>()
            .WithMany()
            .HasForeignKey(s => s.ImportPackageId)
            .OnDelete(DeleteBehavior.Cascade);

        // ==================== INDEXES ====================

        builder.HasIndex(s => s.ImportPackageId)
            .HasDatabaseName("IX_StagingSurveys_ImportPackageId");

        builder.HasIndex(s => new { s.ImportPackageId, s.ValidationStatus })
            .HasDatabaseName("IX_StagingSurveys_ImportPackageId_ValidationStatus");

        builder.HasIndex(s => new { s.ImportPackageId, s.OriginalEntityId })
            .IsUnique()
            .HasDatabaseName("IX_StagingSurveys_ImportPackageId_OriginalEntityId");

        // For cross-entity validation: find surveys by building
        builder.HasIndex(s => new { s.ImportPackageId, s.OriginalBuildingId })
            .HasDatabaseName("IX_StagingSurveys_ImportPackageId_OriginalBuildingId");

        // Reference code for duplicate detection
        builder.HasIndex(s => s.ReferenceCode)
            .HasDatabaseName("IX_StagingSurveys_ReferenceCode");
    }
}
