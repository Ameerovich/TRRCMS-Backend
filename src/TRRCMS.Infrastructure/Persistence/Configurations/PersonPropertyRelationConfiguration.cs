using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Configurations
{
    public class PersonPropertyRelationConfiguration : IEntityTypeConfiguration<PersonPropertyRelation>
    {
        public void Configure(EntityTypeBuilder<PersonPropertyRelation> builder)
        {
            // Table name
            builder.ToTable("PersonPropertyRelations");

            // Primary Key
            builder.HasKey(ppr => ppr.Id);

            // ==================== PROPERTIES ====================

            builder.Property(ppr => ppr.RelationType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ppr => ppr.OwnershipShare)
                .HasPrecision(18, 4);

            builder.Property(ppr => ppr.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ==================== INDEXES ====================

            builder.HasIndex(ppr => ppr.PersonId)
                .HasDatabaseName("IX_PersonPropertyRelation_PersonId");

            builder.HasIndex(ppr => ppr.PropertyUnitId)
                .HasDatabaseName("IX_PersonPropertyRelation_PropertyUnitId");

            builder.HasIndex(ppr => new { ppr.PersonId, ppr.PropertyUnitId })
                .HasDatabaseName("IX_PersonPropertyRelation_PersonId_PropertyUnitId");

            builder.HasIndex(ppr => ppr.IsActive)
                .HasDatabaseName("IX_PersonPropertyRelation_IsActive");

            builder.HasIndex(ppr => ppr.IsDeleted)
                .HasDatabaseName("IX_PersonPropertyRelation_IsDeleted");

            // ==================== RELATIONSHIPS ====================

            // Relationship to Person (configured in PersonConfiguration)
            // Relationship to PropertyUnit
            builder.HasOne(ppr => ppr.PropertyUnit)
                .WithMany()
                .HasForeignKey(ppr => ppr.PropertyUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship to Evidence
            builder.HasMany(ppr => ppr.Evidences)
                .WithOne(e => e.PersonPropertyRelation)
                .HasForeignKey(e => e.PersonPropertyRelationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}