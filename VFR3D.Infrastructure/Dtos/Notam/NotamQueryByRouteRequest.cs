namespace VFR3D.Infrastructure.Dtos.Notam;

/// <summary>
/// Request DTO for querying NOTAMs along a flight route
/// </summary>
public record NotamQueryByRouteRequest
{
    /// <summary>
    /// List of airport identifiers (ICAO codes or FAA identifiers) along the route
    /// </summary>
    public List<string> AirportIdentifiers { get; init; } = [];

    /// <summary>
    /// Optional radius in nautical miles for corridor sampling between airports.
    /// If not specified, uses default from settings.
    /// </summary>
    public double? CorridorRadiusNm { get; init; }

    /// <summary>
    /// Whether to include NOTAMs from corridor sampling points between airports
    /// </summary>
    public bool IncludeCorridorNotams { get; init; } = false;
}
