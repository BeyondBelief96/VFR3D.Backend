using VFR3D.Infrastructure.Dtos.Notam;

namespace VFR3D.Infrastructure.Interfaces;

/// <summary>
/// Low-level client for FAA NMS (NOTAM Management System) API
/// Handles OAuth2 authentication and HTTP calls
/// </summary>
public interface INmsApiClient
{
    /// <summary>
    /// Gets NOTAMs for a specific location (airport identifier)
    /// </summary>
    /// <param name="location">ICAO code or FAA identifier (e.g., KDFW, DFW)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of NOTAMs for the location</returns>
    Task<List<NotamDto>> GetNotamsByLocationAsync(string location, CancellationToken ct = default);

    /// <summary>
    /// Gets NOTAMs within a radius of a geographic point
    /// </summary>
    /// <param name="lat">Latitude in decimal degrees</param>
    /// <param name="lon">Longitude in decimal degrees</param>
    /// <param name="radiusNm">Radius in nautical miles</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of NOTAMs within the radius</returns>
    Task<List<NotamDto>> GetNotamsByRadiusAsync(double lat, double lon, double radiusNm, CancellationToken ct = default);
}
