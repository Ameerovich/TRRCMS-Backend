using TRRCMS.Application.Import.Models;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service interface for .uhc package intake and integrity verification.
/// Covers UC-003 Stage 2 — S12 (Verify Package Integrity):
///   - SHA-256 checksum verification
///   - Digital signature verification
///   - Manifest parsing (SQLite metadata table)
///   - Vocabulary compatibility checking (semver)
///   - Package archival to immutable store
/// 
/// Implementations should be stateless — all state is persisted
/// in the ImportPackage entity via the repository layer.
/// 
/// Delivery Plan Task: TRRCMS-IMP-01.
/// FSD: FR-D-2 (Import Management), FR-D-3 (Validation & Verification).
/// </summary>
public interface IImportService
{
    // ==================== CHECKSUM & INTEGRITY ====================

    /// <summary>
    /// Compute SHA-256 checksum of the .uhc file content.
    /// Resets stream position to 0 after computation.
    /// </summary>
    /// <param name="fileStream">The .uhc file stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SHA-256 hash as lowercase hex string.</returns>
    Task<string> ComputeChecksumAsync(Stream fileStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compute a deterministic SHA-256 checksum of all data table contents in a .uhc
    /// SQLite package, **excluding** the manifest table. This is the checksum that the
    /// mobile app stores in the manifest for integrity verification.
    ///
    /// Algorithm (must match mobile implementation):
    ///   1. Enumerate all user tables except 'manifest' and 'attachments', sorted alphabetically
    ///   2. For each table, read all rows ordered by rowid
    ///   3. For each row, serialize columns in alphabetical order as "col=value" pairs
    ///      separated by tab characters, with NULL represented as "\0"
    ///   4. Each row terminated by newline; each table preceded by a header line "TABLE:name\n"
    ///   5. SHA-256 hash the entire UTF-8 byte sequence
    ///
    /// This avoids the circular dependency where the manifest's checksum field would
    /// change the file hash if the whole file were hashed.
    /// </summary>
    /// <param name="uhcFilePath">File system path to the .uhc file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>SHA-256 hash as lowercase hex string.</returns>
    Task<string> ComputeContentChecksumAsync(string uhcFilePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify that the file's SHA-256 checksum matches the expected value from the manifest.
    /// </summary>
    /// <param name="fileStream">The .uhc file stream.</param>
    /// <param name="expectedChecksum">SHA-256 hash from the manifest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if checksums match.</returns>
    Task<bool> VerifyChecksumAsync(Stream fileStream, string expectedChecksum, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify the digital signature of the .uhc package.
    /// If digital signatures are not required (config), returns true.
    /// </summary>
    /// <param name="fileStream">The .uhc file stream.</param>
    /// <param name="signature">Digital signature string from the manifest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if signature is valid (or not required).</returns>
    Task<bool> VerifyDigitalSignatureAsync(Stream fileStream, string? signature, CancellationToken cancellationToken = default);

    // ==================== MANIFEST PARSING ====================

    /// <summary>
    /// Open the .uhc file as a SQLite database and parse the manifest table.
    /// The .uhc file must be saved to disk first (SQLite requires file path).
    /// </summary>
    /// <param name="uhcFilePath">File system path to the .uhc file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed manifest data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if manifest table is missing or corrupt.</exception>
    Task<ManifestData> ParseManifestAsync(string uhcFilePath, CancellationToken cancellationToken = default);

    // ==================== VOCABULARY COMPATIBILITY ====================

    /// <summary>
    /// Check vocabulary version compatibility between the package and the server.
    /// Compares each vocabulary domain using semver rules:
    ///   - MAJOR mismatch → incompatible (quarantine)
    ///   - MINOR mismatch → compatible with warnings
    ///   - PATCH mismatch → fully compatible
    /// </summary>
    /// <param name="manifest">Parsed manifest containing package vocabulary versions.</param>
    /// <returns>Compatibility result with per-domain details.</returns>
    Task<VocabularyCompatibilityResult> CheckVocabularyCompatibilityAsync(ManifestData manifest, CancellationToken cancellationToken = default);

    // ==================== FILE MANAGEMENT ====================

    /// <summary>
    /// Save the uploaded .uhc file to the temporary package storage path.
    /// Returns the file system path where the file was saved.
    /// </summary>
    /// <param name="fileStream">The uploaded file stream.</param>
    /// <param name="fileName">Original file name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File system path to the saved .uhc file.</returns>
    Task<string> SavePackageFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Archive the .uhc package to the immutable archive store.
    /// Path format: archives/YYYY/MM/[packageId].uhc
    /// </summary>
    /// <param name="sourceFilePath">Current file path of the .uhc package.</param>
    /// <param name="packageId">Package GUID for naming.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Archive path where the file was stored.</returns>
    Task<string> ArchivePackageAsync(string sourceFilePath, Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a temporary package file (cleanup after staging or on failure).
    /// </summary>
    /// <param name="filePath">File path to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeletePackageFileAsync(string filePath, CancellationToken cancellationToken = default);
}
