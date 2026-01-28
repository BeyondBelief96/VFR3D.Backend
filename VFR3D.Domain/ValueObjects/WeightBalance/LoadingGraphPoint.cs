namespace VFR3D.Domain.ValueObjects.WeightBalance;

/// <summary>
/// A point on a loading graph line, representing a weight and its corresponding
/// moment/1000 or arm value (depending on the profile's LoadingGraphFormat).
/// </summary>
public class LoadingGraphPoint
{
    /// <summary>
    /// Weight in pounds (or kg depending on profile units)
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// The corresponding value on the X-axis (Moment/1000 or Arm depending on format)
    /// </summary>
    public double Value { get; set; }
}
