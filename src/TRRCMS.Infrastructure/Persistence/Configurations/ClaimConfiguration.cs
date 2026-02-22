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
        
        // ==================== CLAIM CLASSIFICATION ====================
        
        builder.Property(c => c.ClaimType)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Claim type from controlled vocabulary - e.g., Ownership Claim, Occupancy Claim (نوع المطالبة)");
        
        builder.Property(c => c.ClaimSource)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Claim source - How claim entered system: 1=FieldCollection, 2=OfficeSubmission, 3=SystemImport, 4=Migration, 5=OnlinePortal, 6=ApiIntegration, 7=ManualEntry (مصدر المطالبة)");
        
        builder.Property(c => c.Priority)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Priority level: 1=Low, 2=Normal, 3=Medium, 4=High, 5=Critical, 6=VIP, 7=Escalated (الأولوية)");
        
        // ==================== LIFECYCLE MANAGEMENT ====================
        
        builder.Property(c => c.LifecycleStage)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Current lifecycle stage: 1=DraftPendingSubmission, 2=Submitted, 3=InitialScreening, 4=UnderReview, 5=AwaitingDocuments, 6=ConflictDetected, 7=InAdjudication, 8=Approved, 9=Rejected, 10=CertificateIssued, 11=Archived (مرحلة دورة الحياة)");
        
        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Legacy status: 1=Draft, 2=Finalized, 3=UnderReview, 4=Approved, 5=Rejected, 6=PendingEvidence, 7=Disputed, 99=Archived (الحالة)");
        
        builder.Property(c => c.SubmittedDate)
            .HasColumnType("timestamp with time zone")
            .HasComment("Date when claim was submitted (تاريخ التقديم)");
        
        builder.Property(c => c.SubmittedByUserId)
            .HasComment("User who submitted the claim (معرف المستخدم المقدم)");
        
        builder.Property(c => c.DecisionDate)
            .HasColumnType("timestamp with time zone")
            .HasComment("Date when final decision was made (تاريخ القرار)");
        
        builder.Property(c => c.DecisionByUserId)
            .HasComment("User who made final decision (معرف المستخدم القرار)");
        
        // ==================== ASSIGNMENT & WORKFLOW ====================
        
        builder.Property(c => c.AssignedToUserId)
            .HasComment("Currently assigned case officer (معرف الموظف المسؤول)");
        
        builder.Property(c => c.AssignedDate)
            .HasColumnType("timestamp with time zone")
            .HasComment("Date when assigned to current officer (تاريخ التعيين)");
        
        builder.Property(c => c.TargetCompletionDate)
            .HasColumnType("timestamp with time zone")
            .HasComment("Target completion/decision date (تاريخ الإنجاز المستهدف)");
        
        // ==================== TENURE DETAILS ====================
        
        builder.Property(c => c.TenureContractType)
            .HasConversion<int?>()
            .HasComment("Type of tenure contract: 1=Freehold, 2=Leasehold, 3=SharedOwnership, 4=Rental, 5=Informal, 6=Customary, 7=Usufruct, 99=Other (نوع عقد الحيازة)");
        
        builder.Property(c => c.OwnershipShare)
            .HasComment("Ownership share - Fraction out of 2400 (e.g., 1200 = 50%) (نصيب الملكية)");
        
        builder.Property(c => c.TenureStartDate)
            .HasColumnType("timestamp with time zone")
            .HasComment("Date from which tenure/occupancy started (تاريخ بدء الحيازة)");
        
        builder.Property(c => c.TenureEndDate)
            .HasColumnType("timestamp with time zone")
            .HasComment("Date when tenure/occupancy ended (تاريخ انتهاء الحيازة)");
        
        // ==================== CLAIM DETAILS ====================
        
        builder.Property(c => c.ClaimDescription)
            .HasMaxLength(5000)
            .HasComment("Detailed description of the claim (وصف المطالبة)");
        
        builder.Property(c => c.LegalBasis)
            .HasMaxLength(2000)
            .HasComment("Legal basis for the claim (الأساس القانوني)");
        
        builder.Property(c => c.SupportingNarrative)
            .HasMaxLength(5000)
            .HasComment("Supporting narrative or story (السرد الداعم)");
        
        // ==================== CONFLICT & DISPUTES ====================
        
        builder.Property(c => c.HasConflicts)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicates if there are conflicting claims (وجود تعارضات)");
        
        builder.Property(c => c.ConflictCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of conflicting claims detected (عدد المطالبات المتعارضة)");
        
        builder.Property(c => c.ConflictResolutionStatus)
            .HasMaxLength(100)
            .HasComment("Conflict resolution status - e.g., Pending, Resolved (حالة حل التعارض)");
        
        // ==================== EVIDENCE & DOCUMENTATION ====================
        
        builder.Property(c => c.EvidenceCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of evidence items attached (عدد الأدلة)");
        
        builder.Property(c => c.AllRequiredDocumentsSubmitted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicates if all required documents are submitted (جميع المستندات المطلوبة مقدمة)");
        
        builder.Property(c => c.MissingDocuments)
            .HasMaxLength(2000)
            .HasComment("List of missing document types - stored as JSON (المستندات المفقودة)");
        
        // ==================== REVIEW & VERIFICATION ====================
        
        builder.Property(c => c.VerificationStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Verification status: 1=Pending, 2=Verified, 3=Rejected, 4=RequiresAdditionalInfo (حالة التحقق)");
        
        builder.Property(c => c.VerificationDate)
            .HasColumnType("timestamp with time zone")
            .HasComment("Date when verification was completed (تاريخ التحقق)");
        
        builder.Property(c => c.VerifiedByUserId)
            .HasComment("User who verified the claim (معرف المستخدم المحقق)");
        
        builder.Property(c => c.VerificationNotes)
            .HasMaxLength(2000)
            .HasComment("Verification notes (ملاحظات التحقق)");
        
        // ==================== DECISION & OUTCOME ====================
        
        builder.Property(c => c.FinalDecision)
            .HasMaxLength(200)
            .HasComment("Final decision on the claim - e.g., Approved, Rejected (القرار النهائي)");
        
        builder.Property(c => c.DecisionReason)
            .HasMaxLength(2000)
            .HasComment("Reason for approval or rejection (سبب القرار)");
        
        builder.Property(c => c.DecisionNotes)
            .HasMaxLength(2000)
            .HasComment("Decision notes (ملاحظات القرار)");
        
        // ==================== CERTIFICATE ====================
        
        builder.Property(c => c.CertificateStatus)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Certificate status: 1=NotRequired, 2=PendingGeneration, 3=Generated, 4=Issued, 5=Collected, 6=Rejected, 7=Revoked (حالة الشهادة)");
        
        // Certificate relationship will be configured when Certificate entity is implemented
        builder.Ignore(c => c.CertificateId);
        builder.Ignore(c => c.Certificate);
        
        // ==================== NOTES & HISTORY ====================
        
        builder.Property(c => c.ProcessingNotes)
            .HasMaxLength(5000)
            .HasComment("Internal processing notes (ملاحظات المعالجة الداخلية)");
        
        builder.Property(c => c.PublicRemarks)
            .HasMaxLength(2000)
            .HasComment("Public remarks visible to claimant (الملاحظات العامة)");
        
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
        
        // Referrals relationship (collection)
        builder.HasMany(c => c.Referrals)
            .WithOne(r => r.Claim)
            .HasForeignKey(r => r.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
        
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
        
        // Index on AssignedToUserId (for officer workload)
        builder.HasIndex(c => c.AssignedToUserId)
            .HasDatabaseName("IX_Claims_AssignedToUserId");
        
        // Index on LifecycleStage (for workflow management)
        builder.HasIndex(c => c.LifecycleStage)
            .HasDatabaseName("IX_Claims_LifecycleStage");
        
        // Index on Status (for dashboard and filtering)
        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_Claims_Status");
        
        // Index on Priority (for prioritization)
        builder.HasIndex(c => c.Priority)
            .HasDatabaseName("IX_Claims_Priority");
        
        // Index on VerificationStatus (for verification queue)
        builder.HasIndex(c => c.VerificationStatus)
            .HasDatabaseName("IX_Claims_VerificationStatus");
        
        // Index on CertificateStatus (for certificate processing)
        builder.HasIndex(c => c.CertificateStatus)
            .HasDatabaseName("IX_Claims_CertificateStatus");
        
        // Index on HasConflicts (for adjudication queue)
        builder.HasIndex(c => c.HasConflicts)
            .HasDatabaseName("IX_Claims_HasConflicts");
        
        // Index on IsDeleted (for soft delete filtering)
        builder.HasIndex(c => c.IsDeleted)
            .HasDatabaseName("IX_Claims_IsDeleted");
        
        // Index on SubmittedDate (for date-based queries)
        builder.HasIndex(c => c.SubmittedDate)
            .HasDatabaseName("IX_Claims_SubmittedDate");
        
        // Index on TargetCompletionDate (for overdue tracking)
        builder.HasIndex(c => c.TargetCompletionDate)
            .HasDatabaseName("IX_Claims_TargetCompletionDate");
        
        // Index on DecisionDate (for reporting)
        builder.HasIndex(c => c.DecisionDate)
            .HasDatabaseName("IX_Claims_DecisionDate");
        
        // ==================== COMPOSITE INDEXES ====================
        
        // Composite index on LifecycleStage + AssignedToUserId (for officer's workload by stage)
        builder.HasIndex(c => new { c.LifecycleStage, c.AssignedToUserId })
            .HasDatabaseName("IX_Claims_LifecycleStage_AssignedToUserId");
        
        // Composite index on Priority + TargetCompletionDate (for priority + deadline sorting)
        builder.HasIndex(c => new { c.Priority, c.TargetCompletionDate })
            .HasDatabaseName("IX_Claims_Priority_TargetCompletionDate");
        
        // Composite index on HasConflicts + LifecycleStage (for conflict resolution workflow)
        builder.HasIndex(c => new { c.HasConflicts, c.LifecycleStage })
            .HasDatabaseName("IX_Claims_HasConflicts_LifecycleStage");
        
        // Composite index on IsDeleted + LifecycleStage (for filtered queries)
        builder.HasIndex(c => new { c.IsDeleted, c.LifecycleStage })
            .HasDatabaseName("IX_Claims_IsDeleted_LifecycleStage");
        
        // Composite index on PropertyUnitId + IsDeleted (for conflict detection with soft delete filter)
        builder.HasIndex(c => new { c.PropertyUnitId, c.IsDeleted })
            .HasDatabaseName("IX_Claims_PropertyUnitId_IsDeleted");
    }
}
