using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

public class IdentificationDocumentConfiguration : IEntityTypeConfiguration<IdentificationDocument>
{
    public void Configure(EntityTypeBuilder<IdentificationDocument> builder)
    {
        builder.ToTable("IdentificationDocuments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.PersonId).IsRequired();
        builder.Property(d => d.DocumentType);
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.OriginalFileName).IsRequired().HasMaxLength(255);
        builder.Property(d => d.FilePath).IsRequired().HasMaxLength(500);
        builder.Property(d => d.FileSizeBytes).IsRequired();
        builder.Property(d => d.MimeType).IsRequired().HasMaxLength(100);
        builder.Property(d => d.FileHash).HasMaxLength(64);
        builder.Property(d => d.IssuingAuthority).HasMaxLength(200);
        builder.Property(d => d.DocumentReferenceNumber).HasMaxLength(100);
        builder.Property(d => d.Notes).HasMaxLength(2000);

        builder.HasIndex(d => d.PersonId);
        builder.HasIndex(d => d.DocumentType);
        builder.HasIndex(d => d.FileHash);

        builder.HasOne(d => d.Person)
            .WithMany(p => p.IdentificationDocuments)
            .HasForeignKey(d => d.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
