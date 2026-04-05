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
    /// Enum types to seed as vocabularies, with their metadata.
    /// </summary>
    private static readonly VocabularyEnumDefinition[] EnumDefinitions = new[]
    {
        // Demographics
        Def<Gender>("gender", "الجنس", "Gender", "Demographics"),
        Def<Nationality>("nationality", "الجنسية", "Nationality", "Demographics"),
        Def<RelationshipToHead>("relationship_to_head", "العلاقة برب الأسرة", "Relationship to Head", "Demographics"),

        // Property
        Def<BuildingType>("building_type", "نوع البناء", "Building Type", "Property"),
        Def<BuildingStatus>("building_status", "حالة البناء", "Building Status", "Property"),
Def<OccupancyType>("occupancy_type", "نوع الإشغال", "Occupancy Type", "Property"),
        Def<OccupancyNature>("occupancy_nature", "طبيعة الإشغال", "Occupancy Nature", "Property"),
        Def<TenureContractType>("tenure_contract_type", "نوع عقد الإشغال", "Tenure Contract Type", "Property"),

        // Relations
        Def<RelationType>("relation_type", "نوع العلاقة", "Relation Type", "Relations"),

        // Legal
        Def<EvidenceType>("evidence_type", "نوع الدليل", "Evidence Type", "Legal"),
        Def<DocumentType>("document_type", "نوع الوثيقة", "Document Type", "Legal"),

        // Claims
        Def<ClaimType>("claim_type", "نوع المطالبة", "Claim Type", "Claims"),
        Def<CaseStatus>("case_status", "حالة الحالة", "Case Status", "Claims"),
        Def<ClaimSource>("claim_source", "مصدر المطالبة", "Claim Source", "Claims"),
        // Survey
        Def<SurveyType>("survey_type", "نوع الاستطلاع", "Survey Type", "Survey"),
        Def<SurveyStatus>("survey_status", "حالة الاستطلاع", "Survey Status", "Survey"),
        Def<SurveySource>("survey_source", "مصدر الاستطلاع", "Survey Source", "Survey"),

        // Operations
        Def<TransferStatus>("transfer_status", "حالة النقل", "Transfer Status", "Operations"),
        // Property Units
        Def<PropertyUnitType>("property_unit_type", "نوع الوحدة العقارية", "Property Unit Type", "Property"),
        Def<PropertyUnitStatus>("property_unit_status", "حالة الوحدة العقارية", "Property Unit Status", "Property"),

        // System
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
    ///   Admin-added or admin-modified values are never removed or overwritten.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
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
                    isSystemVocabulary: true,
                    allowCustomValues: false,
                    category: def.Category,
                    createdByUserId: SystemUserId);

                await context.Vocabularies.AddAsync(vocabulary, cancellationToken);
            }
            else
            {
                // Smart merge: add new values, deprecate removed values, update changed labels
                var enumValues = GetEnumValues(def.EnumType);
                var enumByCode = enumValues.ToDictionary(v => v.Code);
                var existingValues = ParseValues(existing.ValuesJson);
                var existingCodes = existingValues.Select(v => v.Code).ToHashSet();

                var hasChanges = false;
                var changeDescriptions = new List<string>();

                // 1. Deprecate DB entries that are no longer in the C# enum
                foreach (var val in existingValues)
                {
                    if (!enumByCode.ContainsKey(val.Code) && !val.IsDeprecated)
                    {
                        val.IsDeprecated = true;
                        hasChanges = true;
                    }

                    // 2. Update labels for entries that exist in both but labels changed
                    if (enumByCode.TryGetValue(val.Code, out var enumVal))
                    {
                        if (val.LabelAr != enumVal.LabelAr || val.LabelEn != enumVal.LabelEn)
                        {
                            val.LabelAr = enumVal.LabelAr;
                            val.LabelEn = enumVal.LabelEn;
                            hasChanges = true;
                        }

                        // Un-deprecate if it was previously deprecated but is back in the enum
                        if (val.IsDeprecated)
                        {
                            val.IsDeprecated = false;
                            hasChanges = true;
                        }
                    }
                }

                // 3. Add new enum values not already in the vocabulary
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

                    hasChanges = true;
                    changeDescriptions.Add($"added {newValues.Count} value(s)");
                }

                if (hasChanges)
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

                    var deprecatedCount = existingValues.Count(v => v.IsDeprecated);
                    var description = $"System: synced with enum — {existingValues.Count} total, {deprecatedCount} deprecated";

                    var newVersion = existing.CreateMinorVersion(
                        mergedJson,
                        description,
                        SystemUserId);

                    await context.Vocabularies.AddAsync(newVersion, cancellationToken);
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
    /// </summary>
    private static List<SeedValue> ParseValues(string valuesJson)
    {
        if (string.IsNullOrWhiteSpace(valuesJson) || valuesJson == "[]")
            return new List<SeedValue>();

        try
        {
            return JsonSerializer.Deserialize<List<SeedValue>>(valuesJson, JsonOptions) ?? new List<SeedValue>();
        }
        catch
        {
            return new List<SeedValue>();
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
