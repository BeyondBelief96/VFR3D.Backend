using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations;

public class WeightBalanceCalculationConfiguration : IEntityTypeConfiguration<WeightBalanceCalculation>
{
    public void Configure(EntityTypeBuilder<WeightBalanceCalculation> builder)
    {
        builder.ToTable("weight_balance_calculations");

        // Primary Key
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.FlightId)
            .HasColumnName("flight_id");

        builder.Property(e => e.WeightBalanceProfileId)
            .IsRequired()
            .HasColumnName("weight_balance_profile_id");

        builder.Property(e => e.EnvelopeId)
            .HasColumnName("envelope_id");

        builder.Property(e => e.FuelBurnGallons)
            .HasColumnType("double precision")
            .HasColumnName("fuel_burn_gallons");

        builder.Property(e => e.LoadedStations)
            .HasColumnType("jsonb")
            .HasColumnName("loaded_stations");

        builder.Property(e => e.TakeoffResult)
            .HasColumnType("jsonb")
            .HasColumnName("takeoff_result");

        builder.Property(e => e.LandingResult)
            .HasColumnType("jsonb")
            .HasColumnName("landing_result");

        builder.Property(e => e.StationBreakdown)
            .HasColumnType("jsonb")
            .HasColumnName("station_breakdown");

        builder.Property(e => e.EnvelopeName)
            .HasMaxLength(100)
            .HasColumnName("envelope_name");

        builder.Property(e => e.EnvelopeLimits)
            .HasColumnType("jsonb")
            .HasColumnName("envelope_limits");

        builder.Property(e => e.Warnings)
            .HasColumnType("jsonb")
            .HasColumnName("warnings");

        builder.Property(e => e.CalculatedAt)
            .HasColumnName("calculated_at");

        builder.Property(e => e.IsStandalone)
            .HasColumnName("is_standalone");

        // Indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.FlightId);
        builder.HasIndex(e => e.WeightBalanceProfileId);
        builder.HasIndex(e => new { e.UserId, e.IsStandalone });

        // Relationships

        // One-to-one with Flight (cascade delete when flight is deleted)
        builder.HasOne(e => e.Flight)
            .WithOne(f => f.WeightBalanceCalculation)
            .HasForeignKey<WeightBalanceCalculation>(e => e.FlightId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-one with WeightBalanceProfile (restrict delete to preserve history)
        builder.HasOne(e => e.WeightBalanceProfile)
            .WithMany()
            .HasForeignKey(e => e.WeightBalanceProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
