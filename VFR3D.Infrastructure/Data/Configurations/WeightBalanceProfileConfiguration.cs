using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations;

public class WeightBalanceProfileConfiguration : IEntityTypeConfiguration<WeightBalanceProfile>
{
    public void Configure(EntityTypeBuilder<WeightBalanceProfile> builder)
    {
        builder.ToTable("weight_balance_profiles");

        // Primary Key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.AircraftId)
            .HasColumnName("aircraft_id");

        builder.Property(e => e.ProfileName)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("profile_name");

        builder.Property(e => e.DatumDescription)
            .HasMaxLength(100)
            .HasColumnName("datum_description");

        builder.Property(e => e.EmptyWeight)
            .HasColumnType("double precision")
            .HasColumnName("empty_weight");

        builder.Property(e => e.EmptyWeightArm)
            .HasColumnType("double precision")
            .HasColumnName("empty_weight_arm");

        builder.Property(e => e.MaxRampWeight)
            .HasColumnType("double precision")
            .HasColumnName("max_ramp_weight");

        builder.Property(e => e.MaxTakeoffWeight)
            .HasColumnType("double precision")
            .HasColumnName("max_takeoff_weight");

        builder.Property(e => e.MaxLandingWeight)
            .HasColumnType("double precision")
            .HasColumnName("max_landing_weight");

        builder.Property(e => e.MaxZeroFuelWeight)
            .HasColumnType("double precision")
            .HasColumnName("max_zero_fuel_weight");

        builder.Property(e => e.WeightUnits)
            .HasColumnName("weight_units")
            .HasConversion<string>();

        builder.Property(e => e.ArmUnits)
            .HasColumnName("arm_units")
            .HasConversion<string>();

        builder.Property(e => e.LoadingStations)
            .HasColumnType("jsonb")
            .HasColumnName("loading_stations");

        builder.Property(e => e.CgEnvelopes)
            .HasColumnType("jsonb")
            .HasColumnName("cg_envelopes");

        // Indexes
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.AircraftId);
        builder.HasIndex(e => new { e.UserId, e.ProfileName }).IsUnique();

        // Relationships
        builder.HasOne(e => e.Aircraft)
            .WithMany(a => a.WeightBalanceProfiles)
            .HasForeignKey(e => e.AircraftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
