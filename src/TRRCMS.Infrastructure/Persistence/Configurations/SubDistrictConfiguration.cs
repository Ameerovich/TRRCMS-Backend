using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for SubDistrict entity
/// </summary>
public class SubDistrictConfiguration : IEntityTypeConfiguration<SubDistrict>
{
    public void Configure(EntityTypeBuilder<SubDistrict> builder)
    {
        builder.ToTable("SubDistricts");

        // ==================== PRIMARY KEY ====================
        builder.HasKey(s => s.Id);

        // ==================== CODES ====================

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(2)
            .HasComment("Sub-district code (2 digits)");

        builder.Property(s => s.GovernorateCode)
            .IsRequired()
            .HasMaxLength(2)
            .HasComment("Parent governorate code");

        builder.Property(s => s.DistrictCode)
            .IsRequired()
            .HasMaxLength(2)
            .HasComment("Parent district code");

        // Unique composite index on GovernorateCode + DistrictCode + Code
        builder.HasIndex(s => new { s.GovernorateCode, s.DistrictCode, s.Code })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_SubDistricts_GovernorateCode_DistrictCode_Code");

        // Composite index for hierarchical filtering
        builder.HasIndex(s => new { s.GovernorateCode, s.DistrictCode })
            .HasDatabaseName("IX_SubDistricts_GovernorateCode_DistrictCode");

        // ==================== NAMES ====================

        builder.Property(s => s.NameArabic)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Arabic name");

        builder.Property(s => s.NameEnglish)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("English name");

        // ==================== STATUS ====================

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Whether this sub-district is active");

        // ==================== RELATIONSHIPS ====================

        builder.HasOne(s => s.District)
            .WithMany()
            .HasForeignKey(s => new { s.GovernorateCode, s.DistrictCode })
            .HasPrincipalKey(d => new { d.GovernorateCode, d.Code })
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== AUDIT FIELDS ====================

        builder.Property(s => s.CreatedAtUtc)
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .IsRequired();

        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
