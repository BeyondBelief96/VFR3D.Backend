namespace VFR3D.Infrastructure.Dtos.Performance;

/// <summary>
/// Crosswind component data for a single runway end
/// </summary>
public record RunwayCrosswindComponentDto
{
    /// <summary>
    /// Runway end identifier (e.g., "09", "27L")
    /// </summary>
    public string RunwayEndId { get; init; } = string.Empty;

    /// <summary>
    /// Runway heading in magnetic degrees
    /// </summary>
    public int MagneticHeadingDegrees { get; init; }

    /// <summary>
    /// Crosswind component in knots. Positive = from the right, Negative = from the left
    /// </summary>
    public double CrosswindKt { get; init; }

    /// <summary>
    /// Headwind component in knots. Positive = headwind, Negative = tailwind
    /// </summary>
    public double HeadwindKt { get; init; }

    /// <summary>
    /// Crosswind component from gusts in knots (if gust data available). Positive = from the right
    /// </summary>
    public double? GustCrosswindKt { get; init; }

    /// <summary>
    /// Headwind component from gusts in knots (if gust data available). Positive = headwind
    /// </summary>
    public double? GustHeadwindKt { get; init; }

    /// <summary>
    /// Absolute value of crosswind component (for comparison purposes)
    /// </summary>
    public double AbsoluteCrosswindKt { get; init; }

    /// <summary>
    /// Whether this runway has a headwind (favorable) vs tailwind
    /// </summary>
    public bool HasHeadwind { get; init; }
}
