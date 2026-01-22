namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Interface for file storage operations
/// Supports local file system, with architecture ready for cloud storage (Azure Blob, AWS S3)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Save uploaded file to storage
    /// </summary>
    /// <param name="file">File stream to save</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="category">Storage category (property-photos, identification-documents, tenure-documents)</param>
    /// <param name="surveyId">Survey ID for organization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File path where file was saved</returns>
    Task<string> SaveFileAsync(
        Stream file,
        string fileName,
        string category,
        Guid surveyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file stream for download
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File stream</returns>
    Task<Stream> GetFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete file from storage
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if file exists
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>True if file exists</returns>
    Task<bool> FileExistsAsync(string filePath);

    /// <summary>
    /// Calculate SHA-256 hash of file
    /// </summary>
    /// <param name="fileStream">File stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SHA-256 hash as hex string</returns>
    Task<string> CalculateFileHashAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get MIME type from file extension
    /// </summary>
    /// <param name="fileName">Filename with extension</param>
    /// <returns>MIME type</returns>
    string GetMimeType(string fileName);

    /// <summary>
    /// Validate file size
    /// </summary>
    /// <param name="fileSizeBytes">File size in bytes</param>
    /// <returns>True if size is valid</returns>
    bool ValidateFileSize(long fileSizeBytes);

    /// <summary>
    /// Validate file extension
    /// </summary>
    /// <param name="fileName">Filename with extension</param>
    /// <param name="allowedExtensions">List of allowed extensions</param>
    /// <returns>True if extension is valid</returns>
    bool ValidateFileExtension(string fileName, string[] allowedExtensions);
}