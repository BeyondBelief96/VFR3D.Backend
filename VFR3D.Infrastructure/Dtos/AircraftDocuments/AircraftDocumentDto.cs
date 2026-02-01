using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos.AircraftDocuments;

public record AircraftDocumentDto
{
    public string Id { get; init; } = string.Empty;
    public string AircraftId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DocumentCategory Category { get; init; }
    public DateTime UploadedAt { get; init; }
    public DateTime LastModifiedAt { get; init; }
}

public record AircraftDocumentListDto
{
    public string Id { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public DocumentCategory Category { get; init; }
    public DateTime UploadedAt { get; init; }
}

public record AircraftDocumentUrlDto
{
    public string Id { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}

public record CreateAircraftDocumentRequest
{
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DocumentCategory Category { get; init; } = DocumentCategory.Other;
}

public record UpdateAircraftDocumentRequest
{
    public string DisplayName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DocumentCategory Category { get; init; }
}
