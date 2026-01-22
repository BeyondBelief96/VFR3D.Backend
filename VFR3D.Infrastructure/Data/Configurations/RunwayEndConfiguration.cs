using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations;

public class RunwayEndConfiguration : IEntityTypeConfiguration<RunwayEnd>
{
    public void Configure(EntityTypeBuilder<RunwayEnd> builder)
    {
        builder.ToTable("runway_ends");

        // Primary Key
        builder.HasKey(e => e.Id);

        // Required fields
        builder.Property(e => e.SiteNo)
            .IsRequired()
            .HasMaxLength(9);

        builder.Property(e => e.RunwayIdRef)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(e => e.RunwayEndId)
            .IsRequired()
            .HasMaxLength(3);

        // Optional fields with max lengths
        builder.Property(e => e.ApproachType)
            .HasMaxLength(10);

        builder.Property(e => e.RunwayMarkingsType)
            .HasMaxLength(5);

        builder.Property(e => e.RunwayMarkingsCondition)
            .HasMaxLength(1);

        builder.Property(e => e.VisualGlideSlopeIndicator)
            .HasMaxLength(5);

        builder.Property(e => e.RunwayVisualRangeEquipment)
            .HasMaxLength(3);

        builder.Property(e => e.ApproachLightSystem)
            .HasMaxLength(8);

        builder.Property(e => e.ControllingObjectDescription)
            .HasMaxLength(11);

        builder.Property(e => e.ControllingObjectMarkedLighted)
            .HasMaxLength(4);

        builder.Property(e => e.ControllingObjectCenterlineOffset)
            .HasMaxLength(7);

        // Decimal precision for coordinates and elevations
        builder.Property(e => e.LatDecimal)
            .HasColumnType("decimal(10,8)");

        builder.Property(e => e.LongDecimal)
            .HasColumnType("decimal(11,8)");

        builder.Property(e => e.Elevation)
            .HasColumnType("decimal(7,1)");

        builder.Property(e => e.ThresholdCrossingHeight)
            .HasColumnType("decimal(5,1)");

        builder.Property(e => e.VisualGlidePathAngle)
            .HasColumnType("decimal(4,2)");

        builder.Property(e => e.DisplacedThresholdLatDecimal)
            .HasColumnType("decimal(10,8)");

        builder.Property(e => e.DisplacedThresholdLongDecimal)
            .HasColumnType("decimal(11,8)");

        builder.Property(e => e.DisplacedThresholdElev)
            .HasColumnType("decimal(7,1)");

        builder.Property(e => e.TouchdownZoneElev)
            .HasColumnType("decimal(7,1)");

        // Relationship to Runway is configured in RunwayConfiguration

        // Unique constraint: one runway end per runway
        builder.HasIndex(re => new { re.RunwayFk, re.RunwayEndId })
            .IsUnique()
            .HasFilter("\"runway_fk\" IS NOT NULL");

        // Index for linking during data sync
        builder.HasIndex(re => new { re.SiteNo, re.RunwayIdRef, re.RunwayEndId })
            .IsUnique();

        // Query index
        builder.HasIndex(re => re.RunwayFk);
    }
}
