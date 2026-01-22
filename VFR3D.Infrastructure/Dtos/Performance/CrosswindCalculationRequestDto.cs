namespace VFR3D.Infrastructure.Dtos.Performance;

/// <summary>
/// Request DTO for manual crosswind calculation with user-provided parameters
/// </summary>
public record CrosswindCalculationRequestDto
{
    /// <summary>
    /// Wind direction in degrees (0-360, or null for variable)
    /// </summary>
    public int? WindDirectionDegrees { get; init; }

    /// <summary>
    /// Wind speed in knots
    /// </summary>
    public int WindSpeedKt { get; init; }

    /// <summary>
    /// Wind gust speed in knots (optional)
    /// </summary>
    public int? WindGustKt { get; init; }

    /// <summary>
    /// Runway heading in magnetic degrees (0-360)
    /// </summary>
    public int RunwayHeadingDegrees { get; init; }
}
