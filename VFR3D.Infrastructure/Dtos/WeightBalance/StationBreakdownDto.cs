namespace VFR3D.Infrastructure.Dtos.WeightBalance;

public record StationBreakdownDto
{
    public string StationId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public double Weight { get; init; }
    public double Arm { get; init; }
    public double Moment { get; init; }
}
