using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations;

public class AircraftConfiguration : IEntityTypeConfiguration<Aircraft>
{
    public void Configure(EntityTypeBuilder<Aircraft> builder)
    {
        builder.ToTable("aircraft");

        // Primary Key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.TailNumber)
            .IsRequired()
            .HasMaxLength(10)
            .HasColumnName("tail_number");

        builder.Property(e => e.AircraftType)
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("aircraft_type");

        builder.Property(e => e.CallSign)
            .HasMaxLength(10)
            .HasColumnName("call_sign");

        builder.Property(e => e.SerialNumber)
            .HasMaxLength(50)
            .HasColumnName("serial_number");

        builder.Property(e => e.PrimaryColor)
            .HasMaxLength(30)
            .HasColumnName("primary_color");

        builder.Property(e => e.Color2)
            .HasMaxLength(30)
            .HasColumnName("color2");

        builder.Property(e => e.Color3)
            .HasMaxLength(30)
            .HasColumnName("color3");

        builder.Property(e => e.Color4)
            .HasMaxLength(30)
            .HasColumnName("color4");

        builder.Property(e => e.Category)
            .HasColumnName("category")
            .HasConversion<string>();

        builder.Property(e => e.AircraftHome)
            .HasMaxLength(10)
            .HasColumnName("aircraft_home");

        builder.Property(e => e.AirspeedUnits)
            .HasColumnName("airspeed_units")
            .HasConversion<string>();

        builder.Property(e => e.LengthUnits)
            .HasColumnName("length_units")
            .HasConversion<string>();

        builder.Property(e => e.DefaultCruiseAltitude)
            .HasColumnName("default_cruise_altitude");

        builder.Property(e => e.MaxCeiling)
            .HasColumnName("max_ceiling");

        builder.Property(e => e.GlideSpeed)
            .HasColumnName("glide_speed");

        builder.Property(e => e.GlideRatio)
            .HasColumnType("double precision")
            .HasColumnName("glide_ratio");

        // Indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.TailNumber }).IsUnique();

        // Relationships
        builder.HasMany(e => e.PerformanceProfiles)
            .WithOne(p => p.Aircraft)
            .HasForeignKey(p => p.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Flights)
            .WithOne(f => f.Aircraft)
            .HasForeignKey(f => f.AircraftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.WeightBalanceProfiles)
            .WithOne(w => w.Aircraft)
            .HasForeignKey(w => w.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
