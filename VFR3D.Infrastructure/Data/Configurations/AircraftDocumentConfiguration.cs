using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations;

public class AircraftDocumentConfiguration : IEntityTypeConfiguration<AircraftDocument>
{
    public void Configure(EntityTypeBuilder<AircraftDocument> builder)
    {
        builder.ToTable("aircraft_documents");

        // Primary Key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.AircraftId)
            .IsRequired()
            .HasColumnName("aircraft_id");

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("file_name");

        builder.Property(e => e.BlobName)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("blob_name");

        builder.Property(e => e.ContentType)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("content_type");

        builder.Property(e => e.FileSizeBytes)
            .HasColumnName("file_size_bytes");

        builder.Property(e => e.DisplayName)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("display_name");

        builder.Property(e => e.Description)
            .HasMaxLength(1000)
            .HasColumnName("description");

        builder.Property(e => e.Category)
            .HasColumnName("category")
            .HasConversion<string>();

        builder.Property(e => e.UploadedAt)
            .HasColumnName("uploaded_at");

        builder.Property(e => e.LastModifiedAt)
            .HasColumnName("last_modified_at");

        // Indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.AircraftId });

        // Relationships
        builder.HasOne(e => e.Aircraft)
            .WithMany(a => a.Documents)
            .HasForeignKey(e => e.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
