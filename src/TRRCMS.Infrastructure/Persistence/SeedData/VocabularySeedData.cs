using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.SeedData;

/// <summary>
/// Seeds vocabulary data from C# enums annotated with [ArabicLabel].
/// Reads enum values via reflection, builds JSON, and upserts into the Vocabularies table.
/// </summary>
public static class VocabularySeedData
{
    /// <summary>
    /// System user ID used for seed data creation.
    /// </summary>
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// Seed version — bump this when enum values change to trigger re-seed.
    /// </summary>
    private const string SeedVersion = "1.1.0";

    /// <summary>
    /// Enum types to seed as vocabularies, with their metadata.
    /// </summary>
    private static readonly VocabularyEnumDefinition[] EnumDefinitions = new[]
    {
        // Demographics
        Def<Gender>("gender", "الجنس", "Gender", "Demographics"),
        Def<Nationality>("nationality", "الجنسية", "Nationality", "Demographics"),
        Def<AgeCategory>("age_category", "الفئة العمرية", "Age Category", "Demographics"),
        Def<RelationshipToHead>("relationship_to_head", "العلاقة برب الأسرة", "Relationship to Head", "Demographics"),

        // Property
        Def<BuildingType>("building_type", "نوع البناء", "Building Type", "Property"),
        Def<BuildingStatus>("building_status", "حالة البناء", "Building Status", "Property"),
        Def<DamageLevel>("damage_level", "مستوى الضرر", "Damage Level", "Property"),
        Def<OccupancyType>("occupancy_type", "نوع الإشغال", "Occupancy Type", "Property"),
        Def<OccupancyNature>("occupancy_nature", "طبيعة الإشغال", "Occupancy Nature", "Property"),
        Def<TenureContractType>("tenure_contract_type", "نوع عقد الإشغال", "Tenure Contract Type", "Property"),

        // Relations
        Def<RelationType>("relation_type", "نوع العلاقة", "Relation Type", "Relations"),

        // Legal
        Def<EvidenceType>("evidence_type", "نوع الدليل", "Evidence Type", "Legal"),
        Def<DocumentType>("document_type", "نوع المستند", "Document Type", "Legal"),
        Def<VerificationStatus>("verification_status", "حالة التحقق", "Verification Status", "Legal"),

        // Claims
        Def<ClaimStatus>("claim_status", "حالة المطالبة", "Claim Status", "Claims"),
        Def<ClaimSource>("claim_source", "مصدر المطالبة", "Claim Source", "Claims"),
        Def<CasePriority>("case_priority", "أولوية الحالة", "Case Priority", "Claims"),
        Def<LifecycleStage>("lifecycle_stage", "مرحلة دورة الحياة", "Lifecycle Stage", "Claims"),
        Def<CertificateStatus>("certificate_status", "حالة الشهادة", "Certificate Status", "Claims"),

        // Survey
        Def<SurveyType>("survey_type", "نوع الاستطلاع", "Survey Type", "Survey"),
        Def<SurveyStatus>("survey_status", "حالة الاستطلاع", "Survey Status", "Survey"),
        Def<SurveySource>("survey_source", "مصدر الاستطلاع", "Survey Source", "Survey"),

        // Operations
        Def<TransferStatus>("transfer_status", "حالة النقل", "Transfer Status", "Operations"),
        Def<ReferralRole>("referral_role", "دور الإحالة", "Referral Role", "Operations"),

        // Property Units
        Def<PropertyUnitType>("property_unit_type", "نوع الوحدة العقارية", "Property Unit Type", "Property"),
        Def<PropertyUnitStatus>("property_unit_status", "حالة الوحدة العقارية", "Property Unit Status", "Property"),

        // System
        Def<UserRole>("user_role", "دور المستخدم", "User Role", "System"),
        Def<ImportStatus>("import_status", "حالة الاستيراد", "Import Status", "System"),
        Def<Permission>("permission", "الصلاحية", "Permission", "System"),
        Def<AuditActionType>("audit_action_type", "نوع إجراء التدقيق", "Audit Action Type", "System"),
    };

    /// <summary>
    /// Seed all vocabularies from enum definitions.
    /// Skips existing vocabularies that already match the current seed version.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
    {
        foreach (var def in EnumDefinitions)
        {
            var existing = await context.Vocabularies
                .Where(v => !v.IsDeleted && v.VocabularyName == def.VocabularyName && v.IsCurrentVersion)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing != null && existing.Version == SeedVersion)
                continue; // Already seeded with this version

            var valuesJson = BuildValuesJson(def.EnumType);
            var valueCount = CountEnumValues(def.EnumType);

            if (existing != null)
            {
                // Create a minor version bump
                var newVersion = existing.CreateMinorVersion(valuesJson, "Seed data updated", SystemUserId);
                await context.Vocabularies.AddAsync(newVersion, cancellationToken);
            }
            else
            {
                // Create new vocabulary
                var vocabulary = Vocabulary.Create(
                    vocabularyName: def.VocabularyName,
                    displayNameArabic: def.DisplayNameArabic,
                    displayNameEnglish: def.DisplayNameEnglish,
                    description: $"System vocabulary for {def.DisplayNameEnglish}",
                    valuesJson: valuesJson,
                    isSystemVocabulary: true,
                    allowCustomValues: false,
                    category: def.Category,
                    createdByUserId: SystemUserId);

                await context.Vocabularies.AddAsync(vocabulary, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Build JSON array from enum values with [ArabicLabel] attributes.
    /// Format: [{"code": 1, "labelAr": "ذكر", "labelEn": "Male", "displayOrder": 0}, ...]
    /// </summary>
    private static string BuildValuesJson(Type enumType)
    {
        var values = new List<object>();
        var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        var order = 0;

        foreach (var field in fields)
        {
            var enumValue = (int)field.GetValue(null)!;
            var arabicAttr = field.GetCustomAttribute<ArabicLabelAttribute>();
            var labelAr = arabicAttr?.Label ?? field.Name;
            var labelEn = FormatEnumName(field.Name);

            values.Add(new
            {
                code = enumValue,
                labelAr = labelAr,
                labelEn = labelEn,
                displayOrder = order++
            });
        }

        return JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = false });
    }

    /// <summary>
    /// Convert PascalCase enum name to readable English.
    /// E.g., "MinorDamage" → "Minor Damage", "OwnerOccupied" → "Owner Occupied"
    /// </summary>
    private static string FormatEnumName(string name)
    {
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
                result.Append(' ');
            else if (i > 0 && char.IsUpper(name[i]) && char.IsUpper(name[i - 1]) && i + 1 < name.Length && !char.IsUpper(name[i + 1]))
                result.Append(' ');

            result.Append(name[i]);
        }
        return result.ToString();
    }

    private static int CountEnumValues(Type enumType)
    {
        return Enum.GetValues(enumType).Length;
    }

    private static VocabularyEnumDefinition Def<TEnum>(string name, string ar, string en, string category) where TEnum : Enum
    {
        return new VocabularyEnumDefinition(typeof(TEnum), name, ar, en, category);
    }

    private record VocabularyEnumDefinition(
        Type EnumType,
        string VocabularyName,
        string DisplayNameArabic,
        string DisplayNameEnglish,
        string Category);
}
