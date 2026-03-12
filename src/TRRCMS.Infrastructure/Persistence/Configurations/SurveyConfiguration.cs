using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Survey entity
/// Supports both Field (UC-001) and Office (UC-004) surveys
/// </summary>
public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.ToTable("Surveys");

        // Primary key
        builder.HasKey(s => s.Id);

        // ==================== IDENTIFIERS ====================

        builder.Property(s => s.ReferenceCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.ReferenceCode)
            .IsUnique()
            .HasDatabaseName("IX_Surveys_ReferenceCode");

        // ==================== RELATIONSHIPS ====================

        builder.Property(s => s.BuildingId)
            .IsRequired();

        builder.HasOne(s => s.Building)
            .WithMany()
            .HasForeignKey(s => s.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.PropertyUnitId);

        builder.HasOne(s => s.PropertyUnit)
            .WithMany()
            .HasForeignKey(s => s.PropertyUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.FieldCollectorId)
            .IsRequired();

        builder.HasOne(s => s.Collector)
            .WithMany()
            .HasForeignKey(s => s.FieldCollectorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== SURVEY CLASSIFICATION ====================

        builder.Property(s => s.Type)
            .IsRequired()
            .HasDefaultValue(SurveyType.Field)
            .HasConversion<int>();

        builder.Property(s => s.Source)
            .IsRequired()
            .HasDefaultValue(SurveySource.FieldCollection)
            .HasConversion<int>();

        // Index for filtering by type
        builder.HasIndex(s => s.Type)
            .HasDatabaseName("IX_Surveys_Type");

        // ==================== SURVEY DETAILS ====================

        builder.Property(s => s.SurveyDate)
            .IsRequired();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasDefaultValue(SurveyStatus.Draft)
            .HasConversion<int>();

        // Index for filtering by status
        builder.HasIndex(s => s.Status)
            .HasDatabaseName("IX_Surveys_Status");

        // Composite index for common queries
        builder.HasIndex(s => new { s.Type, s.Status, s.FieldCollectorId })
            .HasDatabaseName("IX_Surveys_Type_Status_Collector");

        builder.Property(s => s.GpsCoordinates)
            .HasMaxLength(100);

        builder.Property(s => s.IntervieweeName)
            .HasMaxLength(200);

        builder.Property(s => s.IntervieweeRelationship)
            .HasMaxLength(100);

        builder.Property(s => s.Notes)
            .HasMaxLength(4000);

        builder.Property(s => s.DurationMinutes);

        // ==================== OFFICE SURVEY SPECIFIC ====================

        builder.Property(s => s.OfficeLocation)
            .HasMaxLength(200);

        builder.Property(s => s.RegistrationNumber)
            .HasMaxLength(50);

        // Index for registration number lookups
        builder.HasIndex(s => s.RegistrationNumber)
            .HasDatabaseName("IX_Surveys_RegistrationNumber")
            .HasFilter("[RegistrationNumber] IS NOT NULL");

        builder.Property(s => s.AppointmentReference)
            .HasMaxLength(50);

        builder.Property(s => s.ContactPhone)
            .HasMaxLength(20);

        builder.Property(s => s.ContactEmail)
            .HasMaxLength(100);

        builder.Property(s => s.InPersonVisit);

        // ==================== CONTACT PERSON ====================

        builder.Property(s => s.ContactPersonId)
            .HasComment("FK to Person who is the contact person for this survey");

        builder.Property(s => s.ContactPersonFullName)
            .HasMaxLength(500)
            .HasComment("Denormalized contact person full name: firstname fathername familyname (mothername)");

        builder.HasOne(s => s.ContactPerson)
            .WithMany()
            .HasForeignKey(s => s.ContactPersonId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(s => s.ContactPersonId)
            .HasDatabaseName("IX_Surveys_ContactPersonId");

        // ==================== CLAIM LINKING ====================

        builder.Property(s => s.ClaimId);

        builder.HasOne(s => s.Claim)
            .WithMany()
            .HasForeignKey(s => s.ClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.ClaimCreatedDate);

        // ==================== AUDIT FIELDS ====================

        builder.Property(s => s.CreatedAtUtc)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .IsRequired();

        builder.Property(s => s.LastModifiedAtUtc);

        builder.Property(s => s.LastModifiedBy);

        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Index for date-based queries
        builder.HasIndex(s => s.SurveyDate)
            .HasDatabaseName("IX_Surveys_SurveyDate");

        // Index for collector-based queries
        builder.HasIndex(s => s.FieldCollectorId)
            .HasDatabaseName("IX_Surveys_FieldCollectorId");

        // Index for building-based queries
        builder.HasIndex(s => s.BuildingId)
            .HasDatabaseName("IX_Surveys_BuildingId");
    }
}
