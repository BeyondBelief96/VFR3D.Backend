namespace VFR3D.Infrastructure.Dtos.Performance;

/// <summary>
/// Response DTO for density altitude calculation
/// </summary>
public record DensityAltitudeResponseDto
{
    /// <summary>
    /// Airport identifier (for airport-based calculations)
    /// </summary>
    public string? AirportIdentifier { get; init; }

    /// <summary>
    /// Field elevation in feet MSL
    /// </summary>
    public double FieldElevationFt { get; init; }

    /// <summary>
    /// Pressure altitude in feet
    /// </summary>
    public double PressureAltitudeFt { get; init; }

    /// <summary>
    /// Density altitude in feet
    /// </summary>
    public double DensityAltitudeFt { get; init; }

    /// <summary>
    /// Standard (ISA) temperature at this pressure altitude in Celsius
    /// </summary>
    public double IsaTemperatureCelsius { get; init; }

    /// <summary>
    /// Actual temperature in Celsius
    /// </summary>
    public double ActualTemperatureCelsius { get; init; }

    /// <summary>
    /// Temperature deviation from ISA in Celsius
    /// </summary>
    public double TemperatureDeviationCelsius { get; init; }

    /// <summary>
    /// Altimeter setting used in calculation (inHg)
    /// </summary>
    public double AltimeterInHg { get; init; }

    /// <summary>
    /// Raw METAR text for reference (for airport-based calculations)
    /// </summary>
    public string? RawMetar { get; init; }

    /// <summary>
    /// METAR observation time (for airport-based calculations)
    /// </summary>
    public string? ObservationTime { get; init; }
}
