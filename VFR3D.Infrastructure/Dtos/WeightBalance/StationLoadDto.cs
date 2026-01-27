namespace VFR3D.Infrastructure.Dtos.WeightBalance;

public record StationLoadDto
{
    public string StationId { get; init; } = string.Empty;
    public double? Weight { get; init; }
    public double? FuelGallons { get; init; }
}
