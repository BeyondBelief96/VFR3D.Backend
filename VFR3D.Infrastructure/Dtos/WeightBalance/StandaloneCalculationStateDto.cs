namespace VFR3D.Infrastructure.Dtos.WeightBalance;

/// <summary>
/// Lightweight DTO for form repopulation with the user's last standalone calculation state.
/// </summary>
public record StandaloneCalculationStateDto
{
    public Guid CalculationId { get; init; }
    public Guid ProfileId { get; init; }
    public string? EnvelopeId { get; init; }
    public double? FuelBurnGallons { get; init; }

    /// <summary>
    /// The input stations with their loads for form repopulation.
    /// </summary>
    public List<StationLoadDto> LoadedStations { get; init; } = [];

    /// <summary>
    /// When this calculation was performed.
    /// </summary>
    public DateTime CalculatedAt { get; init; }
}
