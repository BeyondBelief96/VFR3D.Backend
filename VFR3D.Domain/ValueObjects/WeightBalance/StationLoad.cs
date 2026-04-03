namespace VFR3D.Domain.ValueObjects.WeightBalance;

/// <summary>
/// Represents user input for a loading station in a W&B calculation.
/// </summary>
public class StationLoad
{
    public string StationId { get; set; } = string.Empty;
    public double? Weight { get; set; }        // For Standard stations
    public double? FuelGallons { get; set; }   // For Fuel stations
    public double? OilQuarts { get; set; }     // For Oil stations
}
