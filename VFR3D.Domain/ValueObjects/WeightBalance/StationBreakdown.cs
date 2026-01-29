namespace VFR3D.Domain.ValueObjects.WeightBalance;

/// <summary>
/// Represents the calculated breakdown for a single station in a W&B calculation.
/// </summary>
public class StationBreakdown
{
    public string StationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Weight { get; set; }
    public double Arm { get; set; }
    public double Moment { get; set; }
}
