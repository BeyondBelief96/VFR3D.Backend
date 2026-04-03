using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VFR3D.Domain.Entities;

[Table("aircraft_performance_profiles")]
public class AircraftPerformanceProfile
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = string.Empty;

    [Column("user_id")]
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Column("profile_name")]
    [Required]
    [MaxLength(50)]
    public string ProfileName { get; set; } = string.Empty;

    [Column("climb_true_airspeed")]
    public int ClimbTrueAirspeed { get; set; }

    [Column("cruise_true_airspeed")]
    public int CruiseTrueAirspeed { get; set; }

    [Column("cruise_fuel_burn")]
    public double CruiseFuelBurn { get; set; }

    [Column("climb_fuel_burn")]
    public double ClimbFuelBurn { get; set; }

    [Column("descent_fuel_burn")]
    public double DescentFuelBurn { get; set; }

    [Column("climb_fpm")]
    public int ClimbFpm { get; set; }

    [Column("descent_fpm")]
    public int DescentFpm { get; set; }

    [Column("descent_true_airspeed")]
    public int DescentTrueAirspeed { get; set; }

    [Column("stt_fuel_gals")]
    public double SttFuelGals { get; set; }

    [Column("fuel_on_board_gals")]
    public double FuelOnBoardGals { get; set; }

    [Column("aircraft_id")]
    public string? AircraftId { get; set; }

    // Navigation Properties
    public virtual Aircraft? Aircraft { get; set; }
    public virtual ICollection<Flight> Flights { get; set; } = new List<Flight>();
}