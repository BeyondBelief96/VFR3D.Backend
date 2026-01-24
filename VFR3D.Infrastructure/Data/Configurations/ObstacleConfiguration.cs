using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations;

public class ObstacleConfiguration : IEntityTypeConfiguration<Obstacle>
{
    public void Configure(EntityTypeBuilder<Obstacle> builder)
    {
        builder.ToTable("obstacles");

        builder.HasKey(e => e.OasNumber);

        builder.Property(e => e.OasNumber).HasMaxLength(10).IsRequired();
        builder.Property(e => e.OasCode).HasMaxLength(2).IsRequired();
        builder.Property(e => e.ObstacleNumber).HasMaxLength(6).IsRequired();
        builder.Property(e => e.VerificationStatus).HasMaxLength(1);
        builder.Property(e => e.CountryId).HasMaxLength(2);
        builder.Property(e => e.StateId).HasMaxLength(2);
        builder.Property(e => e.CityName).HasMaxLength(16);
        builder.Property(e => e.LatHemisphere).HasMaxLength(1);
        builder.Property(e => e.LongHemisphere).HasMaxLength(1);
        builder.Property(e => e.LatDecimal).HasColumnType("decimal(10,8)");
        builder.Property(e => e.LongDecimal).HasColumnType("decimal(11,8)");
        builder.Property(e => e.LatSeconds).HasColumnType("decimal(6,2)");
        builder.Property(e => e.LongSeconds).HasColumnType("decimal(6,2)");
        builder.Property(e => e.ObstacleType).HasMaxLength(18);
        builder.Property(e => e.Lighting).HasMaxLength(1);
        builder.Property(e => e.HorizontalAccuracy).HasMaxLength(1);
        builder.Property(e => e.VerticalAccuracy).HasMaxLength(1);
        builder.Property(e => e.MarkIndicator).HasMaxLength(1);
        builder.Property(e => e.FaaStudyNumber).HasMaxLength(14);
        builder.Property(e => e.Action).HasMaxLength(1);
        builder.Property(e => e.JulianDate).HasMaxLength(7);

        builder.Property(e => e.Location)
            .HasColumnType("geography(Point, 4326)");

        // Indexes for efficient querying
        builder.HasIndex(e => e.OasNumber).IsUnique();
        builder.HasIndex(e => e.StateId);
        builder.HasIndex(e => e.ObstacleType);
        builder.HasIndex(e => e.HeightAgl);
        builder.HasIndex(e => e.HeightAmsl);

        // Spatial index for location-based queries
        builder.HasIndex(e => e.Location)
            .HasMethod("gist");
    }
}
