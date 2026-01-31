namespace VFR3D.Infrastructure.Dtos.Notam;

/// <summary>
/// Represents a point along a flight route - either an airport (by identifier) or a waypoint (by coordinates)
/// </summary>
public record RoutePointDto
{
    /// <summary>
    /// Airport identifier (ICAO or FAA). If provided, this point is an airport.
    /// </summary>
    public string? AirportIdentifier { get; init; }

    /// <summary>
    /// Optional name for waypoints (used in route description)
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Latitude in decimal degrees. Required for waypoints (when AirportIdentifier is null).
    /// </summary>
    public double? Latitude { get; init; }

    /// <summary>
    /// Longitude in decimal degrees. Required for waypoints (when AirportIdentifier is null).
    /// </summary>
    public double? Longitude { get; init; }

    /// <summary>
    /// Optional radius in nautical miles for waypoints (max 100). Ignored for airports.
    /// </summary>
    public double? RadiusNm { get; init; }

    /// <summary>
    /// Returns true if this is an airport point, false if waypoint
    /// </summary>
    public bool IsAirport => !string.IsNullOrWhiteSpace(AirportIdentifier);
}
