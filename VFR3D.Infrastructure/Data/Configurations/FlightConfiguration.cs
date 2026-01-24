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

            builder.Property(e => e.AirspaceGlobalIds)
                .HasColumnType("jsonb")
                .HasColumnName("airspace_global_ids");

            builder.Property(e => e.SpecialUseAirspaceGlobalIds)
                .HasColumnType("jsonb")
                .HasColumnName("special_use_airspace_global_ids");

            // Relationships
            builder.HasOne(e => e.AircraftPerformanceProfile)
                .WithMany(a => a.Flights)
                .HasForeignKey(e => e.AircraftPerformanceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-many: flights <-> airspaces via global_id
            builder
                .HasMany(f => f.Airspaces)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "flight_airspaces",
                    right => right.HasOne<Airspace>()
                                 .WithMany()
                                 .HasForeignKey("airspace_global_id")
                                 .HasPrincipalKey(nameof(Airspace.GlobalId))
                                 .OnDelete(DeleteBehavior.Cascade),
                    left => left.HasOne<Flight>()
                                .WithMany()
                                .HasForeignKey("flight_id")
                                .HasPrincipalKey(nameof(Flight.Id))
                                .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("flight_airspaces");
                        join.Property<string>("flight_id");
                        join.Property<string>("airspace_global_id");
                        join.HasKey("flight_id", "airspace_global_id");
                        join.HasIndex("airspace_global_id");
                    });

            // Many-to-many: flights <-> special_use_airspaces via global_id
            builder
                .HasMany(f => f.SpecialUseAirspaces)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "flight_special_use_airspaces",
                    right => right.HasOne<SpecialUseAirspace>()
                                 .WithMany()
                                 .HasForeignKey("special_use_airspace_global_id")
                                 .HasPrincipalKey(nameof(SpecialUseAirspace.GlobalId))
                                 .OnDelete(DeleteBehavior.Cascade),
                    left => left.HasOne<Flight>()
                                .WithMany()
                                .HasForeignKey("flight_id")
                                .HasPrincipalKey(nameof(Flight.Id))
                                .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("flight_special_use_airspaces");
                        join.Property<string>("flight_id");
                        join.Property<string>("special_use_airspace_global_id");
                        join.HasKey("flight_id", "special_use_airspace_global_id");
                        join.HasIndex("special_use_airspace_global_id");
                    });

            // Many-to-many: flights <-> obstacles via oas_number
            builder
                .HasMany(f => f.Obstacles)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "flight_obstacles",
                    right => right.HasOne<Obstacle>()
                                 .WithMany()
                                 .HasForeignKey("obstacle_oas_number")
                                 .HasPrincipalKey(nameof(Obstacle.OasNumber))
                                 .OnDelete(DeleteBehavior.Cascade),
                    left => left.HasOne<Flight>()
                                .WithMany()
                                .HasForeignKey("flight_id")
                                .HasPrincipalKey(nameof(Flight.Id))
                                .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.ToTable("flight_obstacles");
                        join.Property<string>("flight_id");
                        join.Property<string>("obstacle_oas_number");
                        join.HasKey("flight_id", "obstacle_oas_number");
                        join.HasIndex("obstacle_oas_number");
                    });

            builder.Property(e => e.ObstacleOasNumbers)
                .HasColumnType("jsonb")
                .HasColumnName("obstacle_oas_numbers");

            // Indexes
            builder.HasIndex(e => e.Auth0UserId);
            builder.HasIndex(e => e.AircraftPerformanceId);
            builder.HasIndex(e => new { e.Auth0UserId, e.Name });
        }
    }
}