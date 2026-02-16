using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ConflictResolution entity.
/// Defines table mapping, indexes, max lengths, and column comments
/// for the duplicate detection and conflict resolution workflow.
/// 
/// Key indexes:
/// - Composite on (ConflictType, Status) — primary query: "show pending person duplicates"
/// - ImportPackageId — "show all conflicts for this import"
/// - AssignedToUserId + Status — "show my pending conflict queue"
/// - DetectedDate — chronological ordering
/// 
/// Referenced in UC-007 (Resolve Duplicate Properties) and UC-008 (Resolve Person Duplicates).
/// </summary>
public class ConflictResolutionConfiguration : IEntityTypeConfiguration<ConflictResolution>
{
    public void Configure(EntityTypeBuilder<ConflictResolution> builder)
    {
        builder.ToTable("ConflictResolutions");

        // Primary Key
        builder.HasKey(c => c.Id);

        // ==================== CONFLICT IDENTIFICATION ====================

        builder.Property(c => c.ConflictNumber)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("Human-readable conflict number (CNF-YYYY-NNNN)");

        builder.Property(c => c.ConflictType)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("PersonDuplicate, PropertyDuplicate, ClaimConflict");

        // ==================== ENTITY REFERENCES ====================

        builder.Property(c => c.EntityType)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Type of entities in conflict: Person, PropertyUnit, Building");

        builder.Property(c => c.FirstEntityId)
            .IsRequired();

        builder.Property(c => c.SecondEntityId)
            .IsRequired();

        builder.Property(c => c.FirstEntityIdentifier)
            .HasMaxLength(200)
            .HasComment("Human-readable identifier for first entity (e.g. name, building ID)");

        builder.Property(c => c.SecondEntityIdentifier)
            .HasMaxLength(200)
            .HasComment("Human-readable identifier for second entity");

        builder.Property(c => c.ImportPackageId)
            .HasComment("Import package that triggered this conflict (null for manual detections)");

        // ==================== CONFLICT DETAILS ====================

        builder.Property(c => c.SimilarityScore)
            .IsRequired()
            .HasPrecision(5, 2)
            .HasComment("Similarity score 0-100% — higher means more likely duplicate");

        builder.Property(c => c.ConfidenceLevel)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("Low, Medium, High");

        builder.Property(c => c.ConflictDescription)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.MatchingCriteria)
            .HasMaxLength(4000)
            .HasComment("JSON: {\"national_id\": \"match\", \"name_similarity\": \"95%\", ...}");

        builder.Property(c => c.DataComparison)
            .HasComment("JSON: side-by-side field comparison of conflicting entities");

        // ==================== RESOLUTION STATUS ====================

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasComment("PendingReview, Resolved, Ignored");

        builder.Property(c => c.ResolutionAction)
            .HasComment("KeepBoth, Merge, KeepFirst, KeepSecond, Ignored, etc. - stored as integer");

        builder.Property(c => c.DetectedDate)
            .IsRequired();

        builder.Property(c => c.DetectedByUserId);

        builder.Property(c => c.AssignedDate);

        builder.Property(c => c.AssignedToUserId);

        builder.Property(c => c.ResolvedDate);

        builder.Property(c => c.ResolvedByUserId);

        // ==================== RESOLUTION DETAILS ====================

        builder.Property(c => c.ResolutionReason)
            .HasMaxLength(2000);

        builder.Property(c => c.ResolutionNotes)
            .HasMaxLength(4000);

        builder.Property(c => c.MergedEntityId)
            .HasComment("ID of the resulting merged entity (if Merge action)");

        builder.Property(c => c.DiscardedEntityId)
            .HasComment("ID of the discarded entity (if Merge action)");

        builder.Property(c => c.MergeMapping)
            .HasComment("JSON: which fields came from which entity — audit trail");

        // ==================== PRIORITY & SLA ====================

        builder.Property(c => c.Priority)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("Low, Normal, High, Critical");

        builder.Property(c => c.TargetResolutionHours)
            .HasComment("SLA target in hours for conflict resolution");

        builder.Property(c => c.IsOverdue)
            .IsRequired()
            .HasDefaultValue(false);

        // ==================== AUTOMATED VS MANUAL ====================

        builder.Property(c => c.IsAutoDetected)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.IsAutoResolved)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.AutoResolutionRule)
            .HasMaxLength(500)
            .HasComment("Name of the auto-resolution rule applied (if automated)");

        // ==================== ESCALATION ====================

        builder.Property(c => c.IsEscalated)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.EscalationReason)
            .HasMaxLength(2000);

        builder.Property(c => c.EscalatedDate);

        builder.Property(c => c.EscalatedByUserId);

        // ==================== REVIEW TRACKING ====================

        builder.Property(c => c.ReviewAttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.ReviewHistory)
            .HasComment("JSON array: [{\"AttemptNumber\":1, \"Date\":\"...\", \"Notes\":\"...\"}]");

        // ==================== AUDIT FIELDS (from BaseAuditableEntity) ====================

        builder.Property(c => c.CreatedAtUtc).IsRequired();
        builder.Property(c => c.CreatedBy).IsRequired();
        builder.Property(c => c.IsDeleted).IsRequired().HasDefaultValue(false);

        // ==================== CONCURRENCY ====================

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // ==================== RELATIONSHIPS ====================

        builder.HasOne(c => c.ImportPackage)
            .WithMany()
            .HasForeignKey(c => c.ImportPackageId)
            .OnDelete(DeleteBehavior.SetNull);

        // ==================== INDEXES ====================

        // Primary query: "show pending person duplicates"
        builder.HasIndex(c => new { c.ConflictType, c.Status })
            .HasDatabaseName("IX_ConflictResolutions_ConflictType_Status");

        // All conflicts for a specific import package
        builder.HasIndex(c => c.ImportPackageId)
            .HasDatabaseName("IX_ConflictResolutions_ImportPackageId");

        // Conflict queue for a specific reviewer
        builder.HasIndex(c => new { c.AssignedToUserId, c.Status })
            .HasDatabaseName("IX_ConflictResolutions_AssignedToUserId_Status");

        // Chronological ordering and SLA tracking
        builder.HasIndex(c => c.DetectedDate)
            .HasDatabaseName("IX_ConflictResolutions_DetectedDate");

        // Entity pair lookup: prevent duplicate conflict records for same entity pair
        builder.HasIndex(c => new { c.FirstEntityId, c.SecondEntityId })
            .HasDatabaseName("IX_ConflictResolutions_FirstEntityId_SecondEntityId");

        // Soft-delete filter
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
