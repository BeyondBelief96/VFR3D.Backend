namespace VFR3D.Infrastructure.Dtos.Performance;

/// <summary>
/// Response DTO containing crosswind components for all runways at an airport
/// </summary>
public record AirportCrosswindResponseDto
{
    /// <summary>
    /// ICAO code or identifier of the airport
    /// </summary>
    public string AirportIdentifier { get; init; } = string.Empty;

    /// <summary>
    /// Wind direction in degrees from METAR (null if variable)
    /// </summary>
    public int? WindDirectionDegrees { get; init; }

    /// <summary>
    /// Wind speed in knots from METAR
    /// </summary>
    public int WindSpeedKt { get; init; }

    /// <summary>
    /// Wind gust speed in knots from METAR (if reported)
    /// </summary>
    public int? WindGustKt { get; init; }

    /// <summary>
    /// Whether the wind was reported as variable (VRB)
    /// </summary>
    public bool IsVariableWind { get; init; }

    /// <summary>
    /// Raw METAR text for reference
    /// </summary>
    public string? RawMetar { get; init; }

    /// <summary>
    /// METAR observation time
    /// </summary>
    public string? ObservationTime { get; init; }

    /// <summary>
    /// Crosswind components for each runway end at the airport
    /// </summary>
    public List<RunwayCrosswindComponentDto> Runways { get; init; } = new();

    /// <summary>
    /// Recommended runway end identifier (lowest crosswind with headwind)
    /// </summary>
    public string? RecommendedRunway { get; init; }
}
