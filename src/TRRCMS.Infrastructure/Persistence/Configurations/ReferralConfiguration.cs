using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Referral entity
/// Configures table structure, relationships, and constraints
/// </summary>
public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        // ==================== TABLE CONFIGURATION ====================

        builder.ToTable("Referrals"); // PLURAL - Critical for consistency!

        // ==================== PRIMARY KEY ====================

        builder.HasKey(r => r.Id);

        // ==================== PROPERTIES ====================

        builder.Property(r => r.ReferralNumber)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Referral number - Format: REF-YYYY-NNNN");

        builder.Property(r => r.ClaimId)
            .IsRequired()
            .HasComment("Foreign key to Claim being referred");

        builder.Property(r => r.FromRole)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Role referring the claim");

        builder.Property(r => r.FromUserId)
            .IsRequired()
            .HasComment("User who initiated the referral");

        builder.Property(r => r.ToRole)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Role receiving the claim");

        builder.Property(r => r.ToUserId)
            .HasComment("Specific user assigned (optional)");

        // ==================== RELATIONSHIPS ====================

        builder.HasOne(r => r.Claim)
            .WithMany(c => c.Referrals)
            .HasForeignKey(r => r.ClaimId)
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== INDEXES ====================

        builder.HasIndex(r => r.ClaimId)
            .HasDatabaseName("IX_Referrals_ClaimId");

        builder.HasIndex(r => r.ReferralNumber)
            .IsUnique()
            .HasDatabaseName("IX_Referrals_ReferralNumber");

        builder.HasIndex(r => r.IsDeleted)
            .HasDatabaseName("IX_Referrals_IsDeleted");
    }
}