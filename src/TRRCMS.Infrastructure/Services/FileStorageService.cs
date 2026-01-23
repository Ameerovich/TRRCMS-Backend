using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// File storage service implementation for local file system
/// Architecture supports future migration to cloud storage (Azure Blob, AWS S3)
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly long _maxFileSizeBytes;
    private readonly string[] _allowedImageExtensions;
    private readonly string[] _allowedDocumentExtensions;

    public FileStorageService(IConfiguration configuration)
    {
        _uploadPath = configuration["FileStorage:LocalPath"] ?? "wwwroot/uploads";
        var maxSizeMB = configuration.GetValue<int>("FileStorage:MaxFileSizeMB", 10);
        _maxFileSizeBytes = maxSizeMB * 1024 * 1024; // Convert MB to bytes

        _allowedImageExtensions = configuration.GetSection("FileStorage:AllowedImageExtensions")
            .Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png" };

        _allowedDocumentExtensions = configuration.GetSection("FileStorage:AllowedDocumentExtensions")
            .Get<string[]>() ?? new[] { ".pdf", ".jpg", ".jpeg", ".png" };

        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<string> SaveFileAsync(
        Stream file,
        string fileName,
        string category,
        Guid surveyId,
        CancellationToken cancellationToken = default)
    {
        // Create category directory if it doesn't exist
        var categoryPath = Path.Combine(_uploadPath, category);
        if (!Directory.Exists(categoryPath))
        {
            Directory.CreateDirectory(categoryPath);
        }

        // Create survey-specific directory
        var surveyPath = Path.Combine(categoryPath, surveyId.ToString());
        if (!Directory.Exists(surveyPath))
        {
            Directory.CreateDirectory(surveyPath);
        }

        // Generate unique filename to avoid collisions
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(surveyPath, uniqueFileName);

        // Save file
        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        // Return relative path for database storage
        return Path.Combine(category, surveyId.ToString(), uniqueFileName);
    }

    public async Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_uploadPath, filePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var memoryStream = new MemoryStream();
        using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
        {
            await fileStream.CopyToAsync(memoryStream, cancellationToken);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_uploadPath, filePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadPath, filePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public async Task<string> CalculateFileHashAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        using (var sha256 = SHA256.Create())
        {
            fileStream.Position = 0;
            var hashBytes = await sha256.ComputeHashAsync(fileStream, cancellationToken);
            fileStream.Position = 0;
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }

    public string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }

    public bool ValidateFileSize(long fileSizeBytes)
    {
        return fileSizeBytes > 0 && fileSizeBytes <= _maxFileSizeBytes;
    }

    public bool ValidateFileExtension(string fileName, string[] allowedExtensions)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }
}