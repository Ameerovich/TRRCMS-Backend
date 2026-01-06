using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Configurations
{
    public class HouseholdConfiguration : IEntityTypeConfiguration<Household>
    {
        public void Configure(EntityTypeBuilder<Household> builder)
        {
            // Table name
            builder.ToTable("Households");

            // Primary Key
            builder.HasKey(h => h.Id);

            // ==================== PROPERTIES ====================

            builder.Property(h => h.HeadOfHouseholdName)
                .IsRequired()
                .HasMaxLength(200);

            // ==================== INDEXES ====================

            builder.HasIndex(h => h.PropertyUnitId)
                .HasDatabaseName("IX_Household_PropertyUnitId");

            builder.HasIndex(h => h.HeadOfHouseholdPersonId)
                .HasDatabaseName("IX_Household_HeadOfHouseholdPersonId");

            builder.HasIndex(h => h.IsDeleted)
                .HasDatabaseName("IX_Household_IsDeleted");

            // ==================== RELATIONSHIPS ====================

            // Relationship 1: Household -> PropertyUnit
            builder.HasOne(h => h.PropertyUnit)
                .WithMany()
                .HasForeignKey(h => h.PropertyUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship 2: Household.Members -> Person.Household (one-to-many)
            builder.HasMany(h => h.Members)
                .WithOne(p => p.Household)
                .HasForeignKey(p => p.HouseholdId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship 3: Household.HeadOfHouseholdPerson (one-to-one)
            // This is a separate relationship from Members
            builder.HasOne(h => h.HeadOfHouseholdPerson)
                .WithMany() // Person doesn't have a back-navigation for this
                .HasForeignKey(h => h.HeadOfHouseholdPersonId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); // Optional - household might not have registered head
        }
    }
}