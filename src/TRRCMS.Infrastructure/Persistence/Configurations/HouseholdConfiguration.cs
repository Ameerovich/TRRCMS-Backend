using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Household entity
/// </summary>
public class HouseholdConfiguration : IEntityTypeConfiguration<Household>
{
    public void Configure(EntityTypeBuilder<Household> builder)
    {
        // Table name
        builder.ToTable("Households");

        // Primary key
        builder.HasKey(h => h.Id);

        // ==================== IDENTIFIERS ====================

        builder.Property(h => h.PropertyUnitId)
            .IsRequired()
            .HasComment("Foreign key to PropertyUnit");

        builder.Property(h => h.HeadOfHouseholdPersonId)
            .IsRequired(false)
            .HasComment("Foreign key to Person (head of household)");

        // ==================== BASIC INFORMATION ====================

        builder.Property(h => h.HeadOfHouseholdName)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Name of head of household");

        builder.Property(h => h.HouseholdSize)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Total household size");

        // ==================== GENDER COMPOSITION ====================

        builder.Property(h => h.MaleCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of male members");

        builder.Property(h => h.FemaleCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of female members");

        // ==================== AGE COMPOSITION ====================

        builder.Property(h => h.InfantCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of infants (under 2 years)");

        builder.Property(h => h.ChildCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of children (2-12 years)");

        builder.Property(h => h.MinorCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of minors/adolescents (13-17 years)");

        builder.Property(h => h.AdultCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of adults (18-64 years)");

        builder.Property(h => h.ElderlyCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of elderly (65+ years)");

        // ==================== VULNERABILITY INDICATORS ====================

        builder.Property(h => h.PersonsWithDisabilitiesCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of persons with disabilities");

        builder.Property(h => h.IsFemaleHeaded)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicates if household is female-headed");

        builder.Property(h => h.WidowCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of widows");

        builder.Property(h => h.OrphanCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of orphans");

        builder.Property(h => h.SingleParentCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of single parents");

        // ==================== ECONOMIC INDICATORS ====================

        builder.Property(h => h.EmployedPersonsCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of employed persons");

        builder.Property(h => h.UnemployedPersonsCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Number of unemployed persons");

        builder.Property(h => h.PrimaryIncomeSource)
            .IsRequired(false)
            .HasMaxLength(200)
            .HasComment("Primary income source");

        builder.Property(h => h.MonthlyIncomeEstimate)
            .IsRequired(false)
            .HasPrecision(18, 2)
            .HasComment("Estimated monthly income");

        // ==================== DISPLACEMENT & ORIGIN ====================

        builder.Property(h => h.IsDisplaced)
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicates if household is displaced");

        builder.Property(h => h.OriginLocation)
            .IsRequired(false)
            .HasMaxLength(200)
            .HasComment("Origin location if displaced");

        builder.Property(h => h.ArrivalDate)
            .IsRequired(false)
            .HasComment("Date of arrival at current location");

        builder.Property(h => h.DisplacementReason)
            .IsRequired(false)
            .HasMaxLength(500)
            .HasComment("Reason for displacement");

        // ==================== ADDITIONAL INFORMATION ====================

        builder.Property(h => h.Notes)
            .IsRequired(false)
            .HasMaxLength(2000)
            .HasComment("Household notes");

        builder.Property(h => h.SpecialNeeds)
            .IsRequired(false)
            .HasMaxLength(1000)
            .HasComment("Special needs or circumstances");

        // ==================== AUDIT FIELDS ====================

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

        // ==================== INDEXES ====================

        // Index for property unit lookups
        builder.HasIndex(h => h.PropertyUnitId)
            .HasDatabaseName("IX_Household_PropertyUnitId");

        // Index for head of household person
        builder.HasIndex(h => h.HeadOfHouseholdPersonId)
            .HasDatabaseName("IX_Household_HeadOfHouseholdPersonId");

        // Index for soft delete queries
        builder.HasIndex(h => h.IsDeleted)
            .HasDatabaseName("IX_Household_IsDeleted");

        // Composite index for displacement queries
        builder.HasIndex(h => new { h.IsDisplaced, h.IsDeleted })
            .HasDatabaseName("IX_Household_IsDisplaced_IsDeleted");

        // ==================== RELATIONSHIPS ====================

        // Relationship to PropertyUnit (Many-to-One)
        builder.HasOne(h => h.PropertyUnit)
            .WithMany()  // PropertyUnit.Households collection - configure from PropertyUnit side
            .HasForeignKey(h => h.PropertyUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to HeadOfHouseholdPerson (optional one-to-one)
        builder.HasOne(h => h.HeadOfHouseholdPerson)
            .WithMany()  // Person doesn't have a back-navigation for this specific relationship
            .HasForeignKey(h => h.HeadOfHouseholdPersonId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Relationship to Members (One-to-Many)
        // Person.HouseholdId → Household.Members
        // This is configured from the Person side via Person.HouseholdId FK
        builder.HasMany(h => h.Members)
            .WithOne(p => p.Household)
            .HasForeignKey(p => p.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
