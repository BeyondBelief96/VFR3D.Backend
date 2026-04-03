namespace VFR3D.Infrastructure.Dtos.Performance;

/// <summary>
/// Response DTO for a single crosswind calculation
/// </summary>
public record CrosswindCalculationResponseDto
{
    /// <summary>
    /// Crosswind component in knots. Positive = from the right, Negative = from the left
    /// </summary>
    public double CrosswindKt { get; init; }

    /// <summary>
    /// Headwind component in knots. Positive = headwind, Negative = tailwind
    /// </summary>
    public double HeadwindKt { get; init; }

    /// <summary>
    /// Crosswind component from gusts in knots (if gust data provided). Positive = from the right
    /// </summary>
    public double? GustCrosswindKt { get; init; }

    /// <summary>
    /// Headwind component from gusts in knots (if gust data provided). Positive = headwind
    /// </summary>
    public double? GustHeadwindKt { get; init; }

    /// <summary>
    /// Wind direction used in calculation (degrees)
    /// </summary>
    public int? WindDirectionDegrees { get; init; }

    /// <summary>
    /// Wind speed used in calculation (knots)
    /// </summary>
    public int WindSpeedKt { get; init; }

    /// <summary>
    /// Wind gust speed used in calculation (knots)
    /// </summary>
    public int? WindGustKt { get; init; }

    /// <summary>
    /// Runway heading used in calculation (degrees)
    /// </summary>
    public int RunwayHeadingDegrees { get; init; }

    /// <summary>
    /// Whether the wind was reported as variable (VRB)
    /// </summary>
    public bool IsVariableWind { get; init; }
}
