using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Governorate entity
/// </summary>
public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
{
    public void Configure(EntityTypeBuilder<Governorate> builder)
    {
        builder.ToTable("Governorates");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Code)
            .IsRequired()
            .HasMaxLength(2)
            .HasComment("Governorate code (2 digits)");

        // Unique index on Code (one governorate per code)
        builder.HasIndex(g => g.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_Governorates_Code");

        builder.Property(g => g.NameArabic)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Arabic name");

        builder.Property(g => g.NameEnglish)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("English name");

        builder.Property(g => g.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Whether this governorate is active");

        builder.Property(g => g.CreatedAtUtc)
            .IsRequired();

        builder.Property(g => g.CreatedBy)
            .IsRequired();

        builder.Property(g => g.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
