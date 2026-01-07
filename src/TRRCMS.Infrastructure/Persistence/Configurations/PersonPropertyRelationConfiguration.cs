using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for PersonPropertyRelation entity
/// </summary>
public class PersonPropertyRelationConfiguration : IEntityTypeConfiguration<PersonPropertyRelation>
{
    public void Configure(EntityTypeBuilder<PersonPropertyRelation> builder)
    {
        // Table name
        builder.ToTable("PersonPropertyRelations");

        // Primary key
        builder.HasKey(ppr => ppr.Id);

        // ==================== IDENTIFIERS ====================

        builder.Property(ppr => ppr.PersonId)
            .IsRequired()
            .HasComment("Foreign key to Person");

        builder.Property(ppr => ppr.PropertyUnitId)
            .IsRequired()
            .HasComment("Foreign key to PropertyUnit");

        // ==================== RELATION ATTRIBUTES ====================

        builder.Property(ppr => ppr.RelationType)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Type of relation (owner, tenant, occupant, guest, heir, other, etc.)");

        builder.Property(ppr => ppr.RelationTypeOtherDesc)
            .IsRequired(false)
            .HasMaxLength(200)
            .HasComment("Description when relation type is 'Other'");

        builder.Property(ppr => ppr.OwnershipShare)
            .HasPrecision(18, 4)
            .HasComment("Ownership or occupancy share (0.0 to 1.0 for percentage)");

        builder.Property(ppr => ppr.ContractDetails)
            .IsRequired(false)
            .HasMaxLength(2000)
            .HasComment("Contract or agreement details");

        builder.Property(ppr => ppr.StartDate)
            .IsRequired(false)
            .HasComment("Start date of the relation");

        builder.Property(ppr => ppr.EndDate)
            .IsRequired(false)
            .HasComment("End date of the relation (for terminated relations)");

        builder.Property(ppr => ppr.Notes)
            .IsRequired(false)
            .HasMaxLength(2000)
            .HasComment("Additional notes about this relation");

        builder.Property(ppr => ppr.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Indicates if this relation is currently active");

        // ==================== AUDIT FIELDS ====================

        builder.Property(ppr => ppr.CreatedAtUtc)
            .IsRequired()
            .HasComment("Creation timestamp (UTC)");

        builder.Property(ppr => ppr.CreatedBy)
            .IsRequired()
            .HasComment("User who created this record");

        builder.Property(ppr => ppr.LastModifiedAtUtc)
            .IsRequired(false)
            .HasComment("Last modification timestamp (UTC)");

        builder.Property(ppr => ppr.LastModifiedBy)
            .IsRequired(false)
            .HasComment("User who last modified this record");

        builder.Property(ppr => ppr.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Soft delete flag");

        builder.Property(ppr => ppr.DeletedAtUtc)
            .IsRequired(false)
            .HasComment("Deletion timestamp (UTC)");

        builder.Property(ppr => ppr.DeletedBy)
            .IsRequired(false)
            .HasComment("User who deleted this record");

        builder.Property(ppr => ppr.RowVersion)
            .IsRowVersion()
            .HasComment("Concurrency token");

        // ==================== INDEXES ====================

        // Index for person lookups
        builder.HasIndex(ppr => ppr.PersonId)
            .HasDatabaseName("IX_PersonPropertyRelation_PersonId");

        // Index for property unit lookups
        builder.HasIndex(ppr => ppr.PropertyUnitId)
            .HasDatabaseName("IX_PersonPropertyRelation_PropertyUnitId");

        // Composite index for person-property lookups
        builder.HasIndex(ppr => new { ppr.PersonId, ppr.PropertyUnitId })
            .HasDatabaseName("IX_PersonPropertyRelation_PersonId_PropertyUnitId");

        // Index for active relations queries
        builder.HasIndex(ppr => ppr.IsActive)
            .HasDatabaseName("IX_PersonPropertyRelation_IsActive");

        // Index for soft delete queries
        builder.HasIndex(ppr => ppr.IsDeleted)
            .HasDatabaseName("IX_PersonPropertyRelation_IsDeleted");

        // Composite index for active, non-deleted relations
        builder.HasIndex(ppr => new { ppr.IsActive, ppr.IsDeleted })
            .HasDatabaseName("IX_PersonPropertyRelation_IsActive_IsDeleted");

        // Index for relation type queries
        builder.HasIndex(ppr => ppr.RelationType)
            .HasDatabaseName("IX_PersonPropertyRelation_RelationType");

        // ==================== RELATIONSHIPS ====================

        // Relationship to Person (Many-to-One)
        builder.HasOne(ppr => ppr.Person)
            .WithMany(p => p.PropertyRelations)
            .HasForeignKey(ppr => ppr.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to PropertyUnit (Many-to-One)
        builder.HasOne(ppr => ppr.PropertyUnit)
            .WithMany()  // PropertyUnit doesn't have back-navigation to PersonPropertyRelations
            .HasForeignKey(ppr => ppr.PropertyUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to Evidence (One-to-Many)
        // Evidence.PersonPropertyRelationId → PersonPropertyRelation.Evidences
        builder.HasMany(ppr => ppr.Evidences)
            .WithOne(e => e.PersonPropertyRelation)
            .HasForeignKey(e => e.PersonPropertyRelationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}