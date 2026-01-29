using VFR3D.Infrastructure.Dtos.Notam;

namespace VFR3D.Infrastructure.Interfaces;

/// <summary>
/// Business logic service for NOTAM queries with caching
/// </summary>
public interface INotamService
{
    /// <summary>
    /// Gets NOTAMs for a specific airport with caching
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or FAA identifier (e.g., KDFW, DFW)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response containing NOTAMs for the airport</returns>
    Task<NotamResponseDto> GetNotamsForAirportAsync(string icaoCodeOrIdent, CancellationToken ct = default);

    /// <summary>
    /// Gets NOTAMs within a radius of a geographic point with caching
    /// </summary>
    /// <param name="lat">Latitude in decimal degrees</param>
    /// <param name="lon">Longitude in decimal degrees</param>
    /// <param name="radiusNm">Radius in nautical miles (max 100)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response containing NOTAMs within the radius</returns>
    Task<NotamResponseDto> GetNotamsByRadiusAsync(double lat, double lon, double radiusNm, CancellationToken ct = default);

    /// <summary>
    /// Gets NOTAMs for a flight route (multiple airports) with caching and deduplication
    /// </summary>
    /// <param name="request">Route query request with airport identifiers</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Response containing aggregated NOTAMs for the route</returns>
    Task<NotamResponseDto> GetNotamsForRouteAsync(NotamQueryByRouteRequest request, CancellationToken ct = default);
}
