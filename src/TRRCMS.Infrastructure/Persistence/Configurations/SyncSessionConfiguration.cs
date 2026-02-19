using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

public class SyncSessionConfiguration : IEntityTypeConfiguration<SyncSession>
{
    public void Configure(EntityTypeBuilder<SyncSession> builder)
    {
        builder.ToTable("SyncSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FieldCollectorId)
            .IsRequired();

        builder.Property(x => x.DeviceId)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.ServerIpAddress)
            .HasMaxLength(64);

        builder.Property(x => x.SessionStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.StartedAtUtc)
            .IsRequired();

        builder.Property(x => x.CompletedAtUtc);

        builder.Property(x => x.PackagesUploaded)
            .IsRequired();

        builder.Property(x => x.PackagesFailed)
            .IsRequired();

        builder.Property(x => x.AssignmentsDownloaded)
            .IsRequired();

        builder.Property(x => x.AssignmentsAcknowledged)
            .IsRequired();

        builder.Property(x => x.VocabularyVersionsSent)
            .HasColumnType("jsonb"); // good for querying later

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        // ==================== INDEXES ====================
        builder.HasIndex(x => x.DeviceId);
        builder.HasIndex(x => x.FieldCollectorId);
        builder.HasIndex(x => x.SessionStatus);
        builder.HasIndex(x => x.StartedAtUtc);

        // common query patterns
        builder.HasIndex(x => new { x.FieldCollectorId, x.StartedAtUtc });
        builder.HasIndex(x => new { x.DeviceId, x.StartedAtUtc });
    }
}
