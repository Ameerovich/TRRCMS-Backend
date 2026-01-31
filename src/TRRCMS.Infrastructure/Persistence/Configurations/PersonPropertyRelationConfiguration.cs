using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

public class PersonPropertyRelationConfiguration : IEntityTypeConfiguration<PersonPropertyRelation>
{
    public void Configure(EntityTypeBuilder<PersonPropertyRelation> builder)
    {
        builder.ToTable("PersonPropertyRelations");
        builder.HasKey(ppr => ppr.Id);

        builder.Property(ppr => ppr.PersonId)
            .IsRequired()
            .HasComment("Foreign key to Person");

        builder.Property(ppr => ppr.PropertyUnitId)
            .IsRequired()
            .HasComment("Foreign key to PropertyUnit");

        // RelationType stored as int in database (enum conversion)
        builder.Property(ppr => ppr.RelationType)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("نوع العلاقة - Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99");

        builder.Property(ppr => ppr.RelationTypeOtherDesc)
            .IsRequired(false)
            .HasMaxLength(500)
            .HasComment("Description when relation type is 'Other'");

        // ContractType stored as int (nullable enum conversion)
        builder.Property(ppr => ppr.ContractType)
            .IsRequired(false)
            .HasConversion<int?>()
            .HasComment("نوع العقد - FullOwnership=1, SharedOwnership=2, LongTermRental=3, etc.");

        builder.Property(ppr => ppr.ContractTypeOtherDesc)
            .IsRequired(false)
            .HasMaxLength(500)
            .HasComment("Description when contract type is 'Other'");

        builder.Property(ppr => ppr.OwnershipShare)
            .HasPrecision(18, 4)
            .HasComment("حصة الملكية - Ownership share (0.0 to 1.0)");

        builder.Property(ppr => ppr.ContractDetails)
            .IsRequired(false)
            .HasMaxLength(2000)
            .HasComment("Contract or agreement details");

        builder.Property(ppr => ppr.StartDate)
            .IsRequired(false)
            .HasComment("تاريخ بدء العلاقة - Start date of the relation");

        builder.Property(ppr => ppr.EndDate)
            .IsRequired(false)
            .HasComment("End date of the relation");

        builder.Property(ppr => ppr.Notes)
            .IsRequired(false)
            .HasMaxLength(2000)
            .HasComment("ملاحظاتك - Additional notes about this relation");

        builder.Property(ppr => ppr.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Indicates if this relation is currently active");

        // Audit fields
        builder.Property(ppr => ppr.CreatedAtUtc).IsRequired().HasComment("Creation timestamp (UTC)");
        builder.Property(ppr => ppr.CreatedBy).IsRequired().HasComment("User who created this record");
        builder.Property(ppr => ppr.LastModifiedAtUtc).IsRequired(false).HasComment("Last modification timestamp (UTC)");
        builder.Property(ppr => ppr.LastModifiedBy).IsRequired(false).HasComment("User who last modified this record");
        builder.Property(ppr => ppr.IsDeleted).IsRequired().HasDefaultValue(false).HasComment("Soft delete flag");
        builder.Property(ppr => ppr.DeletedAtUtc).IsRequired(false).HasComment("Deletion timestamp (UTC)");
        builder.Property(ppr => ppr.DeletedBy).IsRequired(false).HasComment("User who deleted this record");
        builder.Property(ppr => ppr.RowVersion).IsRowVersion().HasComment("Concurrency token");

        // Indexes
        builder.HasIndex(ppr => ppr.PersonId).HasDatabaseName("IX_PersonPropertyRelation_PersonId");
        builder.HasIndex(ppr => ppr.PropertyUnitId).HasDatabaseName("IX_PersonPropertyRelation_PropertyUnitId");
        builder.HasIndex(ppr => new { ppr.PersonId, ppr.PropertyUnitId }).HasDatabaseName("IX_PersonPropertyRelation_PersonId_PropertyUnitId");
        builder.HasIndex(ppr => ppr.IsActive).HasDatabaseName("IX_PersonPropertyRelation_IsActive");
        builder.HasIndex(ppr => ppr.IsDeleted).HasDatabaseName("IX_PersonPropertyRelation_IsDeleted");
        builder.HasIndex(ppr => new { ppr.IsActive, ppr.IsDeleted }).HasDatabaseName("IX_PersonPropertyRelation_IsActive_IsDeleted");
        builder.HasIndex(ppr => ppr.RelationType).HasDatabaseName("IX_PersonPropertyRelation_RelationType");
        builder.HasIndex(ppr => ppr.ContractType).HasDatabaseName("IX_PersonPropertyRelation_ContractType");

        // Relationships
        builder.HasOne(ppr => ppr.Person)
            .WithMany(p => p.PropertyRelations)
            .HasForeignKey(ppr => ppr.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ppr => ppr.PropertyUnit)
            .WithMany(p => p.PersonRelations)
            .HasForeignKey(ppr => ppr.PropertyUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ppr => ppr.Evidences)
            .WithOne(e => e.PersonPropertyRelation)
            .HasForeignKey(e => e.PersonPropertyRelationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
