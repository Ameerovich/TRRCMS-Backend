using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

public class LandmarkTypeIconConfiguration : IEntityTypeConfiguration<LandmarkTypeIcon>
{
    public void Configure(EntityTypeBuilder<LandmarkTypeIcon> builder)
    {
        builder.ToTable("LandmarkTypeIcons");

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.Property(e => e.Type)
            .IsRequired();

        builder.HasIndex(e => e.Type)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_LandmarkTypeIcons_Type");

        builder.Property(e => e.SvgContent)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(e => e.DisplayNameArabic)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.DisplayNameEnglish)
            .IsRequired()
            .HasMaxLength(200);
    }
}
