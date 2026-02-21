using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Community entity
/// </summary>
public class CommunityConfiguration : IEntityTypeConfiguration<Community>
{
    public void Configure(EntityTypeBuilder<Community> builder)
    {
        builder.ToTable("Communities");

        // ==================== PRIMARY KEY ====================
        builder.HasKey(c => c.Id);

        // ==================== CODES ====================

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(3)
            .HasComment("Community code (3 digits)");

        builder.Property(c => c.GovernorateCode)
            .IsRequired()
            .HasMaxLength(2)
            .HasComment("Parent governorate code");

        builder.Property(c => c.DistrictCode)
            .IsRequired()
            .HasMaxLength(2)
            .HasComment("Parent district code");

        builder.Property(c => c.SubDistrictCode)
            .IsRequired()
            .HasMaxLength(2)
            .HasComment("Parent sub-district code");

        // Unique composite index on full hierarchy + Code
        builder.HasIndex(c => new { c.GovernorateCode, c.DistrictCode, c.SubDistrictCode, c.Code })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_Communities_FullHierarchy_Code");

        // Composite index for hierarchical filtering
        builder.HasIndex(c => new { c.GovernorateCode, c.DistrictCode, c.SubDistrictCode })
            .HasDatabaseName("IX_Communities_GovernorateCode_DistrictCode_SubDistrictCode");

        // ==================== NAMES ====================

        builder.Property(c => c.NameArabic)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Arabic name");

        builder.Property(c => c.NameEnglish)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("English name");

        // ==================== STATUS ====================

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Whether this community is active");

        // ==================== RELATIONSHIPS ====================

        builder.HasOne(c => c.SubDistrict)
            .WithMany()
            .HasForeignKey(c => new { c.GovernorateCode, c.DistrictCode, c.SubDistrictCode })
            .HasPrincipalKey(s => new { s.GovernorateCode, s.DistrictCode, s.Code })
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== AUDIT FIELDS ====================

        builder.Property(c => c.CreatedAtUtc)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
