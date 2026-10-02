using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    /// Enum types to seed as vocabularies, with their metadata.
    /// </summary>
    private static readonly VocabularyEnumDefinition[] EnumDefinitions = new[]
    {
        // ── Extensible (isSystemVocabulary=false) ─────────────────────────────────
        // Admins can add new codes via a major version through the API.
        // Values are stored as plain integers and never branched on in backend logic —
        // safe to extend without any code changes.

        // Demographics
        Ext<Nationality>("nationality", "الجنسية", "Nationality", "Demographics"),
        Ext<RelationshipToHead>("relationship_to_head", "العلاقة برب الأسرة", "Relationship to Head", "Demographics"),

        // Property
        Ext<BuildingStatus>("building_status", "حالة البناء", "Building Status", "Property"),
        Ext<OccupancyNature>("occupancy_nature", "طبيعة الإشغال", "Occupancy Nature", "Property"),
        Ext<TenureContractType>("tenure_contract_type", "نوع عقد الإشغال", "Tenure Contract Type", "Property"),
        Ext<PropertyUnitType>("property_unit_type", "نوع الوحدة العقارية", "Property Unit Type", "Property"),
        Ext<PropertyUnitStatus>("property_unit_status", "حالة الوحدة العقارية", "Property Unit Status", "Property"),

        // Legal
        Ext<DocumentType>("document_type", "نوع الوثيقة", "Document Type", "Legal"),

        // Survey
        Ext<SurveySource>("survey_source", "مصدر الاستطلاع", "Survey Source", "Survey"),

        // ── System-locked (isSystemVocabulary=true) ───────────────────────────────
        // Major versions (new codes) are blocked via API.
        // Specific enum values are referenced in backend logic — adding unknown codes
        // would cause undefined behaviour in state machines or scoring algorithms.

        Def<Gender>("gender", "الجنس", "Gender", "Demographics"),

        Def<BuildingType>("building_type", "نوع البناء", "Building Type", "Property"),
        Def<OccupancyType>("occupancy_type", "نوع الإشغال", "Occupancy Type", "Property"),

        Def<RelationType>("relation_type", "نوع العلاقة", "Relation Type", "Relations"),

        Def<EvidenceType>("evidence_type", "نوع الدليل", "Evidence Type", "Legal"),

        Def<ClaimType>("claim_type", "نوع المطالبة", "Claim Type", "Claims"),
        Def<CaseStatus>("case_status", "حالة الحالة", "Case Status", "Claims"),
        Def<ClaimSource>("claim_source", "مصدر المطالبة", "Claim Source", "Claims"),

        Def<SurveyType>("survey_type", "نوع الاستطلاع", "Survey Type", "Survey"),
        Def<SurveyStatus>("survey_status", "حالة الاستطلاع", "Survey Status", "Survey"),

        Def<TransferStatus>("transfer_status", "حالة النقل", "Transfer Status", "Operations"),

        Def<UserRole>("user_role", "دور المستخدم", "User Role", "System"),
        Def<ImportStatus>("import_status", "حالة الاستيراد", "Import Status", "System"),
        Def<Permission>("permission", "الصلاحية", "Permission", "System"),
        Def<AuditActionType>("audit_action_type", "نوع إجراء التدقيق", "Audit Action Type", "System"),
    };

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Seed all vocabularies from enum definitions using additive merge logic.
    /// - Vocabulary doesn't exist → create from enum values (first install).
    /// - Vocabulary exists → only append new enum values not already present.
    ///   Existing values (labels, deprecation flags, admin-added codes) are never
    ///   overwritten or reverted — they may hold admin changes made via the versioning API.
    /// - System-locked vocabularies only → codes removed from the C# enum are deprecated.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        foreach (var def in EnumDefinitions)
        {
            var existing = await context.Vocabularies
                .Where(v => !v.IsDeleted && v.VocabularyName == def.VocabularyName && v.IsCurrentVersion)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                // First install — create vocabulary from enum
                var valuesJson = BuildValuesJson(def.EnumType);

                var vocabulary = Vocabulary.Create(
                    vocabularyName: def.VocabularyName,
                    displayNameArabic: def.DisplayNameArabic,
                    displayNameEnglish: def.DisplayNameEnglish,
                    description: $"System vocabulary for {def.DisplayNameEnglish}",
                    valuesJson: valuesJson,
                    isSystemVocabulary: !def.IsExtensible,
                    allowCustomValues: false,
                    category: def.Category,
                    createdByUserId: SystemUserId);

                await context.Vocabularies.AddAsync(vocabulary, cancellationToken);
            }
            else
            {
                // Align the system flag with the code-defined policy. Databases seeded before the
                // extensible/system-locked split still carry isSystemVocabulary=true on extensible
                // vocabularies, which blocks admins from adding codes via a major version.
                var isSystemVocabulary = !def.IsExtensible;
                if (existing.IsSystemVocabulary != isSystemVocabulary)
                    existing.SetSystemVocabulary(isSystemVocabulary, SystemUserId);

                // Additive merge: the current version may hold admin changes made through the
                // versioning API (label fixes, deprecations, added codes) — never revert them.
                var existingValues = ParseValues(existing.ValuesJson);
                if (existingValues == null)
                {
                    logger?.LogWarning(
                        "Vocabulary '{VocabularyName}' v{Version} has unreadable ValuesJson — skipped enum sync",
                        existing.VocabularyName, existing.Version);
                    continue;
                }

                var enumValues = GetEnumValues(def.EnumType);
                var enumCodes = enumValues.Select(v => v.Code).ToHashSet();
                var existingCodes = existingValues.Select(v => v.Code).ToHashSet();
                var changeDescriptions = new List<string>();

                // 1. System-locked only: deprecate codes removed from the C# enum.
                //    Extensible vocabularies hold admin-added codes that never exist in the enum.
                if (!def.IsExtensible)
                {
                    var removedValues = existingValues
                        .Where(v => !enumCodes.Contains(v.Code) && !v.IsDeprecated)
                        .ToList();

                    foreach (var val in removedValues)
                        val.IsDeprecated = true;

                    if (removedValues.Count > 0)
                        changeDescriptions.Add($"deprecated {removedValues.Count} value(s) removed from enum");
                }

                // 2. Add new enum values not already in the vocabulary
                var newValues = enumValues.Where(v => !existingCodes.Contains(v.Code)).ToList();
                if (newValues.Count > 0)
                {
                    var maxOrder = existingValues.Count > 0
                        ? existingValues.Max(v => v.DisplayOrder) + 1
                        : 0;

                    foreach (var val in newValues)
                    {
                        val.DisplayOrder = maxOrder++;
                        existingValues.Add(val);
                    }

                    changeDescriptions.Add($"added {newValues.Count} value(s)");
                }

                if (changeDescriptions.Count > 0)
                {
                    var mergedJson = JsonSerializer.Serialize(existingValues.Select(v => new
                    {
                        code = v.Code,
                        labelAr = v.LabelAr,
                        labelEn = v.LabelEn,
                        description = v.Description,
                        displayOrder = v.DisplayOrder,
                        isDeprecated = v.IsDeprecated
                    }));

                    var description = $"System: {string.Join(", ", changeDescriptions)} from code deployment";

                    var newVersion = existing.CreateMinorVersion(
                        mergedJson,
                        description,
                        SystemUserId);

                    await context.Vocabularies.AddAsync(newVersion, cancellationToken);

                    logger?.LogInformation(
                        "Vocabulary '{VocabularyName}' synced with enum: v{OldVersion} → v{NewVersion} ({Changes})",
                        existing.VocabularyName, existing.Version, newVersion.Version, string.Join(", ", changeDescriptions));
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Get enum values as SeedValue objects for comparison.
    /// </summary>
    private static List<SeedValue> GetEnumValues(Type enumType)
    {
        var values = new List<SeedValue>();
        var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        var order = 0;

        foreach (var field in fields)
        {
            var enumValue = (int)field.GetValue(null)!;
            var arabicAttr = field.GetCustomAttribute<ArabicLabelAttribute>();
            var labelAr = arabicAttr?.Label ?? field.Name;
            var labelEn = FormatEnumName(field.Name);

            values.Add(new SeedValue
            {
                Code = enumValue,
                LabelAr = labelAr,
                LabelEn = labelEn,
                DisplayOrder = order++
            });
        }

        return values;
    }

    /// <summary>
    /// Parse existing vocabulary values from JSON.
    /// Returns null when the JSON is unreadable, so the caller skips the vocabulary
    /// instead of rebuilding it from enum defaults and dropping admin-added codes.
    /// </summary>
    private static List<SeedValue>? ParseValues(string valuesJson)
    {
        if (string.IsNullOrWhiteSpace(valuesJson) || valuesJson == "[]")
            return new List<SeedValue>();

        try
        {
            return JsonSerializer.Deserialize<List<SeedValue>>(valuesJson, JsonOptions) ?? new List<SeedValue>();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private class SeedValue
    {
        public int Code { get; set; }
        public string LabelAr { get; set; } = "";
        public string LabelEn { get; set; } = "";
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDeprecated { get; set; }
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
                displayOrder = order++,
                isDeprecated = false
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

    private static VocabularyEnumDefinition Def<TEnum>(string name, string ar, string en, string category) where TEnum : Enum
        => new VocabularyEnumDefinition(typeof(TEnum), name, ar, en, category, IsExtensible: false);

    private static VocabularyEnumDefinition Ext<TEnum>(string name, string ar, string en, string category) where TEnum : Enum
        => new VocabularyEnumDefinition(typeof(TEnum), name, ar, en, category, IsExtensible: true);

    private record VocabularyEnumDefinition(
        Type EnumType,
        string VocabularyName,
        string DisplayNameArabic,
        string DisplayNameEnglish,
        string Category,
        bool IsExtensible);
}
