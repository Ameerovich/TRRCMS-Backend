using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Configurations
{
    public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
    {
        public void Configure(EntityTypeBuilder<Claim> builder)
        {
            // Table name
            builder.ToTable("Claims");

            // Primary Key
            builder.HasKey(c => c.Id);

            // TEMPORARY: Ignore Certificate navigation to fix migration
            // We'll properly configure this when we implement Certificate entity
            builder.Ignore(c => c.Certificate);
            builder.Ignore(c => c.CertificateId);
        }
    }
}