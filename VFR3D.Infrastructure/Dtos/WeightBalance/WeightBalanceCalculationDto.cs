namespace VFR3D.Infrastructure.Dtos.WeightBalance;

/// <summary>
/// Full W&B calculation response including inputs and outputs.
/// </summary>
public record WeightBalanceCalculationDto
{
    public Guid Id { get; init; }
    public Guid ProfileId { get; init; }
    public string? FlightId { get; init; }
    public string? EnvelopeId { get; init; }
    public double? FuelBurnGallons { get; init; }

    /// <summary>
    /// The input stations with their loads.
    /// </summary>
    public List<StationLoadDto> LoadedStations { get; init; } = [];

    /// <summary>
    /// Takeoff CG calculation result.
    /// </summary>
    public WeightBalanceCgResultDto Takeoff { get; init; } = new();

    /// <summary>
    /// Landing CG calculation result (if fuel burn was provided).
    /// </summary>
    public WeightBalanceCgResultDto? Landing { get; init; }

    /// <summary>
    /// Per-station calculation details.
    /// </summary>
    public List<StationBreakdownDto> StationBreakdown { get; init; } = [];

    /// <summary>
    /// Name of the envelope used for this calculation.
    /// </summary>
    public string EnvelopeName { get; init; } = string.Empty;

    /// <summary>
    /// Envelope limit points.
    /// </summary>
    public List<CgEnvelopePointDto> EnvelopeLimits { get; init; } = [];

    /// <summary>
    /// Warnings generated during the calculation.
    /// </summary>
    public List<string> Warnings { get; init; } = [];

    /// <summary>
    /// When this calculation was performed.
    /// </summary>
    public DateTime CalculatedAt { get; init; }

    /// <summary>
    /// True if this is a standalone calculation (not associated with a flight).
    /// </summary>
    public bool IsStandalone { get; init; }
}
