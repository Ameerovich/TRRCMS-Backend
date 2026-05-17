using System.Reflection;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Audit;

/// <summary>
/// Reads the <see cref="ArabicLabelAttribute"/> baked into <see cref="AuditActionType"/>
/// so the audit DTO can ship an Arabic verb label alongside the English enum name.
/// Result is cached per enum value (the lookup is reflection-based but only once).
/// </summary>
internal static class AuditActionLabel
{
    private static readonly Dictionary<AuditActionType, string> Cache = BuildCache();

    public static string Arabic(AuditActionType value) =>
        Cache.TryGetValue(value, out var label) ? label : value.ToString();

    private static Dictionary<AuditActionType, string> BuildCache()
    {
        var map = new Dictionary<AuditActionType, string>();
        foreach (var field in typeof(AuditActionType).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<ArabicLabelAttribute>();
            if (attr is null) continue;
            var value = (AuditActionType)field.GetValue(null)!;
            map[value] = attr.Label;
        }
        return map;
    }
}
