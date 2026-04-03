using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Dtos.AircraftDocuments;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class AircraftDocumentMapper
{
    public static AircraftDocumentDto MapToDto(AircraftDocument entity)
    {
        return new AircraftDocumentDto
        {
            Id = entity.Id,
            AircraftId = entity.AircraftId,
            UserId = entity.UserId,
            FileName = entity.FileName,
            ContentType = entity.ContentType,
            FileSizeBytes = entity.FileSizeBytes,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            Category = entity.Category,
            UploadedAt = entity.UploadedAt,
            LastModifiedAt = entity.LastModifiedAt
        };
    }

    public static AircraftDocumentListDto MapToListDto(AircraftDocument entity)
    {
        return new AircraftDocumentListDto
        {
            Id = entity.Id,
            FileName = entity.FileName,
            ContentType = entity.ContentType,
            FileSizeBytes = entity.FileSizeBytes,
            DisplayName = entity.DisplayName,
            Category = entity.Category,
            UploadedAt = entity.UploadedAt
        };
    }

    public static AircraftDocument CreateEntity(
        string userId,
        string aircraftId,
        string fileName,
        string blobName,
        string contentType,
        long fileSizeBytes,
        string displayName,
        string? description,
        DocumentCategory category)
    {
        var now = DateTime.UtcNow;
        return new AircraftDocument
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            AircraftId = aircraftId,
            FileName = fileName,
            BlobName = blobName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            DisplayName = displayName,
            Description = description,
            Category = category,
            UploadedAt = now,
            LastModifiedAt = now
        };
    }
}
