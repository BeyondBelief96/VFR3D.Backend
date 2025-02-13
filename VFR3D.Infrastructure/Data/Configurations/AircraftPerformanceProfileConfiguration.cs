using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations;

public class AircraftPerformanceProfileConfiguration : IEntityTypeConfiguration<AircraftPerformanceProfile>
{
    public void Configure(EntityTypeBuilder<AircraftPerformanceProfile> builder)
    {
        builder.ToTable("aircraft_performance");

        // Primary Key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.ProfileName)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("profile_name");

        builder.Property(e => e.ClimbTrueAirspeed)
            .HasColumnName("climb_true_airspeed");

        builder.Property(e => e.CruiseTrueAirspeed)
            .HasColumnName("cruise_true_airspeed");

        builder.Property(e => e.CruiseFuelBurn)
            .HasColumnType("double precision")
            .HasColumnName("cruise_fuel_burn");

        builder.Property(e => e.ClimbFuelBurn)
            .HasColumnType("double precision")
            .HasColumnName("climb_fuel_burn");

        builder.Property(e => e.DescentFuelBurn)
            .HasColumnType("double precision")
            .HasColumnName("descent_fuel_burn");

        builder.Property(e => e.ClimbFpm)
            .HasColumnName("climb_fpm");

        builder.Property(e => e.DescentFpm)
            .HasColumnName("descent_fpm");

        builder.Property(e => e.DescentTrueAirspeed)
            .HasColumnName("descent_true_airspeed");

        builder.Property(e => e.SttFuelGals)
            .HasColumnType("double precision")
            .HasColumnName("stt_fuel_gals");

        builder.Property(e => e.FuelOnBoardGals)
            .HasColumnType("double precision")
            .HasColumnName("fuel_on_board_gals");

        // Indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => new { e.UserId, e.ProfileName }).IsUnique();

        // Relationships
        builder.HasMany(e => e.Flights)
            .WithOne(f => f.AircraftPerformanceProfile)
            .HasForeignKey(f => f.AircraftPerformanceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}