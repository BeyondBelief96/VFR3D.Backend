using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VFR3D.Domain.Enums;

namespace VFR3D.Domain.Entities;

[Table("aircraft_documents")]
public class AircraftDocument
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = string.Empty;

    [Column("aircraft_id")]
    [Required]
    public string AircraftId { get; set; } = string.Empty;

    [Column("user_id")]
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Column("file_name")]
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Column("blob_name")]
    [Required]
    [MaxLength(500)]
    public string BlobName { get; set; } = string.Empty;

    [Column("content_type")]
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Column("file_size_bytes")]
    public long FileSizeBytes { get; set; }

    [Column("display_name")]
    [Required]
    [MaxLength(255)]
    public string DisplayName { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(1000)]
    public string? Description { get; set; }

    [Column("category")]
    public DocumentCategory Category { get; set; }

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; }

    [Column("last_modified_at")]
    public DateTime LastModifiedAt { get; set; }

    // Navigation Properties
    public virtual Aircraft Aircraft { get; set; } = null!;
}
