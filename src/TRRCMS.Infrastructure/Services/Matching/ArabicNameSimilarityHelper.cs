using System.Globalization;
using System.Text;

namespace TRRCMS.Infrastructure.Services.Matching;

/// <summary>
/// Utility for computing similarity between Arabic names.
/// Handles diacritics (tashkeel) stripping and Levenshtein distance calculation.
///
/// Arabic diacritics (حركات التشكيل) such as Fathah, Dammah, Kasrah, Sukun,
/// Shadda, Tanwin are stripped before comparison because field collectors may
/// or may not include them depending on their device/keyboard settings.
///
/// FSD: FR-D-5 — Arabic name Levenshtein similarity (0–40 points).
/// </summary>
public static class ArabicNameSimilarityHelper
{
    // Unicode range for Arabic diacritical marks (tashkeel): U+064B to U+065F
    // Also includes Tatweel (kashida): U+0640
    private static readonly HashSet<UnicodeCategory> DiacriticCategories = new()
    {
        UnicodeCategory.NonSpacingMark,      // Fathah, Dammah, Kasrah, Sukun, Shadda
        UnicodeCategory.EnclosingMark
    };

    /// <summary>
    /// Compute similarity percentage (0–100) between two Arabic name strings.
    /// Returns 0 if either string is null/empty.
    /// </summary>
    /// <param name="name1">First Arabic name.</param>
    /// <param name="name2">Second Arabic name.</param>
    /// <returns>Similarity as a percentage (0.0 – 100.0).</returns>
    public static decimal ComputeSimilarity(string? name1, string? name2)
    {
        if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2))
            return 0m;

        var normalized1 = NormalizeArabic(name1);
        var normalized2 = NormalizeArabic(name2);

        if (normalized1.Length == 0 || normalized2.Length == 0)
            return 0m;

        if (normalized1 == normalized2)
            return 100m;

        var distance = LevenshteinDistance(normalized1, normalized2);
        var maxLength = Math.Max(normalized1.Length, normalized2.Length);

        // Similarity = 1 - (distance / maxLength), scaled to percentage
        var similarity = (1.0m - ((decimal)distance / maxLength)) * 100m;
        return Math.Max(0m, Math.Round(similarity, 1));
    }

    /// <summary>
    /// Compute a composite similarity score for a full Arabic name (first + father + family).
    /// Weighted: first name (30%), father name (30%), family name (40%).
    /// </summary>
    public static decimal ComputeFullNameSimilarity(
        string? firstName1, string? fatherName1, string? familyName1,
        string? firstName2, string? fatherName2, string? familyName2)
    {
        var firstSim = ComputeSimilarity(firstName1, firstName2);
        var fatherSim = ComputeSimilarity(fatherName1, fatherName2);
        var familySim = ComputeSimilarity(familyName1, familyName2);

        // Weighted average: family name carries more weight as it's the most stable identifier
        return Math.Round(firstSim * 0.30m + fatherSim * 0.30m + familySim * 0.40m, 1);
    }

    /// <summary>
    /// Normalize Arabic text for comparison:
    /// 1. Strip diacritical marks (tashkeel)
    /// 2. Remove tatweel/kashida (ـ U+0640)
    /// 3. Normalize Alef variants (أ إ آ → ا)
    /// 4. Normalize Taa Marbutah (ة → ه)
    /// 5. Normalize Yaa variants (ى → ي)
    /// 6. Trim and collapse whitespace
    /// </summary>
    internal static string NormalizeArabic(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder(input.Length);
        bool lastWasSpace = false;

        foreach (var ch in input)
        {
            // Skip diacritics
            if (DiacriticCategories.Contains(CharUnicodeInfo.GetUnicodeCategory(ch)))
                continue;

            // Skip tatweel
            if (ch == '\u0640')
                continue;

            // Normalize Alef variants
            var normalized = ch switch
            {
                '\u0623' => '\u0627', // أ → ا  (Alef with Hamza above)
                '\u0625' => '\u0627', // إ → ا  (Alef with Hamza below)
                '\u0622' => '\u0627', // آ → ا  (Alef with Madda)
                '\u0671' => '\u0627', // ٱ → ا  (Alef Wasla)
                '\u0629' => '\u0647', // ة → ه  (Taa Marbutah → Haa)
                '\u0649' => '\u064A', // ى → ي  (Alef Maksura → Yaa)
                _ => ch
            };

            // Collapse whitespace
            if (char.IsWhiteSpace(normalized))
            {
                if (!lastWasSpace && sb.Length > 0)
                {
                    sb.Append(' ');
                    lastWasSpace = true;
                }
                continue;
            }

            lastWasSpace = false;
            sb.Append(normalized);
        }

        // Trim trailing space
        if (sb.Length > 0 && sb[^1] == ' ')
            sb.Length--;

        return sb.ToString();
    }

    /// <summary>
    /// Compute the Levenshtein edit distance between two strings.
    /// Uses the Wagner-Fischer dynamic programming algorithm with O(min(m,n)) space.
    /// </summary>
    internal static int LevenshteinDistance(string source, string target)
    {
        if (source.Length == 0) return target.Length;
        if (target.Length == 0) return source.Length;

        // Ensure source is the shorter string for O(min(m,n)) space
        if (source.Length > target.Length)
            (source, target) = (target, source);

        var previousRow = new int[source.Length + 1];
        var currentRow = new int[source.Length + 1];

        for (int i = 0; i <= source.Length; i++)
            previousRow[i] = i;

        for (int j = 1; j <= target.Length; j++)
        {
            currentRow[0] = j;

            for (int i = 1; i <= source.Length; i++)
            {
                int cost = source[i - 1] == target[j - 1] ? 0 : 1;

                currentRow[i] = Math.Min(
                    Math.Min(
                        currentRow[i - 1] + 1,       // insertion
                        previousRow[i] + 1),          // deletion
                    previousRow[i - 1] + cost);        // substitution
            }

            // Swap rows
            (previousRow, currentRow) = (currentRow, previousRow);
        }

        return previousRow[source.Length];
    }
}
