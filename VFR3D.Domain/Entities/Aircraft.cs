using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VFR3D.Domain.Enums;

namespace VFR3D.Domain.Entities;

[Table("aircraft")]
public class Aircraft
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Column("tail_number")]
    [Required]
    [MaxLength(10)]
    public string TailNumber { get; set; } = string.Empty;

    [Column("aircraft_type")]
    [Required]
    [MaxLength(20)]
    public string AircraftType { get; set; } = string.Empty;

    [Column("call_sign")]
    [MaxLength(10)]
    public string? CallSign { get; set; }

    [Column("serial_number")]
    [MaxLength(50)]
    public string? SerialNumber { get; set; }

    [Column("primary_color")]
    [MaxLength(30)]
    public string? PrimaryColor { get; set; }

    [Column("color2")]
    [MaxLength(30)]
    public string? Color2 { get; set; }

    [Column("color3")]
    [MaxLength(30)]
    public string? Color3 { get; set; }

    [Column("color4")]
    [MaxLength(30)]
    public string? Color4 { get; set; }

    [Column("category")]
    public AircraftCategory Category { get; set; }

    [Column("aircraft_home")]
    [MaxLength(10)]
    public string? AircraftHome { get; set; }

    [Column("airspeed_units")]
    public AirspeedUnits AirspeedUnits { get; set; } = AirspeedUnits.Knots;

    [Column("length_units")]
    public LengthUnits LengthUnits { get; set; } = LengthUnits.Feet;

    [Column("default_cruise_altitude")]
    public int? DefaultCruiseAltitude { get; set; }

    [Column("max_ceiling")]
    public int? MaxCeiling { get; set; }

    [Column("glide_speed")]
    public int? GlideSpeed { get; set; }

    [Column("glide_ratio")]
    public double? GlideRatio { get; set; }

    // Navigation Properties
    public virtual ICollection<AircraftPerformanceProfile> PerformanceProfiles { get; set; } = new List<AircraftPerformanceProfile>();
    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}
