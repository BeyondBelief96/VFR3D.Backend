using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos.AircraftDocuments;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Infrastructure.Services;

public class AircraftDocumentService : IAircraftDocumentService
{
    private readonly VFR3DDbContext _context;
    private readonly ICloudStorageService _storageService;
    private readonly CloudStorageSettings _storageSettings;
    private readonly ILogger<AircraftDocumentService> _logger;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    private static readonly TimeSpan SasUrlExpiration = TimeSpan.FromHours(1);

    public AircraftDocumentService(
        VFR3DDbContext context,
        ICloudStorageService storageService,
        IOptions<CloudStorageSettings> storageSettings,
        ILogger<AircraftDocumentService> logger)
    {
        _context = context;
        _storageService = storageService;
        _storageSettings = storageSettings.Value;
        _logger = logger;
    }

    public async Task<AircraftDocumentDto> UploadDocumentAsync(
        string userId,
        string aircraftId,
        Stream fileStream,
        string fileName,
        string contentType,
        CreateAircraftDocumentRequest request)
    {
        // Validate content type
        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new ArgumentException($"File type '{contentType}' is not allowed. Allowed types: PDF, images (JPEG, PNG, GIF, WebP), and Office documents (DOC, DOCX, XLS, XLSX).");
        }

        // Validate aircraft ownership
        var aircraft = await _context.Aircraft
            .FirstOrDefaultAsync(a => a.Id == aircraftId && a.UserId == userId);

        if (aircraft == null)
        {
            throw new KeyNotFoundException($"Aircraft not found with ID {aircraftId}");
        }

        // Generate document ID and blob path
        var documentId = Guid.NewGuid().ToString();
        var sanitizedFileName = SanitizeFileName(fileName);
        var blobName = $"{userId}/{aircraftId}/{documentId}_{sanitizedFileName}";

        try
        {
            // Upload to blob storage
            await _storageService.UploadBlobAsync(
                _storageSettings.AircraftDocumentsContainerName,
                blobName,
                fileStream,
                contentType);

            // Create database record
            var document = AircraftDocumentMapper.CreateEntity(
                userId,
                aircraftId,
                fileName,
                blobName,
                contentType,
                fileStream.Length,
                request.DisplayName,
                request.Description,
                request.Category);

            document.Id = documentId;

            _context.AircraftDocuments.Add(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Uploaded document {DocumentId} for aircraft {AircraftId} by user {UserId}",
                documentId, aircraftId, userId);

            return AircraftDocumentMapper.MapToDto(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error uploading document for aircraft {AircraftId} by user {UserId}",
                aircraftId, userId);
            throw;
        }
    }

    public async Task<List<AircraftDocumentListDto>> GetDocumentsForAircraftAsync(string userId, string aircraftId)
    {
        var documents = await _context.AircraftDocuments
            .Where(d => d.UserId == userId && d.AircraftId == aircraftId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

        return documents.Select(AircraftDocumentMapper.MapToListDto).ToList();
    }

    public async Task<AircraftDocumentDto?> GetDocumentAsync(string userId, string documentId)
    {
        var document = await _context.AircraftDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId);

        return document == null ? null : AircraftDocumentMapper.MapToDto(document);
    }

    public async Task<AircraftDocumentUrlDto> GetDocumentUrlAsync(string userId, string documentId)
    {
        var document = await _context.AircraftDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId);

        if (document == null)
        {
            throw new KeyNotFoundException($"Document not found with ID {documentId}");
        }

        var url = await _storageService.GeneratePresignedUrlAsync(
            _storageSettings.AircraftDocumentsContainerName,
            document.BlobName,
            SasUrlExpiration);

        return new AircraftDocumentUrlDto
        {
            Id = document.Id,
            Url = url,
            ExpiresAt = DateTime.UtcNow.Add(SasUrlExpiration)
        };
    }

    public async Task<AircraftDocumentDto> UpdateDocumentMetadataAsync(
        string userId,
        string documentId,
        UpdateAircraftDocumentRequest request)
    {
        var document = await _context.AircraftDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId);

        if (document == null)
        {
            throw new KeyNotFoundException($"Document not found with ID {documentId}");
        }

        document.DisplayName = request.DisplayName;
        document.Description = request.Description;
        document.Category = request.Category;
        document.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Updated metadata for document {DocumentId} by user {UserId}",
            documentId, userId);

        return AircraftDocumentMapper.MapToDto(document);
    }

    public async Task<AircraftDocumentDto> ReplaceDocumentAsync(
        string userId,
        string documentId,
        Stream fileStream,
        string fileName,
        string contentType)
    {
        // Validate content type
        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new ArgumentException($"File type '{contentType}' is not allowed. Allowed types: PDF, images (JPEG, PNG, GIF, WebP), and Office documents (DOC, DOCX, XLS, XLSX).");
        }

        var document = await _context.AircraftDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId);

        if (document == null)
        {
            throw new KeyNotFoundException($"Document not found with ID {documentId}");
        }

        var oldBlobName = document.BlobName;
        var sanitizedFileName = SanitizeFileName(fileName);
        var newBlobName = $"{userId}/{document.AircraftId}/{documentId}_{sanitizedFileName}";

        try
        {
            // Upload new blob
            await _storageService.UploadBlobAsync(
                _storageSettings.AircraftDocumentsContainerName,
                newBlobName,
                fileStream,
                contentType);

            // Delete old blob if different path
            if (oldBlobName != newBlobName)
            {
                try
                {
                    await _storageService.DeleteBlobAsync(
                        _storageSettings.AircraftDocumentsContainerName,
                        oldBlobName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to delete old blob {BlobName} for document {DocumentId}",
                        oldBlobName, documentId);
                }
            }

            // Update database record
            document.FileName = fileName;
            document.BlobName = newBlobName;
            document.ContentType = contentType;
            document.FileSizeBytes = fileStream.Length;
            document.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Replaced document {DocumentId} for user {UserId}",
                documentId, userId);

            return AircraftDocumentMapper.MapToDto(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error replacing document {DocumentId} for user {UserId}",
                documentId, userId);
            throw;
        }
    }

    public async Task DeleteDocumentAsync(string userId, string documentId)
    {
        var document = await _context.AircraftDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId);

        if (document == null)
        {
            throw new KeyNotFoundException($"Document not found with ID {documentId}");
        }

        try
        {
            // Delete from blob storage
            await _storageService.DeleteBlobAsync(
                _storageSettings.AircraftDocumentsContainerName,
                document.BlobName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to delete blob {BlobName} for document {DocumentId}",
                document.BlobName, documentId);
        }

        // Delete from database
        _context.AircraftDocuments.Remove(document);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Deleted document {DocumentId} for user {UserId}",
            documentId, userId);
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove or replace invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Limit length
        if (sanitized.Length > 200)
        {
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);
            sanitized = nameWithoutExtension[..(200 - extension.Length)] + extension;
        }

        return sanitized;
    }
}
