using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for District entity
/// </summary>
public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts");

        // ==================== PRIMARY KEY ====================
        builder.HasKey(d => d.Id);

        // ==================== CODES ====================

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(2)
            .HasComment("District code (2 digits)");

        builder.Property(d => d.GovernorateCode)
            .IsRequired()
            .HasMaxLength(2)
            .HasComment("Parent governorate code");

        // Unique composite index on GovernorateCode + Code
        builder.HasIndex(d => new { d.GovernorateCode, d.Code })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_Districts_GovernorateCode_Code");

        // Index for filtering by governorate
        builder.HasIndex(d => d.GovernorateCode)
            .HasDatabaseName("IX_Districts_GovernorateCode");

        // ==================== NAMES ====================

        builder.Property(d => d.NameArabic)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Arabic name");

        builder.Property(d => d.NameEnglish)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("English name");

        // ==================== STATUS ====================

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Whether this district is active");

        // ==================== RELATIONSHIPS ====================

        builder.HasOne(d => d.Governorate)
            .WithMany()
            .HasForeignKey(d => d.GovernorateCode)
            .HasPrincipalKey(g => g.Code)
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== AUDIT FIELDS ====================

        builder.Property(d => d.CreatedAtUtc)
            .IsRequired();

        builder.Property(d => d.CreatedBy)
            .IsRequired();

        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
