using System.Text.Json;
using TRRCMS.Application.Audit.Dtos;

namespace TRRCMS.Application.Audit;

/// <summary>
/// Parses an audit entry's <c>OldValues</c> / <c>NewValues</c> JSON snapshots into
/// a structured <see cref="AuditChangeDto"/> list. Frontend consumes this instead
/// of re-parsing the JSON strings client-side.
/// </summary>
internal static class AuditChangeBuilder
{
    public static List<AuditChangeDto> Build(string? oldValuesJson, string? newValuesJson, string? changedFieldsHint)
    {
        var oldDoc = TryParse(oldValuesJson);
        var newDoc = TryParse(newValuesJson);

        if (oldDoc is null && newDoc is null)
            return new List<AuditChangeDto>();

        var oldMap = ToObjectMap(oldDoc);
        var newMap = ToObjectMap(newDoc);

        // Field set: prefer explicit hint when present, else union of keys.
        IEnumerable<string> fields;
        var hint = (changedFieldsHint ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (hint.Length > 0)
        {
            fields = hint;
        }
        else
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var k in oldMap.Keys) keys.Add(k);
            foreach (var k in newMap.Keys) keys.Add(k);
            fields = keys;
        }

        var result = new List<AuditChangeDto>();
        foreach (var field in fields)
        {
            oldMap.TryGetValue(field, out var oldEl);
            newMap.TryGetValue(field, out var newEl);

            // If both sides are absent (hint pointed to a missing field), skip.
            if (oldEl.ValueKind == JsonValueKind.Undefined && newEl.ValueKind == JsonValueKind.Undefined)
                continue;

            // When no explicit hint was given, suppress unchanged fields.
            if (hint.Length == 0 && JsonEquals(oldEl, newEl))
                continue;

            result.Add(new AuditChangeDto
            {
                Field = field,
                OldValue = ToValue(oldEl),
                NewValue = ToValue(newEl),
                Type = InferType(newEl.ValueKind != JsonValueKind.Undefined ? newEl : oldEl)
            });
        }

        return result;
    }

    private static JsonDocument? TryParse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonDocument.Parse(json); }
        catch { return null; }
    }

    private static Dictionary<string, JsonElement> ToObjectMap(JsonDocument? doc)
    {
        var map = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        if (doc is null) return map;
        if (doc.RootElement.ValueKind != JsonValueKind.Object) return map;
        foreach (var prop in doc.RootElement.EnumerateObject())
            map[prop.Name] = prop.Value;
        return map;
    }

    private static object? ToValue(JsonElement el)
    {
        return el.ValueKind switch
        {
            JsonValueKind.Undefined => null,
            JsonValueKind.Null => null,
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.TryGetInt64(out var l) ? l : el.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Object => el.GetRawText(),
            JsonValueKind.Array => el.GetRawText(),
            _ => el.GetRawText()
        };
    }

    private static string InferType(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.String => LooksLikeDate(el.GetString()) ? "date" : "string",
        JsonValueKind.Number => "number",
        JsonValueKind.True or JsonValueKind.False => "boolean",
        JsonValueKind.Object => "object",
        JsonValueKind.Array => "array",
        JsonValueKind.Null => "null",
        _ => "string"
    };

    private static bool LooksLikeDate(string? s) =>
        !string.IsNullOrEmpty(s)
        && s.Length >= 10
        && DateTime.TryParse(s, out _);

    private static bool JsonEquals(JsonElement a, JsonElement b)
    {
        if (a.ValueKind != b.ValueKind) return false;
        return a.ValueKind switch
        {
            JsonValueKind.Undefined or JsonValueKind.Null => true,
            JsonValueKind.String => a.GetString() == b.GetString(),
            JsonValueKind.Number => a.GetRawText() == b.GetRawText(),
            JsonValueKind.True or JsonValueKind.False => true,
            _ => a.GetRawText() == b.GetRawText()
        };
    }
}
