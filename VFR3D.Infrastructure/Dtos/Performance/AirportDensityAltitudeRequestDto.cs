namespace VFR3D.Infrastructure.Dtos.Performance;

/// <summary>
/// Request DTO for airport-based density altitude with optional parameter overrides
/// </summary>
public record AirportDensityAltitudeRequestDto
{
    /// <summary>
    /// Override temperature in Celsius (uses METAR if not provided)
    /// </summary>
    public double? TemperatureCelsiusOverride { get; init; }

    /// <summary>
    /// Override altimeter setting in inHg (uses METAR if not provided)
    /// </summary>
    public double? AltimeterInHgOverride { get; init; }
}
