namespace VFR3D.Infrastructure.Dtos.WeightBalance;

/// <summary>
/// Request to calculate and persist a W&B calculation.
/// </summary>
public record SaveWeightBalanceCalculationRequestDto
{
    /// <summary>
    /// The W&B profile ID to use for the calculation.
    /// </summary>
    public Guid ProfileId { get; init; }

    /// <summary>
    /// Optional flight ID to associate the calculation with.
    /// If not provided, this will be a standalone calculation for form repopulation.
    /// </summary>
    public string? FlightId { get; init; }

    /// <summary>
    /// Optional envelope ID from the profile. If not provided, uses the first envelope.
    /// </summary>
    public string? EnvelopeId { get; init; }

    /// <summary>
    /// Optional fuel burn in gallons for landing calculation.
    /// </summary>
    public double? FuelBurnGallons { get; init; }

    /// <summary>
    /// The stations with their loads.
    /// </summary>
    public List<StationLoadDto> LoadedStations { get; init; } = [];
}
