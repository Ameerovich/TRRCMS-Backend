using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Claim entity
/// Configures table structure, relationships, indexes, and constraints
/// </summary>
public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        // ==================== TABLE CONFIGURATION ====================

        builder.ToTable("Claims");

        // ==================== PRIMARY KEY ====================

        builder.HasKey(c => c.Id);

        // ==================== IDENTIFIERS ====================

        builder.Property(c => c.ClaimNumber)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Unique claim identifier - Format: CLM-YYYY-NNNNNNNNN (رقم المطالبة)");

        builder.Property(c => c.PropertyUnitId)
            .IsRequired()
            .HasComment("Foreign key to PropertyUnit - Property being claimed (معرف الوحدة العقارية)");

        builder.Property(c => c.PrimaryClaimantId)
            .HasComment("Foreign key to Person - Primary claimant (معرف المدعي الأساسي)");

        builder.Property(c => c.OriginatingSurveyId)
            .HasComment("Foreign key to Survey that originated this claim (معرف الزيارة المنشئة)");

        // ==================== CLAIM CLASSIFICATION ====================

        builder.Property(c => c.ClaimType)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Claim type: 1=OwnershipClaim (مطالبة ملكية), 2=OccupancyClaim (مطالبة إشغال)");

        builder.Property(c => c.ClaimSource)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Claim source - How claim entered system: 1=FieldCollection, 2=OfficeSubmission, 3=SystemImport, 4=Migration, 5=OnlinePortal, 6=ApiIntegration, 7=ManualEntry (مصدر المطالبة)");

        // ==================== LIFECYCLE MANAGEMENT ====================

        builder.Property(c => c.CaseStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Case status: 1=Open (حالة مفتوحة), 2=Closed (حالة مغلقة)");

        builder.Property(c => c.SubmittedDate)
            .HasColumnType("timestamp with time zone")
            .HasComment("Date when claim was submitted (تاريخ التقديم)");

        builder.Property(c => c.SubmittedByUserId)
            .HasComment("User who submitted the claim (معرف المستخدم المقدم)");

        // ==================== TENURE DETAILS ====================

        builder.Property(c => c.TenureContractType)
            .HasConversion<int?>()
            .HasComment("Type of tenure contract: 1=Freehold, 2=Leasehold, 3=SharedOwnership, 4=Rental, 5=Informal, 6=Customary, 7=Usufruct, 99=Other (نوع عقد الحيازة)");

        builder.Property(c => c.OwnershipShare)
            .HasComment("Ownership share - Fraction out of 2400 (e.g., 1200 = 50%) (نصيب الملكية)");

        // ==================== CLAIM DETAILS ====================

        builder.Property(c => c.ClaimDescription)
            .HasMaxLength(5000)
            .HasComment("Detailed description of the claim (وصف المطالبة)");

        // ==================== AUDIT FIELDS ====================

        builder.Property(c => c.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasComment("UTC timestamp when record was created (تاريخ الإنشاء)");

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasComment("User ID who created this record (معرف المستخدم المنشئ)");

        builder.Property(c => c.LastModifiedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasComment("UTC timestamp when record was last modified (تاريخ آخر تعديل)");

        builder.Property(c => c.LastModifiedBy)
            .HasComment("User ID who last modified this record (معرف المستخدم المعدل)");

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Soft delete flag (علامة الحذف المنطقي)");

        builder.Property(c => c.DeletedAtUtc)
            .HasColumnType("timestamp with time zone")
            .HasComment("UTC timestamp when record was soft deleted (تاريخ الحذف)");

        builder.Property(c => c.DeletedBy)
            .HasComment("User ID who soft deleted this record (معرف المستخدم الحاذف)");

        // ==================== CONCURRENCY ====================

        builder.Property(c => c.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken()
            .HasComment("Concurrency token for optimistic locking (رمز التزامن)");

        // ==================== RELATIONSHIPS ====================

        // PropertyUnit relationship (required)
        builder.HasOne(c => c.PropertyUnit)
            .WithMany(p => p.Claims)  // ✅ FIXED: Explicit navigation property
            .HasForeignKey(c => c.PropertyUnitId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // PrimaryClaimant relationship (optional)
        builder.HasOne(c => c.PrimaryClaimant)
            .WithMany()
            .HasForeignKey(c => c.PrimaryClaimantId)
            .OnDelete(DeleteBehavior.Restrict);

        // OriginatingSurvey relationship (optional — no navigation on Survey side)
        builder.HasOne<Domain.Entities.Survey>()
            .WithMany()
            .HasForeignKey(c => c.OriginatingSurveyId)
            .OnDelete(DeleteBehavior.SetNull);

        // Evidences relationship (collection)
        builder.HasMany(c => c.Evidences)
            .WithOne(e => e.Claim)
            .HasForeignKey(e => e.ClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        // Documents relationship (collection)
        builder.HasMany(c => c.Documents)
            .WithOne(d => d.Claim)
            .HasForeignKey(d => d.ClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== INDEXES ====================

        // Unique index on ClaimNumber
        builder.HasIndex(c => c.ClaimNumber)
            .IsUnique()
            .HasDatabaseName("IX_Claims_ClaimNumber");

        // Index on PropertyUnitId (for conflict detection and queries)
        builder.HasIndex(c => c.PropertyUnitId)
            .HasDatabaseName("IX_Claims_PropertyUnitId");

        // Index on PrimaryClaimantId (for claimant view)
        builder.HasIndex(c => c.PrimaryClaimantId)
            .HasDatabaseName("IX_Claims_PrimaryClaimantId");

        // Index on OriginatingSurveyId (for finding all claims from a survey)
        builder.HasIndex(c => c.OriginatingSurveyId)
            .HasDatabaseName("IX_Claims_OriginatingSurveyId");

        // Index on CaseStatus (for dashboard and filtering)
        builder.HasIndex(c => c.CaseStatus)
            .HasDatabaseName("IX_Claims_CaseStatus");

        // Index on IsDeleted (for soft delete filtering)
        builder.HasIndex(c => c.IsDeleted)
            .HasDatabaseName("IX_Claims_IsDeleted");

        // Index on SubmittedDate (for date-based queries)
        builder.HasIndex(c => c.SubmittedDate)
            .HasDatabaseName("IX_Claims_SubmittedDate");

        // ==================== COMPOSITE INDEXES ====================

        // Composite index on PropertyUnitId + IsDeleted (for conflict detection with soft delete filter)
        builder.HasIndex(c => new { c.PropertyUnitId, c.IsDeleted })
            .HasDatabaseName("IX_Claims_PropertyUnitId_IsDeleted");
    }
}
