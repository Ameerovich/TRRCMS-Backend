using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Household entity (canonical v1.9 shape).
/// </summary>
public class HouseholdConfiguration : IEntityTypeConfiguration<Household>
{
    public void Configure(EntityTypeBuilder<Household> builder)
    {
        builder.ToTable("Households");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.PropertyUnitId)
            .IsRequired()
            .HasComment("Foreign key to PropertyUnit");

        builder.Property(h => h.HouseholdSize)
            .IsRequired()
            .HasComment("عدد الأفراد - Total household size");

        builder.Property(h => h.MaleCount)
            .IsRequired(false)
            .HasComment("عدد الذكور - Total males (all ages)");

        builder.Property(h => h.FemaleCount)
            .IsRequired(false)
            .HasComment("عدد الإناث - Total females (all ages)");

        builder.Property(h => h.AdultCount)
            .IsRequired(false)
            .HasComment("عدد البالغين - Number of adults");

        builder.Property(h => h.ChildCount)
            .IsRequired(false)
            .HasComment("عدد الأطفال - Number of children");

        builder.Property(h => h.ElderlyCount)
            .IsRequired(false)
            .HasComment("عدد كبار السن - Number of elderly");

        builder.Property(h => h.DisabledCount)
            .IsRequired(false)
            .HasComment("عدد ذوي الإعاقة - Number of persons with disabilities");

        builder.Property(h => h.OccupancyNature)
            .IsRequired(false)
            .HasComment("طبيعة الإشغال - Occupancy nature enum stored as integer");

        builder.Property(h => h.OccupancyStartDate)
            .IsRequired(false)
            .HasComment("تاريخ بداية الإشغال - Date the household started occupying this unit (UTC)");

        builder.Property(h => h.Notes)
            .IsRequired(false)
            .HasMaxLength(2000)
            .HasComment("ملاحظات - Household notes");

        builder.Property(h => h.CreatedAtUtc)
            .IsRequired()
            .HasComment("Creation timestamp (UTC)");

        builder.Property(h => h.CreatedBy)
            .IsRequired()
            .HasComment("User who created this record");

        builder.Property(h => h.LastModifiedAtUtc)
            .IsRequired(false)
            .HasComment("Last modification timestamp (UTC)");

        builder.Property(h => h.LastModifiedBy)
            .IsRequired(false)
            .HasComment("User who last modified this record");

        builder.Property(h => h.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Soft delete flag");

        builder.Property(h => h.DeletedAtUtc)
            .IsRequired(false)
            .HasComment("Deletion timestamp (UTC)");

        builder.Property(h => h.DeletedBy)
            .IsRequired(false)
            .HasComment("User who deleted this record");

        builder.Property(h => h.RowVersion)
            .IsRowVersion()
            .HasComment("Concurrency token");

        builder.HasIndex(h => h.PropertyUnitId)
            .HasDatabaseName("IX_Household_PropertyUnitId");

        builder.HasIndex(h => h.IsDeleted)
            .HasDatabaseName("IX_Household_IsDeleted");

        builder.HasOne(h => h.PropertyUnit)
            .WithMany()
            .HasForeignKey(h => h.PropertyUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(h => h.Members)
            .WithOne(p => p.Household)
            .HasForeignKey(p => p.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
