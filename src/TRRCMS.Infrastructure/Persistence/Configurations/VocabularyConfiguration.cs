using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Vocabulary entity.
/// Supports versioned controlled vocabularies with JSON value storage.
/// </summary>
public class VocabularyConfiguration : IEntityTypeConfiguration<Vocabulary>
{
    public void Configure(EntityTypeBuilder<Vocabulary> builder)
    {
        builder.ToTable("Vocabularies");

        // ==================== PRIMARY KEY ====================
        builder.HasKey(v => v.Id);

        // ==================== VOCABULARY IDENTIFICATION ====================

        builder.Property(v => v.VocabularyName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Vocabulary identifier (e.g., 'gender', 'relation_type')");

        builder.Property(v => v.DisplayNameArabic)
            .IsRequired()
            .HasMaxLength(200)
            .HasComment("Display name in Arabic");

        builder.Property(v => v.DisplayNameEnglish)
            .HasMaxLength(200)
            .HasComment("Display name in English");

        builder.Property(v => v.Description)
            .HasMaxLength(1000)
            .HasComment("Description of this vocabulary");

        // ==================== VERSIONING ====================

        builder.Property(v => v.Version)
            .IsRequired()
            .HasMaxLength(20)
            .HasComment("Semantic version: MAJOR.MINOR.PATCH");

        builder.Property(v => v.MajorVersion)
            .IsRequired()
            .HasComment("Major version number");

        builder.Property(v => v.MinorVersion)
            .IsRequired()
            .HasComment("Minor version number");

        builder.Property(v => v.PatchVersion)
            .IsRequired()
            .HasComment("Patch version number");

        builder.Property(v => v.VersionDate)
            .IsRequired()
            .HasComment("Date when this version was created");

        builder.Property(v => v.IsCurrentVersion)
            .IsRequired()
            .HasComment("Whether this is the active version");

        builder.Property(v => v.PreviousVersionId)
            .HasComment("Reference to previous version");

        // ==================== VALUES (JSON) ====================

        builder.Property(v => v.ValuesJson)
            .IsRequired()
            .HasColumnType("jsonb")
            .HasComment("Vocabulary values as JSON array");

        builder.Property(v => v.ValueCount)
            .IsRequired()
            .HasComment("Number of values in this vocabulary");

        // ==================== METADATA ====================

        builder.Property(v => v.Category)
            .HasMaxLength(100)
            .HasComment("Category grouping (e.g., Demographics, Property, Legal)");

        builder.Property(v => v.IsSystemVocabulary)
            .IsRequired()
            .HasComment("System-defined vocabulary (cannot be deleted)");

        builder.Property(v => v.AllowCustomValues)
            .IsRequired()
            .HasComment("Whether custom values can be added");

        builder.Property(v => v.IsMandatory)
            .IsRequired()
            .HasComment("Whether this vocabulary is mandatory");

        builder.Property(v => v.IsActive)
            .IsRequired()
            .HasComment("Whether this vocabulary is active");

        // ==================== IMPORT COMPATIBILITY ====================

        builder.Property(v => v.MinimumCompatibleVersion)
            .HasMaxLength(20)
            .HasComment("Minimum compatible version for imports");

        builder.Property(v => v.ChangeLog)
            .HasMaxLength(2000)
            .HasComment("Changelog for this version");

        // ==================== USAGE TRACKING ====================

        builder.Property(v => v.LastUsedDate)
            .HasComment("Date when vocabulary was last used");

        builder.Property(v => v.UsageCount)
            .IsRequired()
            .HasComment("How many times this vocabulary has been used");

        // ==================== INDEXES ====================

        // Filtered unique index: only one current version per vocabulary name
        builder.HasIndex(v => v.VocabularyName)
            .HasFilter("\"IsCurrentVersion\" = true AND \"IsDeleted\" = false")
            .IsUnique()
            .HasDatabaseName("IX_Vocabularies_VocabularyName_Current");

        // Index on Category for filtered queries
        builder.HasIndex(v => v.Category)
            .HasDatabaseName("IX_Vocabularies_Category");

        // Composite index for version lookups
        builder.HasIndex(v => new { v.VocabularyName, v.IsCurrentVersion })
            .HasDatabaseName("IX_Vocabularies_Name_IsCurrent");

        // ==================== RELATIONSHIPS ====================

        // Self-referencing: previous version
        builder.HasOne(v => v.PreviousVersion)
            .WithMany()
            .HasForeignKey(v => v.PreviousVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        // ==================== AUDIT FIELDS (from BaseAuditableEntity) ====================

        builder.Property(v => v.CreatedAtUtc)
            .IsRequired();

        builder.Property(v => v.CreatedBy)
            .IsRequired();

        builder.Property(v => v.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
