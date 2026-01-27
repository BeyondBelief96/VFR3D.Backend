using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VFR3D.Domain.Enums;
using VFR3D.Domain.ValueObjects.WeightBalance;

namespace VFR3D.Domain.Entities;

[Table("weight_balance_profiles")]
public class WeightBalanceProfile
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Column("aircraft_id")]
    public string? AircraftId { get; set; }

    [Column("profile_name")]
    [Required]
    [MaxLength(50)]
    public string ProfileName { get; set; } = string.Empty;

    [Column("datum_description")]
    [MaxLength(100)]
    public string? DatumDescription { get; set; }

    [Column("empty_weight")]
    public double EmptyWeight { get; set; }

    [Column("empty_weight_arm")]
    public double EmptyWeightArm { get; set; }

    [Column("max_ramp_weight")]
    public double? MaxRampWeight { get; set; }

    [Column("max_takeoff_weight")]
    public double MaxTakeoffWeight { get; set; }

    [Column("max_landing_weight")]
    public double? MaxLandingWeight { get; set; }

    [Column("max_zero_fuel_weight")]
    public double? MaxZeroFuelWeight { get; set; }

    [Column("weight_units")]
    public WeightUnits WeightUnits { get; set; } = WeightUnits.Pounds;

    [Column("arm_units")]
    public ArmUnits ArmUnits { get; set; } = ArmUnits.Inches;

    [Column("loading_stations", TypeName = "jsonb")]
    public List<LoadingStation> LoadingStations { get; set; } = [];

    [Column("cg_envelopes", TypeName = "jsonb")]
    public List<CgEnvelope> CgEnvelopes { get; set; } = [];

    // Navigation Properties
    public virtual Aircraft? Aircraft { get; set; }
}
