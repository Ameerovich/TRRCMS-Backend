using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Models;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Implementation of IImportService.
/// Handles .uhc package intake: checksum verification, manifest parsing (SQLite),
/// vocabulary compatibility checking, and file management.
/// 
/// The .uhc file format is a renamed SQLite database containing:
///   - manifest table: package metadata, checksums, vocabulary versions
///   - surveys, buildings, property_units, persons, households,
///     person_property_relations, evidences, claims: data tables
///   - attachments table or blob storage for evidence files
/// </summary>
public class ImportService : IImportService
{
    private readonly ImportPipelineSettings _settings;
    private readonly ILogger<ImportService> _logger;
    private readonly IVocabularyVersionProvider _vocabularyVersionProvider;

    public ImportService(
        IOptions<ImportPipelineSettings> settings,
        ILogger<ImportService> logger,
        IVocabularyVersionProvider vocabularyVersionProvider)
    {
        _settings = settings.Value;
        _logger = logger;
        _vocabularyVersionProvider = vocabularyVersionProvider;
    }

    // ==================== CHECKSUM & INTEGRITY ====================

    public async Task<string> ComputeChecksumAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        using var sha256 = SHA256.Create();
        fileStream.Position = 0;
        var hashBytes = await sha256.ComputeHashAsync(fileStream, cancellationToken);
        fileStream.Position = 0;
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <inheritdoc />
    public async Task<string> ComputeContentChecksumAsync(
        string uhcFilePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(uhcFilePath))
            throw new FileNotFoundException("Package file not found", uhcFilePath);

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = uhcFilePath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // 1. Enumerate all user data tables, excluding manifest and SQLite internals
        var excludedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "manifest", "attachments", "sqlite_sequence"
        };

        var tables = new List<string>();
        using (var listCmd = connection.CreateCommand())
        {
            listCmd.CommandText =
                "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
            using var reader = await listCmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var tableName = reader.GetString(0);
                if (!tableName.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase)
                    && !excludedTables.Contains(tableName))
                {
                    tables.Add(tableName);
                }
            }
        }

        // tables is already sorted alphabetically (ORDER BY name)

        // 2. Build canonical representation and hash incrementally
        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var encoding = System.Text.Encoding.UTF8;

        foreach (var table in tables)
        {
            // Table header
            sha256.AppendData(encoding.GetBytes($"TABLE:{table}\n"));

            // Get column names sorted alphabetically
            var columns = new List<string>();
            using (var pragmaCmd = connection.CreateCommand())
            {
                pragmaCmd.CommandText = $"PRAGMA table_info(\"{table}\")";
                using var pragmaReader = await pragmaCmd.ExecuteReaderAsync(cancellationToken);
                while (await pragmaReader.ReadAsync(cancellationToken))
                {
                    columns.Add(pragmaReader.GetString(1)); // column name is at index 1
                }
            }
            columns.Sort(StringComparer.Ordinal);

            // Read all rows ordered by rowid
            using var rowCmd = connection.CreateCommand();
            rowCmd.CommandText = $"SELECT * FROM \"{table}\" ORDER BY rowid";
            using var rowReader = await rowCmd.ExecuteReaderAsync(cancellationToken);

            // Build column index lookup (reader column order → sorted column order)
            var colIndexMap = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < rowReader.FieldCount; i++)
            {
                colIndexMap[rowReader.GetName(i)] = i;
            }

            while (await rowReader.ReadAsync(cancellationToken))
            {
                var parts = new List<string>(columns.Count);
                foreach (var col in columns)
                {
                    if (colIndexMap.TryGetValue(col, out var idx))
                    {
                        var value = rowReader.IsDBNull(idx) ? "\\0" : rowReader.GetValue(idx)?.ToString() ?? "\\0";
                        parts.Add($"{col}={value}");
                    }
                }

                var rowLine = string.Join("\t", parts) + "\n";
                sha256.AppendData(encoding.GetBytes(rowLine));
            }
        }

        var hashBytes = sha256.GetHashAndReset();
        var checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        _logger.LogDebug(
            "Content checksum computed over {TableCount} tables: {Checksum}",
            tables.Count, checksum);

        return checksum;
    }

    public async Task<bool> VerifyChecksumAsync(
        Stream fileStream,
        string expectedChecksum,
        CancellationToken cancellationToken = default)
    {
        var computed = await ComputeChecksumAsync(fileStream, cancellationToken);
        return string.Equals(computed, expectedChecksum, StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> VerifyDigitalSignatureAsync(
        Stream fileStream,
        string? signature,
        CancellationToken cancellationToken = default)
    {
        // If digital signatures are not required by config, always pass
        if (!_settings.RequireDigitalSignature)
        {
            _logger.LogDebug("Digital signature verification skipped (not required by config)");
            return Task.FromResult(true);
        }

        // If required but no signature provided, fail
        if (string.IsNullOrWhiteSpace(signature))
        {
            _logger.LogWarning("Digital signature required but not provided in package");
            return Task.FromResult(false);
        }

        // TODO: Implement actual signature verification when mobile team provides
        // the signing mechanism. For now, accept any non-empty signature.
        // Future: RSA/ECDSA signature verification using a known public key.
        _logger.LogDebug("Digital signature verification: placeholder (accepting non-empty signature)");
        return Task.FromResult(true);
    }

    // ==================== MANIFEST PARSING ====================

    public async Task<ManifestData> ParseManifestAsync(
        string uhcFilePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(uhcFilePath))
            throw new FileNotFoundException("Package file not found", uhcFilePath);

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = uhcFilePath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false          // Release file handle immediately on dispose (Windows)
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // Verify manifest table exists
        using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText =
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='manifest'";
        var tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(cancellationToken));

        if (tableExists == 0)
            throw new InvalidOperationException(
                "Invalid .uhc package: manifest table not found. " +
                "The file may be corrupted or not a valid .uhc container.");

        // Read manifest — expecting a single row with key-value pairs
        var manifest = new ManifestData();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT key, value FROM manifest";

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var manifestDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync(cancellationToken))
        {
            var key = reader.GetString(0);
            var value = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            manifestDict[key] = value;
        }

        // Parse required fields
        manifest.PackageId = ParseGuid(manifestDict, "package_id");
        manifest.SchemaVersion = GetValueOrDefault(manifestDict, "schema_version", "1.0.0");
        manifest.CreatedUtc = ParseDateTime(manifestDict, "created_utc");
        manifest.DeviceId = GetValueOrDefault(manifestDict, "device_id", "");
        manifest.AppVersion = GetValueOrDefault(manifestDict, "app_version", "");
        manifest.ExportedByUserId = ParseGuid(manifestDict, "exported_by_user_id");
        manifest.ExportedDateUtc = ParseDateTime(manifestDict, "exported_date_utc");
        manifest.Checksum = GetValueOrDefault(manifestDict, "checksum", "");
        manifest.DigitalSignature = GetValueOrDefault(manifestDict, "digital_signature", null);
        manifest.FormSchemaVersion = GetValueOrDefault(manifestDict, "form_schema_version", "1.0.0");

        // Parse content counts
        manifest.SurveyCount = ParseInt(manifestDict, "survey_count");
        manifest.BuildingCount = ParseInt(manifestDict, "building_count");
        manifest.PropertyUnitCount = ParseInt(manifestDict, "property_unit_count");
        manifest.PersonCount = ParseInt(manifestDict, "person_count");
        manifest.HouseholdCount = ParseInt(manifestDict, "household_count");
        manifest.RelationCount = ParseInt(manifestDict, "relation_count");
        manifest.ClaimCount = ParseInt(manifestDict, "claim_count");
        manifest.DocumentCount = ParseInt(manifestDict, "document_count");
        manifest.TotalAttachmentSizeBytes = ParseLong(manifestDict, "total_attachment_size_bytes");

        // Parse vocabulary versions (JSON string → Dictionary)
        var vocabJson = GetValueOrDefault(manifestDict, "vocab_versions", null);
        if (!string.IsNullOrWhiteSpace(vocabJson))
        {
            try
            {
                manifest.VocabVersions = JsonSerializer.Deserialize<Dictionary<string, string>>(vocabJson)
                    ?? new Dictionary<string, string>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse vocab_versions JSON: {Json}", vocabJson);
                manifest.VocabVersions = new Dictionary<string, string>();
            }
        }

        _logger.LogDebug(
            "Manifest parsed successfully: PackageId={PackageId}, Schema={Schema}, " +
            "Surveys={Surveys}, Buildings={Buildings}, Persons={Persons}, Claims={Claims}",
            manifest.PackageId, manifest.SchemaVersion,
            manifest.SurveyCount, manifest.BuildingCount,
            manifest.PersonCount, manifest.ClaimCount);

        return manifest;
    }

    // ==================== VOCABULARY COMPATIBILITY ====================

    public async Task<VocabularyCompatibilityResult> CheckVocabularyCompatibilityAsync(ManifestData manifest, CancellationToken cancellationToken = default)
    {
        var serverVersions = await _vocabularyVersionProvider.GetAllCurrentVersionsAsync(cancellationToken);

        var result = new VocabularyCompatibilityResult
        {
            IsCompatible = true,
            IsFullyCompatible = true,
            Items = new List<VocabularyCheckItem>()
        };

        var issues = new List<string>();

        foreach (var (domain, packageVersionStr) in manifest.VocabVersions)
        {
            if (!serverVersions.TryGetValue(domain, out var serverVersionStr))
            {
                // Server doesn't know this vocabulary domain — warn but accept
                result.Items.Add(new VocabularyCheckItem
                {
                    Domain = domain,
                    PackageVersion = packageVersionStr,
                    ServerVersion = "N/A",
                    Level = VocabularyCompatibilityLevel.UnknownDomain,
                    Message = $"Unknown vocabulary domain '{domain}' (v{packageVersionStr})"
                });
                result.IsFullyCompatible = false;
                issues.Add($"{domain}: unknown domain (package has v{packageVersionStr})");
                continue;
            }

            var comparison = CompareSemanticVersions(packageVersionStr, serverVersionStr);

            result.Items.Add(new VocabularyCheckItem
            {
                Domain = domain,
                PackageVersion = packageVersionStr,
                ServerVersion = serverVersionStr,
                Level = comparison,
                Message = comparison switch
                {
                    VocabularyCompatibilityLevel.Identical =>
                        null,
                    VocabularyCompatibilityLevel.PatchDifference =>
                        $"{domain}: patch difference (package v{packageVersionStr}, server v{serverVersionStr})",
                    VocabularyCompatibilityLevel.MinorDifference =>
                        $"{domain}: minor difference (package v{packageVersionStr}, server v{serverVersionStr})",
                    VocabularyCompatibilityLevel.MajorDifference =>
                        $"{domain}: MAJOR incompatibility (package v{packageVersionStr}, server v{serverVersionStr})",
                    _ => null
                }
            });

            switch (comparison)
            {
                case VocabularyCompatibilityLevel.MajorDifference:
                    result.IsCompatible = false;
                    result.IsFullyCompatible = false;
                    issues.Add($"{domain}: MAJOR version mismatch (package v{packageVersionStr}, server v{serverVersionStr})");
                    break;
                case VocabularyCompatibilityLevel.MinorDifference:
                    result.IsFullyCompatible = false;
                    issues.Add($"{domain}: minor version difference (package v{packageVersionStr}, server v{serverVersionStr})");
                    break;
                case VocabularyCompatibilityLevel.PatchDifference:
                    // Patch differences are fully compatible, but note them
                    break;
            }
        }

        result.Summary = issues.Count > 0 ? string.Join("; ", issues) : null;
        result.VersionsJson = JsonSerializer.Serialize(manifest.VocabVersions);
        result.IssuesJson = issues.Count > 0 ? JsonSerializer.Serialize(issues) : null;

        return result;
    }

    // ==================== FILE MANAGEMENT ====================

    public async Task<string> SavePackageFileAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var storagePath = Path.GetFullPath(_settings.PackageStoragePath);
        Directory.CreateDirectory(storagePath);

        // Unique filename to avoid collisions: {guid}_{originalname}.uhc
        var uniqueName = $"{Guid.NewGuid():N}_{Path.GetFileNameWithoutExtension(fileName)}.uhc";
        var filePath = Path.Combine(storagePath, uniqueName);

        using var outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        fileStream.Position = 0;
        await fileStream.CopyToAsync(outputStream, cancellationToken);

        _logger.LogDebug("Package saved to: {FilePath}", filePath);
        return filePath;
    }

    public Task<string> ArchivePackageAsync(
        string sourceFilePath,
        Guid packageId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var archiveDir = Path.Combine(
            Path.GetFullPath(_settings.ArchiveBasePath),
            now.Year.ToString("D4"),
            now.Month.ToString("D2"));

        Directory.CreateDirectory(archiveDir);

        var archivePath = Path.Combine(archiveDir, $"{packageId:N}.uhc");

        // Copy rather than move — the source may still be needed for staging
        File.Copy(sourceFilePath, archivePath, overwrite: true);

        _logger.LogInformation("Package archived to: {ArchivePath}", archivePath);
        return Task.FromResult(archivePath);
    }

    public async Task DeletePackageFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            return;

        // Retry with back-off: SQLite connection pooling on Windows may hold
        // the file handle briefly even after the connection is disposed.
        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                File.Delete(filePath);
                _logger.LogDebug("Deleted package file: {FilePath}", filePath);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                _logger.LogDebug(
                    "File still locked, retrying delete (attempt {Attempt}/{Max}): {FilePath}",
                    attempt, maxAttempts, filePath);

                // Clear any lingering SQLite pooled connections
                SqliteConnection.ClearAllPools();
                await Task.Delay(200 * attempt, cancellationToken);
            }
        }
    }

    // ==================== PRIVATE HELPERS ====================

    /// <summary>
    /// Compare two semver strings and return the compatibility level.
    /// </summary>
    private static VocabularyCompatibilityLevel CompareSemanticVersions(
        string packageVersion, string serverVersion)
    {
        var pkg = ParseSemver(packageVersion);
        var srv = ParseSemver(serverVersion);

        if (pkg.Major != srv.Major)
            return VocabularyCompatibilityLevel.MajorDifference;

        if (pkg.Minor != srv.Minor)
            return VocabularyCompatibilityLevel.MinorDifference;

        if (pkg.Patch != srv.Patch)
            return VocabularyCompatibilityLevel.PatchDifference;

        return VocabularyCompatibilityLevel.Identical;
    }

    /// <summary>
    /// Parse a semver string "MAJOR.MINOR.PATCH" into components.
    /// Tolerant: missing parts default to 0.
    /// </summary>
    private static (int Major, int Minor, int Patch) ParseSemver(string version)
    {
        var parts = (version ?? "0.0.0").Split('.');
        var major = parts.Length > 0 && int.TryParse(parts[0], out var m) ? m : 0;
        var minor = parts.Length > 1 && int.TryParse(parts[1], out var mi) ? mi : 0;
        var patch = parts.Length > 2 && int.TryParse(parts[2], out var p) ? p : 0;
        return (major, minor, patch);
    }

    private static Guid ParseGuid(Dictionary<string, string> dict, string key)
    {
        if (dict.TryGetValue(key, out var value) && Guid.TryParse(value, out var result))
            return result;

        throw new InvalidOperationException(
            $"Manifest is missing or has invalid value for required field '{key}'");
    }

    private static DateTime ParseDateTime(Dictionary<string, string> dict, string key)
    {
        if (dict.TryGetValue(key, out var value) && DateTime.TryParse(value, out var result))
            return result.ToUniversalTime();

        // Fallback to UtcNow if not present
        return DateTime.UtcNow;
    }

    private static int ParseInt(Dictionary<string, string> dict, string key)
    {
        if (dict.TryGetValue(key, out var value) && int.TryParse(value, out var result))
            return result;
        return 0;
    }

    private static long ParseLong(Dictionary<string, string> dict, string key)
    {
        if (dict.TryGetValue(key, out var value) && long.TryParse(value, out var result))
            return result;
        return 0;
    }

    private static string? GetValueOrDefault(
        Dictionary<string, string> dict, string key, string? defaultValue)
    {
        return dict.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
