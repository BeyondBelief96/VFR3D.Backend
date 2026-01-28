using VFR3D.Domain.Enums;

namespace VFR3D.Domain.ValueObjects.WeightBalance;

public class LoadingStation
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double MaxWeight { get; set; }

    /// <summary>
    /// First point on the loading graph line for this station.
    /// Used with Point2 to define the line for interpolation.
    /// </summary>
    public LoadingGraphPoint Point1 { get; set; } = new();

    /// <summary>
    /// Second point on the loading graph line for this station.
    /// Used with Point1 to define the line for interpolation.
    /// </summary>
    public LoadingGraphPoint Point2 { get; set; } = new();

    public LoadingStationType StationType { get; set; } = LoadingStationType.Standard;

    // Fuel station properties (when StationType == Fuel)
    public double? FuelCapacityGallons { get; set; }
    public double? FuelWeightPerGallon { get; set; }  // Typically 6.0 for avgas

    // Oil station properties (when StationType == Oil)
    public double? OilCapacityQuarts { get; set; }
    public double? OilWeightPerQuart { get; set; }    // Typically ~1.875 for aviation oil

    /// <summary>
    /// Interpolates the loading graph to get the value (arm or moment/1000) for a given weight.
    /// </summary>
    /// <param name="weight">The weight to interpolate for</param>
    /// <returns>The interpolated value (arm or moment/1000 depending on profile format)</returns>
    public double InterpolateValue(double weight)
    {
        // Handle edge case where both points have same weight (avoid division by zero)
        if (Math.Abs(Point2.Weight - Point1.Weight) < 0.0001)
        {
            return Point1.Value;
        }

        // Linear interpolation: value = point1.value + slope * (weight - point1.weight)
        var slope = (Point2.Value - Point1.Value) / (Point2.Weight - Point1.Weight);
        return Point1.Value + slope * (weight - Point1.Weight);
    }
}
