namespace VFR3D.Infrastructure.Dtos.Performance;

/// <summary>
/// Request DTO for manual density altitude calculation
/// </summary>
public record DensityAltitudeRequestDto
{
    /// <summary>
    /// Field elevation in feet MSL
    /// </summary>
    public double FieldElevationFt { get; init; }

    /// <summary>
    /// Altimeter setting in inches of mercury (inHg)
    /// </summary>
    public double AltimeterInHg { get; init; }

    /// <summary>
    /// Temperature in degrees Celsius
    /// </summary>
    public double TemperatureCelsius { get; init; }
}
