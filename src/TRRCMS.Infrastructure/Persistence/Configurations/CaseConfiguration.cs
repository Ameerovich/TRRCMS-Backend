using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

public class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        builder.ToTable("Cases");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CaseNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(c => c.CaseNumber)
            .IsUnique();

        builder.Property(c => c.PropertyUnitId)
            .IsRequired();

        builder.HasIndex(c => c.PropertyUnitId)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasDefaultValue(CaseLifecycleStatus.Open);

        builder.Property(c => c.OpenedDate)
            .IsRequired();

        builder.Property(c => c.IsEditable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(c => c.Status);

        // Relationships
        builder.HasOne(c => c.PropertyUnit)
            .WithOne()
            .HasForeignKey<Case>(c => c.PropertyUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ClosedByClaim)
            .WithMany()
            .HasForeignKey(c => c.ClosedByClaimId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Surveys)
            .WithOne(s => s.Case)
            .HasForeignKey(s => s.CaseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Claims)
            .WithOne(cl => cl.Case)
            .HasForeignKey(cl => cl.CaseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.PersonPropertyRelations)
            .WithOne(r => r.Case)
            .HasForeignKey(r => r.CaseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
