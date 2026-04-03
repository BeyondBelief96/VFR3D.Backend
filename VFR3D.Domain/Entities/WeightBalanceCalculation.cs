using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VFR3D.Domain.ValueObjects.WeightBalance;

namespace VFR3D.Domain.Entities;

/// <summary>
/// Represents a persisted W&B calculation, either associated with a flight or standalone for form repopulation.
/// </summary>
[Table("weight_balance_calculations")]
public class WeightBalanceCalculation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Optional FK to Flight. When set, this is a flight-associated calculation.
    /// </summary>
    [Column("flight_id")]
    public string? FlightId { get; set; }

    /// <summary>
    /// FK to the W&B profile used for this calculation.
    /// </summary>
    [Column("weight_balance_profile_id")]
    [Required]
    public Guid WeightBalanceProfileId { get; set; }

    /// <summary>
    /// The envelope ID used for this calculation (from the profile's CgEnvelopes).
    /// </summary>
    [Column("envelope_id")]
    public string? EnvelopeId { get; set; }

    /// <summary>
    /// Fuel burn in gallons for landing calculation.
    /// </summary>
    [Column("fuel_burn_gallons")]
    public double? FuelBurnGallons { get; set; }

    /// <summary>
    /// The input stations with their loads - stored as JSONB for form repopulation.
    /// </summary>
    [Column("loaded_stations", TypeName = "jsonb")]
    public List<StationLoad> LoadedStations { get; set; } = [];

    /// <summary>
    /// Takeoff CG calculation result.
    /// </summary>
    [Column("takeoff_result", TypeName = "jsonb")]
    public WeightBalanceCgResult TakeoffResult { get; set; } = new();

    /// <summary>
    /// Landing CG calculation result (if fuel burn was provided).
    /// </summary>
    [Column("landing_result", TypeName = "jsonb")]
    public WeightBalanceCgResult? LandingResult { get; set; }

    /// <summary>
    /// Per-station calculation details.
    /// </summary>
    [Column("station_breakdown", TypeName = "jsonb")]
    public List<StationBreakdown> StationBreakdown { get; set; } = [];

    /// <summary>
    /// Name of the envelope used for this calculation.
    /// </summary>
    [Column("envelope_name")]
    [MaxLength(100)]
    public string EnvelopeName { get; set; } = string.Empty;

    /// <summary>
    /// Envelope limit points at time of calculation (stored for display).
    /// </summary>
    [Column("envelope_limits", TypeName = "jsonb")]
    public List<CgEnvelopePoint> EnvelopeLimits { get; set; } = [];

    /// <summary>
    /// Warnings generated during the calculation.
    /// </summary>
    [Column("warnings", TypeName = "jsonb")]
    public List<string> Warnings { get; set; } = [];

    /// <summary>
    /// When this calculation was performed.
    /// </summary>
    [Column("calculated_at")]
    public DateTime CalculatedAt { get; set; }

    /// <summary>
    /// True if this is a standalone calculation (not associated with a flight).
    /// Used to find the user's most recent standalone calculation for form repopulation.
    /// </summary>
    [Column("is_standalone")]
    public bool IsStandalone { get; set; }

    // Navigation Properties
    public virtual Flight? Flight { get; set; }
    public virtual WeightBalanceProfile? WeightBalanceProfile { get; set; }
}
