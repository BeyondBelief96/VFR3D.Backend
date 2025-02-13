using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations
{
    public class FlightConfiguration : IEntityTypeConfiguration<Flight>
    {
        public void Configure(EntityTypeBuilder<Flight> builder)
        {
            builder.ToTable("flights");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Auth0UserId)
                .IsRequired()
                .HasColumnName("auth0_user_id");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");

            builder.Property(e => e.DepartureTime)
                .IsRequired()
                .HasColumnName("departure_time");

            builder.Property(e => e.PlannedCruisingAltitude)
                .IsRequired()
                .HasColumnName("planned_cruising_altitude");

            builder.Property(e => e.Waypoints)
                .HasColumnType("jsonb")
                .HasColumnName("waypoints");

            builder.Property(e => e.AircraftPerformanceId)
                .IsRequired()
                .HasColumnName("aircraft_performance_id");

            builder.Property(e => e.TotalRouteDistance)
                .HasColumnType("double precision")
                .HasColumnName("total_route_distance");

            builder.Property(e => e.TotalRouteTimeHours)
                .HasColumnType("double precision")
                .HasColumnName("total_route_time_hours");

            builder.Property(e => e.TotalFuelUsed)
                .HasColumnType("double precision")
                .HasColumnName("total_fuel_used");

            builder.Property(e => e.AverageWindComponent)
                .HasColumnType("double precision")
                .HasColumnName("average_wind_component");

            builder.Property(e => e.Legs)
                .HasColumnType("jsonb")
                .HasColumnName("legs");

            builder.Property(e => e.StateCodesAlongRoute)
                .HasColumnType("jsonb")
                .HasColumnName("state_codes_along_route");

            // Relationships
            builder.HasOne(e => e.AircraftPerformanceProfile)
                .WithMany(a => a.Flights)
                .HasForeignKey(e => e.AircraftPerformanceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(e => e.Auth0UserId);
            builder.HasIndex(e => e.AircraftPerformanceId);
            builder.HasIndex(e => new { e.Auth0UserId, e.Name });
        }
    }
}